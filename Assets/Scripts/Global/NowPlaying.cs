using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS0162

public class NowPlaying : Singleton<NowPlaying>
{
    public static Scene CurrentScene;

    [Header( "Song" )]
    public ReadOnlyCollection<Song> Songs { get; private set; }
    public static Song CurrentSong { get; private set; }
    public static int CurrentIndex { get; private set; }
    public static int KeyCount     { get; private set; }
    public static int TotalJudge   { get; private set; }
    public static int TotalNote    { get; private set; }
    public static int TotalSlider  { get; private set; }
    public static double MainBPM   { get; private set; }

    [Header( "Time" )]
    public  static double Playback { get; private set; }
    public  static double Distance { get; private set; }
    private static double DistanceCache;

    private static readonly double AudioLeadIn   = 3000d;
    private static readonly double WaitPauseTime = 2000d;

    private static double SaveTime;
    private int bpmIndex;

    [Header( "BGM" )]
    private int bgmIndex;

    [Header( "Thread" )]
    public  static ReadOnlyCollection<Note>[] Notes { get; private set; } // 레인별로 분할된 노트 데이터
    private static List<KeySound> Samples;                                // 잘린 노트의 키음등이 추가된 BGM 리스트

    private Task timeTask;
    private CancellationTokenSource breakPoint = new();
    public static Action OnUpdateThread;

    [DllImport( "Kernel32.dll" )]
    private static extern bool QueryPerformanceCounter( out long lpPerformanceCount );

    [DllImport( "Kernel32.dll" )]
    private static extern bool QueryPerformanceFrequency( out long lpFrequency );

    //
    public static bool IsStart { get; private set; }
    public static bool IsLoaded { get; private set; }

    // Initialize Event
    public static event Action OnPreInit;     // Main  Thread
    public static event Action OnPostInit;    // Main  Thread
    public static event Action OnAsyncInit;   // Other Thread
    public static event Action OnUpdateInThread;

    // Game Event
    public static event Action OnGameStart;
    public static event Action OnGameOver;
    public static event Action OnClear;
    public static event Action OnRelease;
    public static event Action<bool> OnPause;

    protected override void Awake()
    {
        base.Awake();
        // 싱글톤 활성화
        Config        config        = Config.Inst;
        GameSetting   gameSetting   = GameSetting.Inst;
        SystemSetting systemSetting = SystemSetting.Inst;
        Network       network       = Network.Inst;
        AudioManager  audioManager  = AudioManager.Inst;
        InputManager  inputManager  = InputManager.Inst;
        DataStorage   dataStorage   = DataStorage.Inst;
        Judgement     judgement     = Judgement.Inst;

        DataStorage.Inst.LoadSongs();
        Songs = DataStorage.OriginSongs;
        
        if ( Songs.Count > 0 )
             UpdateSong( 0 );
    }

    private async void OnApplicationQuit()
    {
        await ThreadCancel();
    }

    /// <summary> 게임 시작할 때마다 초기화 </summary>
    public void Initialize()
    {
        MainBPM = CurrentSong.mainBPM * GameSetting.CurrentPitch;

        // 모드를 선택한 상태로 InGame 진입 후 계산
        bool isNoSlider = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        bool isConvert  = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) &&  CurrentSong.keyCount == 7;
        KeyCount        = isConvert  ? 6 : CurrentSong.keyCount;
        TotalNote       = isConvert  ? CurrentSong.noteCount - CurrentSong.delNoteCount : CurrentSong.noteCount;
        TotalSlider     = isNoSlider ? 0 :
                          isConvert  ? CurrentSong.sliderCount - CurrentSong.delSliderCount : 
                                       CurrentSong.sliderCount;
        TotalNote       = isNoSlider ? TotalNote + TotalSlider : TotalNote;
        TotalJudge      = TotalNote + ( TotalSlider * 2 );

        // 선 파싱
        Notes = new ReadOnlyCollection<Note>[KeyCount];
        DataStorage.Inst.LoadChart();

        // 후 계산
        Samples = DataStorage.Samples.ToList(); // 원본 리스트 받아오기
        OnPreInit?.Invoke(); // 객체 생성 등의 초기화( 채보 선택 후 한번만 실행 )
    }

    public async Task Load()
    {
        // Other Thread Loading
        Task task = Task.Run( () =>
        {
            // 키음 곡이 아닌 경우 프리뷰 음악을 재생한다.
            // 하나의 음악을 메인으로 재생하지만, Clap 등 자잘한 키음이 들어간 경우도 있다.
            if ( !CurrentSong.isOnlyKeySound )
                  Samples.Add( new KeySound( GameSetting.SoundOffset, CurrentSong.audioName, 1f ) );

            for ( int i = 0; i < Samples.Count; i++ )
            {
                DataStorage.Inst.LoadSound( Samples[i].name );
            }

            DivideNotes();
            OnAsyncInit?.Invoke();
        } );

        // Main  Thread Loading
        OnPostInit?.Invoke();                                
        await task;

        // 특정모드 선택으로 잘린 키음이 추가될 수 있다.( 시간 오름차순 정렬 )
        Samples.Sort( delegate ( KeySound _left, KeySound _right )
        {
            if      ( _left.time > _right.time ) return 1;
            else if ( _left.time < _right.time ) return -1;
            else                                 return 0;
        } );

        // 기본 변수 초기화
        Clear();
        IsLoaded = true;
    }

    private void TimeUpdate( long _targetFrame, CancellationToken _token )
    {
        QueryPerformanceFrequency( out long frequency );
        QueryPerformanceCounter( out long start );

        long end         = 0;
        long startTime   = start;
        long targetTicks = frequency / _targetFrame; // 1 seconds = 10,000,000 ticks
        SpinWait spinner = new SpinWait();
        var timings      = DataStorage.Timings;

        Debug.Log( $"Time Thread Start( {_targetFrame} Frame, {targetTicks} ticks )" );
        while ( true )
        {
            _token.ThrowIfCancellationRequested();

            QueryPerformanceCounter( out end );
            if ( targetTicks <= ( end - start ) )
            {
                // 시간 갱신
                Playback = SaveTime + ( ( double )( end - startTime ) / frequency * 1000d ); // ms
                // 이동된 거리 계산
                for ( int i = bpmIndex; i < timings.Count; i++ )
                {
                    double time = timings[i].time;
                    double bpm  = timings[i].bpm / MainBPM;

                    // 지나간 타이밍에 대한 거리
                    if ( i + 1 < timings.Count && timings[i + 1].time < Playback )
                    {
                        bpmIndex += 1;
                        DistanceCache += bpm * ( timings[i + 1].time - time );
                        break;
                    }

                    // 이전 타이밍까지의 거리( 캐싱 ) + 현재 타이밍에 대한 거리
                    Distance = DistanceCache + ( bpm * ( Playback - time ) );
                    break;
                }

                // 배경음 처리( 시간의 흐름에 따라 자동재생 )
                while ( bgmIndex < Samples.Count && Samples[bgmIndex].time <= Playback )
                {
                    if ( DataStorage.Inst.GetSound( Samples[bgmIndex].name, out FMOD.Sound sound ) )
                         AudioManager.Inst.Play( sound, Samples[bgmIndex].volume );

                    bgmIndex += 1;
                }

                OnUpdateInThread?.Invoke();
                QueryPerformanceCounter( out start );
            }
            else
            {
                //System.Threading.Thread.SpinWait( 1 );
                spinner.SpinOnce();
                spinner.Reset();
            }
        }
    }

    private async Task ThreadCancel()
    {
        breakPoint?.Cancel();
        try
        {
            if ( timeTask is not null )
                 await timeTask;
        }
        catch ( OperationCanceledException )
        {
            Debug.Log( "Time Thread Cancel Completed" );
        }
        finally
        {
            breakPoint?.Dispose();
            breakPoint = null;
            timeTask   = null;
        }
    }

    private void DivideNotes()
    {
        bool isNoSlider = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        bool isConvert  = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) &&  CurrentSong.keyCount == 7;

        ReadOnlyCollection<Note> datas = DataStorage.Notes;
        List<int/* lane */> emptyLanes = new List<int>( KeyCount );
        List<Note>[] notes             = Array.ConvertAll( new int[KeyCount], _ => new List<Note>() );
        System.Random random           = new System.Random( ( int )DateTime.Now.Ticks );
        double[] prevTimes             = Enumerable.Repeat( double.MinValue, KeyCount ).ToArray();
        double   secondPerBeat         = ( ( ( 60d / CurrentSong.mainBPM ) * 4d ) / 32d );
        for ( int i = 0; i < datas.Count; i++ )
        {
            Note newNote = datas[i];
            
            newNote.isSlider = isNoSlider ? false : newNote.isSlider;
            if ( isConvert )
            {
                if ( newNote.lane == 3 )
                { 
                    // 잘린 노트의 키음은 자동재생되도록 한다.
                    DataStorage.Inst.LoadSound( newNote.keySound.name );
                    Samples.Add( newNote.keySound );
                    continue;
                }
                else if ( newNote.lane > 3 )
                {
                    // 제외된 중앙노트보다 우측의 노트는 한칸 이동시킨다.
                    newNote.lane -= 1;
                }
            }

            switch ( GameSetting.CurrentRandom )
            {
                // 레인 인덱스와 동일한 번호에 노트 분배
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Basic_Random:
                case GameRandom.Half_Random:
                {
                    newNote.distance    = GetDistance( newNote.time );
                    newNote.endDistance = GetDistance( newNote.endTime );

                    DataStorage.Inst.LoadSound( newNote.keySound.name );
                    notes[newNote.lane].Add( newNote );
                }
                break;

                // 맥스랜덤은 무작위 레인에 노트를 배치한다.
                case GameRandom.Max_Random:
                {
                    emptyLanes.Clear();
                    // 빠른계단, 즈레 등 고밀도로 배치될 때 보정
                    for ( int j = 0; j < KeyCount; j++ )
                    {
                        if ( secondPerBeat < ( newNote.time - prevTimes[j] ) )
                             emptyLanes.Add( j );
                    }

                    // 자리가 없을 때 보정되지않은 상태로 배치
                    if ( emptyLanes.Count == 0 )
                    {
                        for ( int j = 0; j < KeyCount; j++ )
                        {
                            if ( prevTimes[j] < newNote.time )
                                 emptyLanes.Add( j );
                        }
                    }

                    int selectLane        = emptyLanes[random.Next( 0, int.MaxValue ) % emptyLanes.Count];
                    prevTimes[selectLane] = newNote.isSlider ? newNote.endTime : newNote.time;

                    newNote.distance    = GetDistance( newNote.time );
                    newNote.endDistance = GetDistance( newNote.endTime );

                    DataStorage.Inst.LoadSound( newNote.keySound.name );
                    notes[selectLane].Add( newNote );
                }
                break;
            }
        }

        // 기본방식의 랜덤은 노트분배가 끝난 후, 완성된 데이터를 스왑한다.
        switch ( GameSetting.CurrentRandom )
        {
            case GameRandom.Mirror:       notes.Reverse();                           break;
            case GameRandom.Basic_Random: Global.Math.Shuffle( notes, 0, KeyCount ); break;
            case GameRandom.Half_Random:
            {
                int keyCountHalf = Mathf.FloorToInt( KeyCount * .5f );
                Global.Math.Shuffle( notes, 0, keyCountHalf );
                Global.Math.Shuffle( notes, keyCountHalf + 1, KeyCount );
            }
            break;
        }

        // Notes = Enumerable.Range( 0, KeyCount ).Select( x => new ReadOnlyCollection<Note>( notes[x] ) ).ToArray();
        for ( int i = 0; i < KeyCount; i++ )
        {
            Notes[i] = new ReadOnlyCollection<Note>( notes[i] );
        }
    }

    #region Search
    public int Search( string _keyword )
    {
        ReadOnlyCollection<Song> originSongs = DataStorage.OriginSongs;
        if ( originSongs.Count == 0 || _keyword.Replace( " ", string.Empty ) == string.Empty )
        {
            Songs = originSongs;
            UpdateSong( CurrentSong.index );

            return -1;
        }

        List<Song> newSongs = new List<Song>();
        string keyword = _keyword.Replace( " ", string.Empty );
        bool   isSV    = keyword.ToUpper().CompareTo( "SV" ) == 0;
        for ( int i = 0; i < originSongs.Count; i++ )
        {
            if ( isSV )
            {
                if ( originSongs[i].minBpm != originSongs[i].maxBpm )
                     newSongs.Add( originSongs[i] );
            }
            else
            {
                if ( originSongs[i].title.Replace(   " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].version.Replace( " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].artist.Replace(  " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) ||
                     originSongs[i].source.Replace(  " ", string.Empty ).Contains( keyword, StringComparison.OrdinalIgnoreCase ) )
                     newSongs.Add( originSongs[i] );
            }
        }

        int prevIndex = 0;
        Songs = newSongs.Count == 0 ? originSongs : new ReadOnlyCollection<Song>( newSongs );
        for ( int i = 0; i < Songs.Count; i++ )
        {
            if ( Songs[i].title.Contains(   CurrentSong.title,   StringComparison.OrdinalIgnoreCase ) &&
                 Songs[i].version.Contains( CurrentSong.version, StringComparison.OrdinalIgnoreCase ) )
            {
                prevIndex = i;
                break;
            }
        }

        UpdateSong( prevIndex );

        return newSongs.Count;
    }

    public void Search( Song _song )
    {
        int prevIndex = 0;
        ReadOnlyCollection<Song> originSongs = DataStorage.OriginSongs;
        for ( int i = 0; i < originSongs.Count; i++ )
        {
            if ( originSongs[i].title.Contains(   _song.title,   StringComparison.OrdinalIgnoreCase ) &&
                 originSongs[i].version.Contains( _song.version, StringComparison.OrdinalIgnoreCase ) )
            {
                prevIndex = i;
                break;
            }
        }

        UpdateSong( prevIndex );
    }
    #endregion
   
    #region Playing
    /// <summary> 기본 변수 초기화 </summary>
    public void Clear()
    {
        AudioManager.Inst.AllStop();

        SaveTime      = -AudioLeadIn;
        Playback      = double.MinValue;
        Distance      = double.MinValue;
        DistanceCache = 0d;
        bgmIndex      = 0;
        bpmIndex      = 0;
        IsStart       = false;

        OnClear?.Invoke();
    }

    public async Task Release()
    {
        StopAllCoroutines();
        await ThreadCancel();
        
        OnRelease?.Invoke();
        Notes     = null;
        Samples   = null;
        IsLoaded  = false;
    }

    public void GameStart()
    {
        OnGameStart?.Invoke();
        breakPoint            ??= new CancellationTokenSource();
        timeTask                = Task.Run( () => { TimeUpdate( SystemSetting.InputTargetFrame, breakPoint.Token ); } );
        AudioManager.Inst.Pause = false;
        IsStart                 = true;
    }

    public IEnumerator GameOver()
    {
        IsStart   = false;
        Task task = ThreadCancel();
        yield return new WaitUntil( () => task.IsCompleted );
        float speed = 1f;
        while ( speed > 0f )
        {
            speed -= ( 1f / 3f ) * Time.deltaTime;
            float decrease = ( 1f - speed ) * GameSetting.CurrentPitch * .3f;
            AudioManager.Inst.Pitch = GameSetting.CurrentPitch - decrease;

            Playback += speed * ( 1000f * Time.deltaTime );
            Distance  = DistanceCache + ( Playback - DataStorage.Timings[bpmIndex].time );
            
            yield return null;
        }

        OnGameOver?.Invoke();
    }

    /// <returns> FALSE : Playback is higher than the last note time. </returns>
    public async void Pause( bool _isPause )
    {
        if ( _isPause )
        {
            IsStart = false;
            await ThreadCancel();

            SaveTime = Playback;
            AudioManager.Inst.Pause  = true;
            OnPause?.Invoke( true );
        }
        else
        {
            StartCoroutine( Continue() );
        }
    }

    private IEnumerator Continue()
    {
        CurrentScene.IsInputLock = true;
        double recoil  = Playback - WaitPauseTime;
        var    timings = DataStorage.Timings;
        while ( Playback > recoil )
        {
            Playback -= Time.deltaTime * 1200f;
            Distance = DistanceCache + ( Playback - timings[bpmIndex].time );

            yield return null;
        }

        while ( Playback < SaveTime )
        {
            Playback += Time.deltaTime * 1200f;
            Distance = DistanceCache + ( Playback - timings[bpmIndex].time );

            yield return null;
        }

        OnPause?.Invoke( false );
        breakPoint             ??= new CancellationTokenSource();
        timeTask                 = Task.Run( () => { TimeUpdate( SystemSetting.InputTargetFrame, breakPoint.Token ); } );
        AudioManager.Inst.Pause  = false;
        IsStart                  = true;
        CurrentScene.IsInputLock = false;
    }
    #endregion

    public void UpdateSong( int _index )
    {
        if ( _index >= Songs.Count )
             throw new Exception( "out of range" );

        CurrentIndex = _index;
        CurrentSong  = Songs[_index];
    }

    public double GetDistance( double _time )
    {
        double result  = 0d;
        var    timings = DataStorage.Timings;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / MainBPM;

            // 지나간 타이밍에 대한 거리
            if ( i + 1 < timings.Count && timings[i + 1].time < _time )
            {
                result += bpm * ( timings[i + 1].time - time );
                continue;
            }

            // 현재 타이밍에 대한 거리
            result += bpm * ( _time - time );
            break;
        }

        return result;
    }
}
#define START_FREESTYLE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS0162

public enum SoundType { BGM, KeySound, }


public class NowPlaying : Singleton<NowPlaying>
{
    #region Variables
    public static Scene CurrentScene;

    public ReadOnlyCollection<Song> Songs;
    public ReadOnlyCollection<Song> OriginSongs;
    public int CurrentIndex { get; private set; }
    public static Song CurrentSong { get; private set; }
    public static Chart CurrentChart { get; private set; }
    public static ReadOnlyCollection<Timing> Timings { get; private set; }
    public static int KeyCount => GameSetting.HasFlag( GameMode.KeyConversion ) && CurrentSong.keyCount == 7 ? 6 : CurrentSong.keyCount;
    public static int TotalNotes 
    {
        get 
        {
            bool hasKeyConversion = GameSetting.HasFlag( GameMode.KeyConversion ) && CurrentSong.keyCount == 7;
            var note   = hasKeyConversion ? CurrentSong.noteCount   - CurrentSong.delNoteCount   : CurrentSong.noteCount;
            var slider = hasKeyConversion ? CurrentSong.sliderCount - CurrentSong.delSliderCount : CurrentSong.sliderCount;
            return note + ( slider * 2 );
        }
    }

    #region Sample Sounds
    private Dictionary<string/* 키음 이름 */, FMOD.Sound> loadedSounds = new Dictionary<string, FMOD.Sound>();
    private List<KeySound> bgms = new List<KeySound>();
    private int bgmIndex;
    public static bool UseAllSamples { get; private set; }
    #endregion

    #region Time
    private double startTime;
    public double  SaveTime { get; private set; }
    public  static double WaitTime { get; private set; }
    public  static readonly double StartWaitTime = -3000d;
    public  static readonly double PauseWaitTime = -2000d;
    public  static double Playback { get; private set; }
    public  static double Distance { get; private set; }
    private static double DistanceCache;
    #endregion

    public int TotalFileCount { get; private set; }
    public static event Action<Song> OnParsing;
    public static event Action       OnParsingEnd;

    public static bool IsStart { get; private set; }
    public static bool IsParsing { get; private set; }
    public static bool IsLoadBGA { get; set; }
    #endregion

    private double mainBPM;
    private int timingIndex;
    private CancellationTokenSource breakPoint = new CancellationTokenSource();

    #region Unity Callback
    protected override async void Awake()
    {
        base.Awake();
        
        KeySetting   keySetting   = KeySetting.Inst;
        InputManager inputManager = InputManager.Inst;

        LoadSongs();
        await Task.Run( () => UpdateTime( breakPoint.Token ) );
    }

    private void OnApplicationQuit()
    {
        Stop();
        breakPoint?.Cancel();
    }
    #endregion

    private async void UpdateTime( CancellationToken _token )
    {
        while ( !_token.IsCancellationRequested )
        {
            // FMOD System Update
            AudioManager.Inst.SystemUpdate();

            if ( IsStart )
            {
                // 시간 갱신
                Playback = SaveTime + ( DateTime.Now.TimeOfDay.TotalMilliseconds - startTime );

                // 이동된 거리 계산
                for ( int i = timingIndex; i < Timings.Count; i++ )
                {
                    double time = Timings[i].time;
                    double bpm  = Timings[i].bpm / mainBPM;

                    // 지나간 타이밍에 대한 거리
                    if ( i + 1 < Timings.Count && Timings[i + 1].time < Playback )
                    {
                        timingIndex   += 1;
                        DistanceCache += bpm * ( Timings[i + 1].time - time );
                        break;
                    }

                    // 이전 타이밍까지의 거리( 캐싱 ) + 현재 타이밍에 대한 거리
                    Distance = DistanceCache + ( bpm * ( Playback - time ) );
                    break;
                }

                // 시간의 흐름에 따라 자동재생되는 음악 처리 ( 사운드 샘플 )
                while ( bgmIndex < bgms.Count && bgms[bgmIndex].time <= Playback )
                {
                    Play( bgms[bgmIndex] );
                    if ( ++bgmIndex < bgms.Count )
                         UseAllSamples = true;
                }
            }

            await Task.Delay( 1 );
        }
    }

    #region Parsing
    public void LoadChart()
    {
        Stop();
        WaitTime = StartWaitTime;
        mainBPM  = CurrentSong.mainBPM * GameSetting.CurrentPitch;

        // 채보 파싱
        using ( FileParser parser = new FileParser() )
        {
            if ( parser.TryParse( CurrentSong.filePath, out Chart chart ) )
            {
                CurrentChart = chart;
                Timings      = chart.timings;

                // 단일 배경음은 자동재생되는 사운드샘플로 재생
                if ( !CurrentSong.isOnlyKeySound )
                      AddSample( new KeySound( GameSetting.SoundOffset, Path.GetFileName( CurrentSong.audioName ), 1f ), SoundType.BGM );

                // 사운드샘플 로딩 ( 자동재생 )
                for ( int i = 0; i < chart.samples.Count; i++ )
                      AddSample( chart.samples[i], SoundType.BGM );

                InputManager.Inst.Initialize();
            }
            else
            {
                CurrentScene.LoadScene( SceneType.FreeStyle );
                Debug.LogWarning( $"Parsing failed  Current Chart : {CurrentSong.title}" );
            }
        }
    }

    public bool LoadSongs()
    {
        Timer timer = new Timer();
        IsParsing = true;
        //ConvertSong();
        List<Song> newSongs = new List<Song>();

        // StreamingAsset\\Songs 안의 모든 파일 순회하며 파싱
        string[] files = Global.FILE.GetFilesInSubDirectories( GameSetting.SoundDirectoryPath, "*.osu" );
        TotalFileCount = files.Length;
        for ( int i = 0; i < TotalFileCount; i++ )
        {
            using ( FileParser parser = new FileParser() )
            {
                if ( parser.TryParse( files[i], out Song newSong ) )
                {
                    newSong.index = newSongs.Count;
                    newSongs.Add( newSong );
                    OnParsing?.Invoke( newSong );

                }
            }
        }

        newSongs.Sort( ( _left, _right ) => _left.title.CompareTo( _right.title ) );
        for ( int i = 0; i < newSongs.Count; i++ )
        {
            var song    = newSongs[i];
            song.index  = i;
            newSongs[i] = song;
        }

        Songs       = new ReadOnlyCollection<Song>( newSongs );
        OriginSongs = new ReadOnlyCollection<Song>( Songs );

        if ( Songs.Count > 0 )
             UpdateSong( 0 );

        OnParsingEnd?.Invoke();
        IsParsing = false;

        Debug.Log( $"Update Songs {timer.End} ms" );

        // 파일 수정하고 싶을 때 사용
        //for ( int i = 0; i < OriginSongs.Count; i++ )
        //{
        //    using ( FileParser parser = new FileParser() )
        //        parser.ReWrite( OriginSongs[i] );
        //}

        return true;
    }
    #endregion

    #region KeySound
    public void Play( in KeySound _sample )
    {
        if ( loadedSounds.ContainsKey( _sample.name ) )
        {
            AudioManager.Inst.Play( loadedSounds[_sample.name], _sample.volume );
        }
    }

    /// <summary> 시간의 흐름에 따라 자동으로 재생되는 사운드샘플 </summary>
    public void AddSample( in KeySound _sample, SoundType _type )
    {
        if ( _type == SoundType.BGM )
             bgms.Add( _sample );

        if ( loadedSounds.ContainsKey( _sample.name ) )
        {
            // 이미 로딩된 사운드
        }
        else
        {
            // 새로운 사운드 로딩
            var dir = Path.GetDirectoryName( CurrentSong.filePath );
            if ( AudioManager.Inst.Load( Path.Combine( dir, _sample.name ), out FMOD.Sound sound ) )
                 loadedSounds.Add( _sample.name, sound );
        }
    }
    #endregion
    #region Search
    public void Search( string _keyword )
    {
        if ( OriginSongs.Count == 0 )
             return;

        if ( _keyword.Replace( " ", string.Empty ) == string.Empty )
        {
            Songs = new ReadOnlyCollection<Song>( OriginSongs );
            UpdateSong( CurrentSong.index );

            return;
        }

        List<Song> newSongs = new List<Song>();
        bool isSV = _keyword.Replace( " ", string.Empty ).ToUpper().CompareTo( "SV" ) == 0;
        for ( int i = 0; i < OriginSongs.Count; i++ )
        {
            if ( isSV )
            {
                if ( OriginSongs[i].minBpm != OriginSongs[i].maxBpm )
                     newSongs.Add( OriginSongs[i] );
            }
            else
            {
                if ( OriginSongs[i].title.Replace(   " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) ||
                     OriginSongs[i].version.Replace( " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) ||
                     OriginSongs[i].artist.Replace(  " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) ||
                     OriginSongs[i].source.Replace(  " ", string.Empty ).Contains( _keyword, StringComparison.OrdinalIgnoreCase ) )
                     newSongs.Add( OriginSongs[i] );
            }
        }

        Songs = new ReadOnlyCollection<Song>( newSongs );
        if ( Songs.Count != 0 )
             UpdateSong( 0 );
    }

    public void Search( Song _song )
    {
        for ( int i = 0; i < OriginSongs.Count; i++ )
        {
            if ( OriginSongs[i].title.Contains(   _song.title,   StringComparison.OrdinalIgnoreCase ) &&
                 OriginSongs[i].version.Contains( _song.version, StringComparison.OrdinalIgnoreCase ) )
            { 
                UpdateSong( i );
                return;
            }
        }

        UpdateSong( 0 );
    }

    #endregion
   
    #region Sound Process
    public void Play()
    {
        // 사운드샘플 오름차순 정렬 ( 시간기준 )
        bgms.Sort( delegate ( KeySound _A, KeySound _B )
        {
            if      ( _A.time > _B.time ) return 1;
            else if ( _A.time < _B.time ) return -1;
            else                          return 0;
        } );

        AudioManager.Inst.SetPaused( false, ChannelType.BGM );
        startTime      = DateTime.Now.TimeOfDay.TotalMilliseconds;
        SaveTime       = WaitTime;
        IsStart        = true;
    }

    public void Clear()
    {
        StopAllCoroutines();
        Playback       = WaitTime;
        SaveTime       = WaitTime;
        Distance       = 0d;
        DistanceCache  = 0d;
        bgmIndex       = 0;
        timingIndex    = 0;
        IsStart        = false;
        UseAllSamples  = false;
    }

    public void Stop()
    {
        Clear();
        IsLoadBGA      = false;

        foreach ( var keySound in loadedSounds )
        {
            var sound = keySound.Value;
            if ( sound.hasHandle() )
            {
                sound.release();
                sound.clearHandle();
            }
        }
        loadedSounds.Clear();
        bgms.Clear();
    }


    public IEnumerator GameOver()
    {
        IsStart = false;
        float slowTimeOffset = 1f / 3f;
        float speed = 1f;
        float pitchOffset = GameSetting.CurrentPitch * .3f;
        while ( true )
        {
            Playback += speed * Time.deltaTime;
            Distance = DistanceCache + ( ( Timings[timingIndex].bpm / mainBPM ) * ( Playback - Timings[timingIndex].time ) );

            CurrentScene.UpdatePitch( GameSetting.CurrentPitch - ( ( 1f - speed ) * pitchOffset ) );
            speed -= slowTimeOffset * Time.deltaTime;
            if ( speed < 0f )
                 break;

            yield return null;
        }
    }

    /// <returns>   FALSE : Playback is higher than the last note time. </returns>
    public void Pause( bool _isPause )
    {
        if ( _isPause )
        {
            IsStart = false;
            SaveTime = Playback + PauseWaitTime;
            AudioManager.Inst.SetPaused( true, ChannelType.BGM );
        }
        else
        {
            StartCoroutine( Continue() );
        }
    }

    private IEnumerator Continue()
    {
        while ( Playback > SaveTime )
        {
            yield return null;

            Playback -= Time.deltaTime * 1.2f;
            Distance = DistanceCache + ( ( Timings[timingIndex].bpm / mainBPM ) * ( Playback - Timings[timingIndex].time ) );
        }

        startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        IsStart = true;

        WaitUntil waitPlayback = new WaitUntil( () => Playback > SaveTime - PauseWaitTime );
        yield return waitPlayback;
        AudioManager.Inst.SetPaused( false, ChannelType.BGM );

        yield return YieldCache.WaitForSeconds( 3f );
        CurrentScene.IsInputLock = false;
    }
    #endregion

    #region Etc.
    public void UpdateSong( int _index )
    {
        if ( _index >= Songs.Count )
             throw new Exception( "out of range" );

        CurrentIndex = _index;
        CurrentSong  = Songs[_index];
    }
    #endregion

    public double GetDistance( double _time )
    {
        double result = 0d;
        for ( int i = 0; i < Timings.Count; i++ )
        {
            double time = Timings[i].time;
            double bpm  = Timings[i].bpm / mainBPM;

            // 지나간 타이밍에 대한 거리
            if ( i + 1 < Timings.Count && Timings[i + 1].time < _time )
            {
                result += bpm * ( Timings[i + 1].time - time );
                continue;
            }

            // 현재 타이밍에 대한 거리
            result += bpm * ( _time - time );
            break;
        }
        return result;
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum GameKeyCount : int { _1 = 1, _2, _3, _4, _5, _6, _7, _8, };
public enum KeyState { None, Down, Hold, Up, }
public enum NoteType { None, Default, Slider, }

public struct HitData
{
    public double    time; // 누르거나 뗀 시간
    public HitResult hitResult;
    public KeyState  keyState;

    public HitData( double _time, HitResult _hitResult, KeyState _keyState )
    {
        time      = _time;
        hitResult = _hitResult;
        keyState  = _keyState;
    }
}

public class InputManager : Singleton<InputManager>
{
    [Header( "Input Key" )]
    // 게임에서 사용되는 사용자가 설정한 키
    public static Dictionary<GameKeyCount, KeyCode[]> Keys { get; private set; } = new();
    // 상호 변환을 위해 매핑된 키
    private static readonly Dictionary<int/* Virtual Key */, KeyCode>        VKeyToUnity   = new ();
    private static readonly Dictionary<KeyCode, int/* Virtual Key */>        UnityToVKey   = new ();
    private static readonly Dictionary<KeyCode, string/*keyCode to string*/> UnityToString = new ();
    #region Properties
    public static Dictionary<KeyCode, string>.KeyCollection AvailableKeys => UnityToString.Keys;
    public static bool IsAvailable( KeyCode _key )      => UnityToString.ContainsKey( _key );
    public static KeyCode GetKeyCode( int _vKey )       => VKeyToUnity.TryGetValue( _vKey, out KeyCode keyCode ) ? keyCode : KeyCode.None;
    public static int GetVirtualKey( KeyCode _keyCode ) => UnityToVKey.TryGetValue( _keyCode, out int vKey ) ? vKey : -1;
    public static string GetString( KeyCode _code )     => UnityToString.ContainsKey( _code ) ? UnityToString[_code] : "None";
    #endregion

    [Header( "Lane" )]
    private List<Lane>   lanes = new();
    private List<Note>[] notes;
    private Lane         prefab;

    [Header( "Thread" )]
    private CancellationTokenSource breakPoint = new();
    [DllImport( "user32.dll" )] static extern short GetAsyncKeyState( int _vKey );

    private static Queue<HitData> HitDataQueue = new ();
    private static List<HitData>  HitDatas     = new ();
    private static int[]          Indexes   ; //= new int     [lanes.Count];
    private static bool[]         IsEntries ; //= new bool    [lanes.Count]; // 하나의 입력으로 하나의 노트만 처리하기위한 노트 진입점
    private static bool[]         Previous  ; //= new bool[lanes.Count];
    private static KeyState[]     KeyStates ; //= new KeyState[lanes.Count]; // 레인별 입력 상태
    private static KeySound[]     KeySounds ; //= new KeySound[lanes.Count];

    public  static event Action<HitData> OnHitNote;


    private System.Random random;

    protected override void Awake()
    {
        base.Awake();
        // Key Setting
        KeyBind( GameKeyCount._4, new KeyCode[] { KeyCode.W, KeyCode.E, KeyCode.P, KeyCode.LeftBracket } );
        KeyBind( GameKeyCount._6, new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.P, KeyCode.LeftBracket, KeyCode.RightBracket } );
        KeyBind( GameKeyCount._7, new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.Space, KeyCode.P, KeyCode.LeftBracket, KeyCode.RightBracket, } );
        KeyMapping();

        // Event Bind
        NowPlaying.OnPreInitialize  += CreateLanes;
        NowPlaying.OnPostInitAsync  += DivideNotes;
        NowPlaying.OnPostInitialize += SetLanes;
        NowPlaying.OnUpdateInThread += UpdateInput;

        // Load Asset
        DataStorage.Inst.LoadAssetsAsync( "Lane", ( GameObject _lane ) =>
        {
            if ( !_lane.TryGetComponent( out prefab ) )
                  Debug.LogError( $"Load Asset Failed ( Lane )" );
        } );
    }

    private void Update()
    {
        if ( HitDataQueue.TryDequeue( out HitData hitData ) )
        {
            HitDatas.Add( hitData );
            OnHitNote?.Invoke( hitData );
        }
    }

    private void OnApplicationQuit()
    {
        breakPoint?.Cancel();
        Release();
    }

    private void SelectNextNote( int _lane )
    {
        int prev = Indexes[_lane];
        Indexes[_lane]++;
        IsEntries[_lane] = false;

        // 사운드 변경 ( 모든 데이터 체크완료 시 마지막 사운드로 고정 )
        if ( Indexes[_lane]   < notes[_lane].Count )
             KeySounds[_lane] = notes[_lane][Indexes[_lane]].keySound;
    }

    private void UpdateInput()
    {
        for ( int i = 0; i < lanes.Count; i++ )
        {

            //KeyState previous = KeyStates[i];
            //if ( ( GetAsyncKeyState( lanes[i].VKey ) & 0x8000 ) != 0 ) KeyStates[i] = previous == KeyState.None || previous == KeyState.Up   ? KeyState.Down : KeyState.Hold;
            //else                                                       KeyStates[i] = previous == KeyState.Down || previous == KeyState.Hold ? KeyState.Up   : KeyState.None;

            // 입력 상태 체크
            bool current = ( GetAsyncKeyState( lanes[i].VKey ) & 0x8000 ) != 0;
            KeyStates[i] = (Previous[i], current) switch
            {
                ( false, true ) => KeyState.Down,
                ( true, true  ) => KeyState.Hold,
                ( true, false ) => KeyState.Up,
                _               => KeyState.None
            };
            Previous[i] = current;

            //  키음 선 실행
            if ( KeyStates[i] == KeyState.Down )
            {
                // 노트에 키음이 없을 수도 있다. 
                if ( DataStorage.Inst.TryGetSound( KeySounds[i].name, out FMOD.Sound sound ) )
                     AudioManager.Inst.Play( sound, KeySounds[i].volume );
            }

            // 데이터 소진 시 계산 불필요
            if ( Indexes[i] >= notes[i].Count )
                 continue;
            
            Note note        = notes[i][Indexes[i]];
            double playback  = NowPlaying.Playback;
            double startDiff = note.time       - playback;
            double endDiff   = note.sliderTime - playback;

            if ( !IsEntries[i] ) // 노트 시작판정
            {
                if ( KeyStates[i] == KeyState.Down && Judgement.CanBeHit( startDiff ) )
                {
                    if ( note.isSlider ) IsEntries[i] = true;
                    else                 SelectNextNote( i );

                    HitData hitData = new HitData( playback, Judgement.UpdateResult( startDiff, note.isSlider ), KeyState.Down );
                    HitDataQueue.Enqueue( hitData );
                    lanes[i].AddData( hitData );
                }
                else if ( Judgement.IsMiss( startDiff ) )
                {
                    SelectNextNote( i );
                    // 롱노트 시작에서 Miss일 때, 시작판정, 끝판정 2번의 Miss 처리가 되게끔 한다.
                    HitData hitData = new HitData( playback, Judgement.UpdateResult( startDiff, note.isSlider ), KeyState.None );
                    HitDataQueue.Enqueue( hitData );
                    lanes[i].AddData( hitData );
                }
            }
            else // 롱노트 끝판정
            {
                if ( KeyStates[i] == KeyState.Up || endDiff <= 0d )
                {
                    SelectNextNote( i );
                    HitData hitData = new HitData( playback, Judgement.UpdateResult( endDiff ), KeyState.Up );
                    HitDataQueue.Enqueue( hitData );
                    lanes[i].AddData( hitData );
                }
                else if ( Judgement.IsMiss( endDiff ) )
                {
                    SelectNextNote( i );
                    HitData hitData = new HitData( playback, Judgement.UpdateResult( endDiff ), KeyState.None );
                    HitDataQueue.Enqueue( hitData );
                    lanes[i].AddData( hitData );
                }
            }
        }
    }

    
    private void DivideNotes()
    {
        Debug.Log( NowPlaying.TotalJudge );
        ReadOnlyCollection<Note> datas = DataStorage.Notes;
        bool isConvert  = GameSetting.HasFlag( GameMode.KeyConversion ) && NowPlaying.CurrentSong.keyCount == 7;
        bool isNoSlider = GameSetting.HasFlag( GameMode.NoSlider );
        random = new System.Random( ( int )DateTime.Now.Ticks );

        List<int/* lane */> emptyLanes = new List<int>( NowPlaying.KeyCount );
        double[] prevTimes             = Enumerable.Repeat( double.MinValue, NowPlaying.KeyCount ).ToArray();
        double   secondPerBeat         = ( ( ( 60d / NowPlaying.CurrentSong.mainBPM ) * 4d ) / 32d );
        for ( int i = 0; i < datas.Count; i++ )
        {
            Note newNote = datas[i];
            if ( isConvert )
            {
                switch ( newNote.lane )
                {
                    // 잘려진 노트는 키음만 자동재생되도록 한다.
                    case 3: NowPlaying.Inst.AddSound( new KeySound( newNote ), SoundType.BGM );
                    continue;
                    
                    // 잘려진 옆 노트를 한칸씩 이동한다.
                    case > 3: newNote.lane -= 1; 
                    break;
                }
            }
            
            if ( isNoSlider )
                 newNote.isSlider = false;

            switch ( GameSetting.CurrentRandom )
            {
                // 레인 인덱스와 동일한 번호에 노트 분배
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Basic_Random:
                case GameRandom.Half_Random:
                {
                    newNote.noteDistance   = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.sliderDistance = NowPlaying.Inst.GetDistance( newNote.sliderTime );

                    NowPlaying.Inst.AddSound( newNote.keySound, SoundType.KeySound );
                    notes[newNote.lane].Add( newNote );
                } break;

                // 맥스랜덤은 무작위 레인에 노트를 배치한다.
                case GameRandom.Max_Random:
                {
                    emptyLanes.Clear();
                    // 빠른계단, 즈레 등 고밀도로 배치될 때 보정
                    for ( int j = 0; j < NowPlaying.KeyCount; j++ )
                    {
                        if ( secondPerBeat < ( newNote.time - prevTimes[j] ) )
                             emptyLanes.Add( j );
                    }

                    // 자리가 없을 때 보정되지않은 상태로 배치
                    if ( emptyLanes.Count == 0 )
                    {
                        for ( int j = 0; j < NowPlaying.KeyCount; j++ )
                        {
                            if ( prevTimes[j] < newNote.time )
                                 emptyLanes.Add( j );
                        }
                    }

                    int selectLane        = emptyLanes[random.Next( 0, int.MaxValue ) % emptyLanes.Count];
                    prevTimes[selectLane] = newNote.isSlider ? newNote.sliderTime : newNote.time;

                    newNote.noteDistance   = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.sliderDistance = NowPlaying.Inst.GetDistance( newNote.sliderTime );

                    NowPlaying.Inst.AddSound( newNote.keySound, SoundType.KeySound );
                    notes[selectLane].Add( newNote );
                } break;
            }
        }

        switch ( GameSetting.CurrentRandom )
        {
            // 레인은 유지하고, 분배된 노트 데이터만 스왑한다.
            case GameRandom.Mirror:
            {
                notes.Reverse();
            } break;

            case GameRandom.Basic_Random:
            {
                Swap( 0, NowPlaying.KeyCount );
            } break;

            case GameRandom.Half_Random:
            { 
                int keyCountHalf = Mathf.FloorToInt( NowPlaying.KeyCount * .5f );
                Swap( 0,                keyCountHalf );
                Swap( keyCountHalf + 1, NowPlaying.KeyCount );
            } break;
        }


        for ( int i = 0; i < lanes.Count; i++ )
              KeySounds[i] = notes[i][0].keySound;
    }

    private void CreateLanes()
    {
        Indexes   = new int     [NowPlaying.KeyCount]; // 노트 인덱스
        IsEntries = new bool    [NowPlaying.KeyCount]; // 롱노트 진입점
        Previous  = new bool    [NowPlaying.KeyCount]; // 이전 키 상태
        KeyStates = new KeyState[NowPlaying.KeyCount]; // 레인별 입력 상태
        KeySounds = new KeySound[NowPlaying.KeyCount];

        notes = new List<Note>[NowPlaying.KeyCount];
        for ( int i = 0; i < NowPlaying.KeyCount; i++ )
        {
            lanes.Add( Instantiate( prefab, transform ) );
            notes[i] = new List<Note>();
        }
        Debug.Log( $"Create {lanes.Count} lanes." );
    }

    private void SetLanes()
    {
        for ( int i = 0; i < lanes.Count; i++ )
              lanes[i].Initialize( i, notes[i] );
    }

    //public async void GameStart()
    //{
    //    await Task.Run( () => UpdateInput( breakPoint.Token ) );
    //}

    public void Release()
    {
        for ( int i = 0; i < lanes.Count; i++ )
            DestroyImmediate( lanes[i], true );
        
        lanes.Clear();

        for ( int i = 0; i < notes.Length; i++ )
            notes[i].Clear();

        notes = null;
    }

    private void Swap( int _min, int _max )
    {
        for ( int i = 0; i < 10; i++ )
        {
            var randA = random.Next( _min, _max );
            var randB = random.Next( _min, _max );

            var tmp      = notes[randA];
            notes[randA] = notes[randB];
            notes[randB] = tmp;
        }
    }

    #region Key Setting
    private void KeyBind( GameKeyCount _key, KeyCode[] _code )
    {
        int count = ( int )_key;
        if ( count != _code.Length )
        {
            Debug.LogError( "key count and length do not match. " );
            return;
        }
        if ( Keys.ContainsKey( _key ) )
        {
            Debug.LogWarning( "The key already exists." );
            return;
        }

        Keys.Add( _key, _code );
    }

    private void AddMapping( int _vKey, KeyCode _keyCode, string _string )
    {
        VKeyToUnity[_vKey]    = _keyCode;
        UnityToVKey[_keyCode] = _vKey;
    }

    private void KeyMapping()
    {
        // 숫자 (0~9)
        for ( int i = 0; i <= 9; i++ )
        {
            int vKey     = 0x30 + i; // '0' ~ '9'
            KeyCode uKey = KeyCode.Alpha0 + i;
            AddMapping( vKey, uKey, uKey.ToString() );
        }


        // 알파벳 (A~Z)
        for ( int i = 0; i < 26; i++ )
        {
            int vKey     = 0x41 + i; // 'A' ~ 'Z'
            KeyCode uKey = KeyCode.A + i;
            AddMapping( vKey, uKey, uKey.ToString() );
        }

        // 특수키
        //AddMapping( 0x08, KeyCode.Backspace, "Backspace" );
        AddMapping( 0xDC, KeyCode.Backslash, "\\"       );
        AddMapping( 0xC0, KeyCode.BackQuote, "`"        ); 
        AddMapping( 0x14, KeyCode.CapsLock,  "CapsLock" );
        AddMapping( 0x0D, KeyCode.Return,    "Return"   );
        AddMapping( 0x1B, KeyCode.Escape,    "Escape"   );
        AddMapping( 0x20, KeyCode.Space,     "Space"    );
        AddMapping( 0x09, KeyCode.Tab,       "Tab"      );
        AddMapping( 0xBB, KeyCode.Plus,      "="        );
        AddMapping( 0xBD, KeyCode.Minus,     "-"        ); 
        

        AddMapping( 0xDD, KeyCode.RightBracket, "["  );
        AddMapping( 0xDB, KeyCode.LeftBracket,  "]"  );
        AddMapping( 0xBA, KeyCode.Semicolon,    ";"  );
        AddMapping( 0xDE, KeyCode.Quote,        "\'" );
        AddMapping( 0xBC, KeyCode.Comma,        ","  );
        AddMapping( 0xBE, KeyCode.Period,       "."  );
        AddMapping( 0xBF, KeyCode.Slash,        "/"  );

        AddMapping( 0xA0, KeyCode.LeftShift,    "LShift" );  
        AddMapping( 0xA2, KeyCode.LeftControl,  "LCtrl"  );  
        AddMapping( 0xA4, KeyCode.LeftAlt,      "LAlt"   );      
        AddMapping( 0xA1, KeyCode.RightShift,   "RShift" ); 
        AddMapping( 0x19, KeyCode.RightControl, "RCtrl"  ); // 한자
        AddMapping( 0x15, KeyCode.RightAlt,     "RAlt"   ); // 한영
         
        AddMapping( 0x23, KeyCode.End,      "End"    );
        AddMapping( 0x24, KeyCode.Home,     "Home"   );
        AddMapping( 0x2E, KeyCode.Delete,   "Delete" );
        AddMapping( 0x2D, KeyCode.Insert,   "Insert" );
        AddMapping( 0x21, KeyCode.PageUp,   "PgUp"   );
        AddMapping( 0x22, KeyCode.PageDown, "PgDn"   );

        AddMapping( 0x26, KeyCode.UpArrow,    "Up"    );
        AddMapping( 0x28, KeyCode.DownArrow,  "Down"  );
        AddMapping( 0x25, KeyCode.LeftArrow,  "Left"  );
        AddMapping( 0x27, KeyCode.RightArrow, "Right" );


        // 넘버패드
        AddMapping( 0x60, KeyCode.Keypad0,        "Pad 0" );
        AddMapping( 0x61, KeyCode.Keypad1,        "Pad 1" );
        AddMapping( 0x62, KeyCode.Keypad2,        "Pad 2" );
        AddMapping( 0x63, KeyCode.Keypad3,        "Pad 3" );
        AddMapping( 0x64, KeyCode.Keypad4,        "Pad 4" );
        AddMapping( 0x65, KeyCode.Keypad5,        "Pad 5" );
        AddMapping( 0x66, KeyCode.Keypad6,        "Pad 6" );
        AddMapping( 0x67, KeyCode.Keypad7,        "Pad 7" );
        AddMapping( 0x68, KeyCode.Keypad8,        "Pad 8" );
        AddMapping( 0x69, KeyCode.Keypad9,        "Pad 9" );
        AddMapping( 0x6A, KeyCode.KeypadMultiply, "Pad *" );
        AddMapping( 0x6B, KeyCode.KeypadPlus,     "Pad +" );
        AddMapping( 0x6D, KeyCode.KeypadMinus,    "Pad -" );
        AddMapping( 0x6E, KeyCode.KeypadPeriod,   "Pad ." );
        AddMapping( 0x6F, KeyCode.KeypadDivide,   "Pad /" );

        //// 펑션키
        //for ( int i = 0; i < 12; i++ )
        //{
        //    int vKey = 0x70 + i;       // F1~F12
        //    KeyCode uKey = KeyCode.F1 + i;
        //    AddMapping( vKey, uKey, uKey.ToString() );
        //}
    }
    #endregion
}

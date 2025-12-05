using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public enum GameKeyCount : int { _1 = 1, _2, _3, _4, _5, _6, _7, _8, };
public enum KeyState : int { None, Down, Hold, Up, }

public struct HitData
{
    public int       lane;
    public double    time; // 누르거나 뗀 시간
    public double      diff;
    public HitResult hitResult;
    public KeyState  keyState;

    public HitData( int _lane, double _time, double _diff, HitResult _hitResult, KeyState _keyState )
    {
        lane      = _lane;
        time      = _time;
        diff      = _diff;
        hitResult = _hitResult;
        keyState  = _keyState;
    }

    public HitData( HitResult _hitResult )
    {
        lane      = 0;
        time      = 0d;
        diff      = 0d;
        hitResult = _hitResult;
        keyState  = KeyState.None;
    }
}

public class InputManager : Singleton<InputManager>
{
    [Header( "Input Key" )]
    // 사용자가 설정한 키
    public static Dictionary<GameKeyCount, KeyCode[]> Keys { get; private set; } = new();
    // 상호 변환을 위해 매핑된 키
    private static readonly Dictionary<int/* Virtual Key */, KeyCode>     VKeyToUnity   = new ();
    private static readonly Dictionary<KeyCode, int/* Virtual Key */>     UnityToVKey   = new ();
    private static readonly Dictionary<KeyCode, string/*keyCode String*/> UnityToString = new ();

    [Header( "Input Process" )]
    private int[]                    VKey;      // 가상 키
    private int[]                    Indexes;   // 노트 인덱스
    private double[]                 HeadDiff;  // 롱노트 전용 Head Hit Diff
    private bool[]                   IsEntries; // 롱노트 진입점
    private bool[]                   PrevState;  // 이전 키 상태( 입력 2중 체크 )
    private KeyState[]               KeyStates; // 레인별 입력 상태
    private KeySound[]               KeySounds;

    [Header( "Hold Process" )]
    private double LNStartTime;
    private double LNEndTime;
    private bool   isHolding;

    private static readonly double LNComboInterval = 100; // ms

    [DllImport( "user32.dll" )] static extern short GetAsyncKeyState( int _vKey );

    #region Properties
    public static Dictionary<KeyCode, string>.KeyCollection AvailableKeys => UnityToString.Keys;
    public static bool    IsAvailable( KeyCode _key )       => UnityToString.ContainsKey( _key );
    public static int     GetVirtualKey( KeyCode _keyCode ) => UnityToVKey.TryGetValue( _keyCode, out int vKey ) ? vKey : -1;
    public static string  GetString( KeyCode _code )        => UnityToString.ContainsKey( _code ) ? UnityToString[_code] : "None";
    public static KeyCode GetKeyCode( int _vKey )           => VKeyToUnity.TryGetValue( _vKey, out KeyCode keyCode ) ? keyCode : KeyCode.None;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        // 기본 키 설정
        if ( Config.Inst.Read( ConfigType._4K, out KeyCode[] _4K_KeyCodes ) ) KeyBind( GameKeyCount._4, _4K_KeyCodes );
        else                                                                  KeyBind( GameKeyCount._4, new KeyCode[] { KeyCode.W, KeyCode.E, KeyCode.P, KeyCode.LeftBracket } );

        if ( Config.Inst.Read( ConfigType._6K, out KeyCode[] _6K_KeyCodes ) ) KeyBind( GameKeyCount._6, _6K_KeyCodes );
        else                                                                  KeyBind( GameKeyCount._6, new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.Delete, KeyCode.End, KeyCode.PageDown } );

        if ( Config.Inst.Read( ConfigType._7K, out KeyCode[] _7K_KeyCodes ) ) KeyBind( GameKeyCount._7, _7K_KeyCodes );
        else                                                                  KeyBind( GameKeyCount._7, new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.Space, KeyCode.P, KeyCode.LeftBracket, KeyCode.RightBracket, } );
        KeyMapping();

        // 이벤트 연결
        NowPlaying.OnInitialize      += Initialize;
        NowPlaying.OnUpdateInThread  += UpdateInput;
        NowPlaying.OnRelease         += Release;
        NowPlaying.OnClear           += Clear;
    }

    private void Initialize()
    {
        VKey        = new int      [NowPlaying.KeyCount];
        Indexes     = new int      [NowPlaying.KeyCount];
        HeadDiff    = new double   [NowPlaying.KeyCount];
        IsEntries   = new bool     [NowPlaying.KeyCount];
        PrevState   = new bool     [NowPlaying.KeyCount];
        KeyStates   = new KeyState [NowPlaying.KeyCount];
        KeySounds   = new KeySound [NowPlaying.KeyCount];

        for ( int i = 0; i < NowPlaying.KeyCount; i++ )
        {
            VKey[i] = GetVirtualKey( Keys[( GameKeyCount )NowPlaying.KeyCount][i] );
        }
    }

    private void Clear()
    {
        for ( int i = 0; i < NowPlaying.KeyCount; i++ )
        {
            Indexes[i]     = 0;
            HeadDiff[i]    = 0d;
            IsEntries[i]   = false;
            PrevState[i]   = false;
            KeyStates[i]   = KeyState.None;
            KeySounds[i]   = NowPlaying.Notes[i][0].keySound;
        }
    }

    private void Release()
    {
        VKey        = null;
        Indexes     = null;
        HeadDiff    = null;
        IsEntries   = null;
        PrevState   = null;
        KeyStates   = null;
        KeySounds   = null;
    }

    private void UpdateInput()
    {
        double playback = NowPlaying.Playback;
        for ( int lane = 0; lane < NowPlaying.KeyCount; lane++ )
        {
            // 입력 상태 체크
            bool current = ( GetAsyncKeyState( VKey[lane] ) & 0x8000 ) != 0;
            KeyStates[lane] = (PrevState[lane], current) switch {
                ( false, true  ) => KeyState.Down,
                ( true,  true  ) => KeyState.Hold,
                ( true,  false ) => KeyState.Up,
                _                => KeyState.None
            };
            PrevState[lane] = current;

            //  키음 선 실행
            if ( KeyStates[lane] == KeyState.Down )
            {
                // 노트에 키음이 없을 수도 있다. 
                if ( DataStorage.Inst.GetSound( KeySounds[lane].name, out FMOD.Sound sound ) )
                    AudioManager.Inst.Play( sound, KeySounds[lane].volume );
            }


            // 데이터 소진 시 계산 불필요
            if ( Indexes[lane] >= NowPlaying.Notes[lane].Count )
                continue;

            Note   note     = NowPlaying.Notes[lane][Indexes[lane]];
            double headDiff = note.time    - playback;
            double tailDiff = note.endTime - playback;
            bool   isSlider = note.isSlider;


            if ( !IsEntries[lane] ) // 노트 시작판정
            {
                if ( Judgement.IsLateMiss( headDiff ) ) // Head Late Miss
                {
                    Judgement.UpdateResult( note, lane, playback, headDiff, 0d, KeyState.Down );
                    SelectNextNote( lane );
                    continue;
                }

                if ( KeyStates[lane] == KeyState.Down )
                {
                    Debug.Log( $"{playback}" );
                    if ( !isSlider && Judgement.IsEarlyMiss( headDiff ) )
                    {
                        Judgement.UpdateResult( note, lane, playback, headDiff, 0d, KeyState.Down );
                        SelectNextNote( lane );
                        continue;
                    }

                    if ( Judgement.CanBeHit( headDiff ) ) // Head Hit
                    {
                        Judgement.UpdateResult( note, lane, playback, headDiff, 0d, KeyState.Down );

                        if ( isSlider )
                        {
                            IsEntries[lane] = true;
                            HeadDiff[lane]  = headDiff;
                            LNEndTime       = Global.Math.Max( LNEndTime, note.endTime );

                            if ( !isHolding )
                            {
                                isHolding   = true;
                                LNStartTime = playback;
                            }
                        }
                        else
                        { 
                            SelectNextNote( lane );
                        }
                    }
                }
            }
            else // 롱노트 끝판정
            {
                if ( Judgement.IsLateMiss( tailDiff ) )
                {
                    Judgement.UpdateResult( note, lane, playback, HeadDiff[lane], tailDiff, KeyState.Up );
                    SelectNextNote( lane );
                    continue;
                }

                if ( KeyStates[lane] == KeyState.Up )
                {
                    Judgement.UpdateResult( note, lane, playback, HeadDiff[lane], tailDiff, KeyState.Up );
                    SelectNextNote( lane );
                }
            }
        }

        // Hold 중 콤보 증가
        if ( isHolding )
        {
            while ( LNStartTime + LNComboInterval <= playback &&
                    LNStartTime + LNComboInterval <= LNEndTime )
            {
                Judgement.AddCombo();
                LNStartTime += LNComboInterval;
            }
        }
    }

    private void SelectNextNote( int _lane )
    {
        Indexes[_lane]++;
        IsEntries[_lane] = false;
        HeadDiff[_lane]  = 0d;

        if ( isHolding )
        {
            bool isNotEntries = true;
            for ( int lane = 0; lane < NowPlaying.KeyCount; lane++ )
            {
                if ( IsEntries[lane] )
                {
                    isNotEntries = false;
                    break;
                }
            }

            if ( isNotEntries )
            {
                isHolding = false;
                LNStartTime = 0d;
                LNEndTime   = 0d;
            }
        }

        // 사운드 변경 ( 모든 데이터 체크완료 시 마지막 사운드로 고정 )
        if ( Indexes[_lane] < NowPlaying.Notes[_lane].Count )
             KeySounds[_lane] = NowPlaying.Notes[_lane][Indexes[_lane]].keySound;
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
        VKeyToUnity[_vKey]      = _keyCode;
        UnityToVKey[_keyCode]   = _vKey;
        UnityToString[_keyCode] = _string;
    }

    private void KeyMapping()
    {
        // 숫자 ( 0 ~ 9 )
        for ( int i = 0; i <= 9; i++ )
        {
            int vKey     = 0x30 + i; // '0' ~ '9'
            KeyCode uKey = KeyCode.Alpha0 + i;
            AddMapping( vKey, uKey, uKey.ToString() );
        }


        // 알파벳 ( A ~ Z )
        for ( int i = 0; i < 26; i++ )
        {
            int vKey     = 0x41 + i; // 'A' ~ 'Z'
            KeyCode uKey = KeyCode.A + i;
            AddMapping( vKey, uKey, uKey.ToString() );
        }

        // 특수키
        //AddMapping( 0x08, KeyCode.Backspace, "Backspace" );
        AddMapping( 0x09, KeyCode.Tab,            "Tab"      );
        AddMapping( 0xDC, KeyCode.Backslash,      "\\"       );
        AddMapping( 0xC0, KeyCode.BackQuote,      "`"        ); 
        AddMapping( 0x14, KeyCode.CapsLock,       "CapsLock" );
        AddMapping( 0x20, KeyCode.Space,          "Space"    );
        AddMapping( 0xBB, KeyCode.Plus,           "="        );
        AddMapping( 0xBD, KeyCode.Minus,          "-"        ); 

        AddMapping( 0xDD, KeyCode.RightBracket,   "]"        );
        AddMapping( 0xDB, KeyCode.LeftBracket,    "["        );
        AddMapping( 0xBA, KeyCode.Semicolon,      ";"        );
        AddMapping( 0xDE, KeyCode.Quote,          "\'"       );
        AddMapping( 0xBC, KeyCode.Comma,          ","        );
        AddMapping( 0xBE, KeyCode.Period,         "."        );
        AddMapping( 0xBF, KeyCode.Slash,          "/"        );
                                                  
        AddMapping( 0xA0, KeyCode.LeftShift,      "LShift"   );  
        AddMapping( 0xA2, KeyCode.LeftControl,    "LCtrl"    );  
        AddMapping( 0xA4, KeyCode.LeftAlt,        "LAlt"     );      
        AddMapping( 0xA1, KeyCode.RightShift,     "RShift"   ); 
        AddMapping( 0x19, KeyCode.RightControl,   "RCtrl"    ); // + 한자
        AddMapping( 0x15, KeyCode.RightAlt,       "RAlt"     ); // + 한영
                                                             
        AddMapping( 0x23, KeyCode.End,            "End"      );
        AddMapping( 0x24, KeyCode.Home,           "Home"     );
        AddMapping( 0x2E, KeyCode.Delete,         "Delete"   );
        AddMapping( 0x2D, KeyCode.Insert,         "Insert"   );
        AddMapping( 0x21, KeyCode.PageUp,         "PgUp"     );
        AddMapping( 0x22, KeyCode.PageDown,       "PgDn"     );
                                                  
        //AddMapping( 0x26, KeyCode.UpArrow,        "Up"       );
        //AddMapping( 0x28, KeyCode.DownArrow,      "Down"     );
        //AddMapping( 0x25, KeyCode.LeftArrow,      "Left"     );
        //AddMapping( 0x27, KeyCode.RightArrow,     "Right"    );

        // 넘버패드
        AddMapping( 0x60, KeyCode.Keypad0,        "Pad 0"    );
        AddMapping( 0x61, KeyCode.Keypad1,        "Pad 1"    );
        AddMapping( 0x62, KeyCode.Keypad2,        "Pad 2"    );
        AddMapping( 0x63, KeyCode.Keypad3,        "Pad 3"    );
        AddMapping( 0x64, KeyCode.Keypad4,        "Pad 4"    );
        AddMapping( 0x65, KeyCode.Keypad5,        "Pad 5"    );
        AddMapping( 0x66, KeyCode.Keypad6,        "Pad 6"    );
        AddMapping( 0x67, KeyCode.Keypad7,        "Pad 7"    );
        AddMapping( 0x68, KeyCode.Keypad8,        "Pad 8"    );
        AddMapping( 0x69, KeyCode.Keypad9,        "Pad 9"    );
        AddMapping( 0x6A, KeyCode.KeypadMultiply, "Pad *"    );
        AddMapping( 0x6B, KeyCode.KeypadPlus,     "Pad +"    );
        AddMapping( 0x6D, KeyCode.KeypadMinus,    "Pad -"    );
        AddMapping( 0x6E, KeyCode.KeypadPeriod,   "Pad ."    );
        AddMapping( 0x6F, KeyCode.KeypadDivide,   "Pad /"    );

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

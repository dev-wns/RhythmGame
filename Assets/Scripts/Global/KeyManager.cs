using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum KeyState { None, Down, Hold, Up, }
public class KeyManager : Singleton<KeyManager>
{
    private static readonly Dictionary<int/* Virtual Key */, KeyCode> vKeyToUnity = new();
    private static readonly Dictionary<KeyCode, int/* Virtual Key */> unityToVKey = new();

    private readonly Dictionary<int, KeyState> keyStates = new();

    // Thread
    private CancellationTokenSource cancelSource = new();

    [DllImport( "user32.dll" )]
    private static extern short GetAsyncKeyState( int _vKey );

    private double startTime;

    private async void Start()
    {
        startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        KeyMapping();

        await Task.Run( () => Process( cancelSource.Token ) );
    }

    private void OnApplicationQuit()
    {
        Stop();
    }

    private void Stop()
    {
        cancelSource?.Cancel();
    }

    private async void Process( CancellationToken _token )
    {
        while( !_token.IsCancellationRequested )
        {
            for ( int key = 0; key < 256; key++ )
            {
                // 마우스 제외
                if ( key >= 0x01 && key <= 0x06 )
                     continue; 

                // 키 상태 생성
                if ( !keyStates.ContainsKey( key ) )
                     keyStates[key] = KeyState.None;

                // 키 체크
                KeyState previous = keyStates[key];
                if ( ( GetAsyncKeyState( key ) & 0x8000 ) != 0 )
                {
                    keyStates[key] = previous == KeyState.None || previous == KeyState.Up ? KeyState.Down : KeyState.Hold;

                    if ( keyStates[key] == KeyState.Down )
                         Debug.Log( $"WinAPI \"{GetKeyCode( key )}\" Down {( uint )( DateTime.Now.TimeOfDay.TotalMilliseconds - startTime )} ms" );
                }
                else
                {
                    keyStates[key] = previous == KeyState.Down || previous == KeyState.Hold ? KeyState.Up : KeyState.None;
                }
            }

            await Task.Delay( 1 ); // 1000Hz
        }
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.A ) )
            Debug.Log( $"Unity \"{KeyCode.A}\" Down {( uint )( DateTime.Now.TimeOfDay.TotalMilliseconds - startTime )} ms" );

        if ( Input.GetKeyDown( KeyCode.S ) )
            Debug.Log( $"Unity \"{KeyCode.S}\" Down {( uint )( DateTime.Now.TimeOfDay.TotalMilliseconds - startTime )} ms" );

        if ( Input.GetKeyDown( KeyCode.D ) )
            Debug.Log( $"Unity \"{KeyCode.D}\" Down {( uint )( DateTime.Now.TimeOfDay.TotalMilliseconds - startTime )} ms" );
    }

    private void KeyMapping()
    {
        // 숫자 (0~9)
        for ( int i = 0; i <= 9; i++ )
        {
            int vKey = 0x30 + i; // '0' ~ '9'
            KeyCode uKey = KeyCode.Alpha0 + i;
            AddMapping( vKey, uKey );
        }

        // 알파벳 (A~Z)
        for ( int i = 0; i < 26; i++ )
        {
            int vKey = 0x41 + i; // 'A' ~ 'Z'
            KeyCode uKey = KeyCode.A + i;
            AddMapping( vKey, uKey );
        }

        // 특수키
        AddMapping( 0x20, KeyCode.Space );
        AddMapping( 0x1B, KeyCode.Escape );
        AddMapping( 0x0D, KeyCode.Return );
        AddMapping( 0x08, KeyCode.Backspace );
        AddMapping( 0x09, KeyCode.Tab );
        AddMapping( 0x2E, KeyCode.Delete );
        AddMapping( 0x2D, KeyCode.Insert );
        AddMapping( 0x24, KeyCode.Home );
        AddMapping( 0x23, KeyCode.End );
        AddMapping( 0x21, KeyCode.PageUp );
        AddMapping( 0x22, KeyCode.PageDown );
        AddMapping( 0x25, KeyCode.LeftArrow );
        AddMapping( 0x26, KeyCode.UpArrow );
        AddMapping( 0x27, KeyCode.RightArrow );
        AddMapping( 0x28, KeyCode.DownArrow );

        AddMapping( 0x10, KeyCode.LeftShift );    // 왼쪽 쉬프트
        AddMapping( 0x11, KeyCode.LeftControl );  // 왼쪽 컨트롤
        AddMapping( 0x12, KeyCode.LeftAlt );      // 왼쪽 Alt
        AddMapping( 0x5B, KeyCode.LeftWindows );  // 왼쪽 윈도우키

        // 펑션키
        for ( int i = 0; i < 12; i++ )
        {
            int vKey = 0x70 + i;       // F1~F12
            KeyCode uKey = KeyCode.F1 + i;
            AddMapping( vKey, uKey );
        }

        // 넘버패드
        AddMapping( 0x60, KeyCode.Keypad0 );
        AddMapping( 0x61, KeyCode.Keypad1 );
        AddMapping( 0x62, KeyCode.Keypad2 );
        AddMapping( 0x63, KeyCode.Keypad3 );
        AddMapping( 0x64, KeyCode.Keypad4 );
        AddMapping( 0x65, KeyCode.Keypad5 );
        AddMapping( 0x66, KeyCode.Keypad6 );
        AddMapping( 0x67, KeyCode.Keypad7 );
        AddMapping( 0x68, KeyCode.Keypad8 );
        AddMapping( 0x69, KeyCode.Keypad9 );
        AddMapping( 0x6A, KeyCode.KeypadMultiply );
        AddMapping( 0x6B, KeyCode.KeypadPlus );
        AddMapping( 0x6D, KeyCode.KeypadMinus );
        AddMapping( 0x6E, KeyCode.KeypadPeriod );
        AddMapping( 0x6F, KeyCode.KeypadDivide );
    }
    private void AddMapping( int _vKey, KeyCode _keyCode )
    {
        vKeyToUnity[_vKey] = _keyCode;
        unityToVKey[_keyCode] = _vKey;
    }

    public KeyCode GetKeyCode( int _vKey ) => vKeyToUnity.TryGetValue( _vKey, out KeyCode keyCode ) ? keyCode : KeyCode.None;

    public int GetVirtualKey( KeyCode _keyCode ) => unityToVKey.TryGetValue( _keyCode, out int vKey ) ? vKey : -1;
}

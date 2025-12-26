using System.Runtime.InteropServices;

public enum GameKeyCount : int { _1 = 1, _2, _3, _4, _5, _6, _7, _8, }; // Unity KeyCode
public enum KeyState : ushort { None = 0, Down, Hold, Up, }

[StructLayout( LayoutKind.Sequential )]
public struct KeyEvent
{
    public int       vKey;
    public KeyState  keyState;
    public long      timestamp;

    public KeyEvent( int _vKey, KeyState _keyState, long _timestamp )
    {
        vKey      = _vKey;
        keyState  = _keyState;
        timestamp = _timestamp;
    }
}
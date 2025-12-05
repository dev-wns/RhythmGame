using System.Runtime.InteropServices;
using UnityEngine;

public class RawInput : MonoBehaviour
{
    [DllImport( "Assistance" )]
    public static extern bool StartRawInput();

    [DllImport( "Assistance" )]
    public static extern void StopRawInput();
    [DllImport( "Assistance" )]
    public static extern bool TryGetKeyEvent( out KeyEvent _keyEvent );

    [StructLayout( LayoutKind.Sequential )]
    public struct KeyEvent
    {
        public int    vKey;
        public KeyState keyState;
        public double timestamp;
    }

    private void Start()
    {
        if ( StartRawInput() ) Debug.Log( "Input Initialize Successed" );
        else                   Debug.Log( "Input Initialize Failed" );

    }

    private void OnDestroy()
    {
        StopRawInput();
    }

    private void Update()
    {
        while ( TryGetKeyEvent( out KeyEvent keyEvent ) )
        {
            Debug.Log( $"{keyEvent.vKey}  {keyEvent.keyState}  {keyEvent.timestamp}" );
        }
    }
}

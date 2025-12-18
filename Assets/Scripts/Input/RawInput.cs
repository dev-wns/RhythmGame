using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class RawInput : MonoBehaviour
{
    [DllImport( "Assistant" )]
    public static extern bool GetKeyEvent( out KeyEvent keyEvent );
    [DllImport( "Assistant" )]
    public static extern bool EnableRawInput( bool isEnable );
    [DllImport( "Assistant" )]
    private static extern bool GetPerformanceCounter( out long performanceCount );
    [DllImport( "Assistant" )]
    private static extern bool GetPerformanceFrequency( out long frequency );
    //[DllImport( "Assistant", CallingConvention = CallingConvention.Cdecl )]
    //public static extern bool NP_Start();
    //[DllImport( "Assistant", CallingConvention = CallingConvention.Cdecl )]
    //public static extern bool NP_Stop();
    //[DllImport( "Assistant", CallingConvention = CallingConvention.Cdecl )]
    //public static extern bool GetKeyEvent( out KeyEvent _keyEvent );
    //[DllImport( "Assistant", CallingConvention = CallingConvention.Cdecl )]
    //public static extern bool GetTimeFrame( out uint _frame );
    //[DllImport( "Assistant", CallingConvention = CallingConvention.Cdecl )]
    //public static extern bool GetInputFrame( out uint _frame );

    [StructLayout( LayoutKind.Sequential )]
    public struct KeyEvent
    {
        public int       vKey;
        public KeyState  keyState;
        public long      timestamp;
    }

    public TextMeshProUGUI mainFrame;
    public TextMeshProUGUI inputFrame;

    private long freq;
    private long startTick;

    private void Start()
    {
        if ( EnableRawInput( true ) )
        {
            GetPerformanceCounter( out startTick );
            GetPerformanceFrequency( out freq );
            Debug.Log( "Assistant.dll Initialize Successed" );
        }
        else Debug.Log( "Assistant.dll Initialize Failed" );
    }

    private void OnDestroy()
    {
        if ( EnableRawInput( false ) ) Debug.Log( "Assistant.dll Release Successed" );
        else                           Debug.Log( "Assistant.dll Release Failed" );
    }

    private void Update()
    {
        while ( GetKeyEvent( out KeyEvent keyEvent ) )
        {
            Debug.Log( $"{keyEvent.vKey}  {keyEvent.keyState}  {( long )( ( (double)keyEvent.timestamp - startTick ) / freq * 1000d )}" );
        }
    }
}

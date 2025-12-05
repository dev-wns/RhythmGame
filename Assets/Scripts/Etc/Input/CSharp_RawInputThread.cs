using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Tamporary
{
    public class RawInputThread : MonoBehaviour
    {
        // ==================== Win32 Interop ==================== //

        private delegate IntPtr WndProcDelegate( IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam );
        private const uint RID_INPUT = 0x10000003;
        private string className = "RawInputHiddenWindow";


        [StructLayout( LayoutKind.Sequential )]
        private struct RAWINPUTHEADER
        {
            public uint type;
            public uint size;
            public IntPtr device;
            public IntPtr wparam;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct RAWKEYBOARD
        {
            public ushort makeCode;
            public ushort flags;
            public ushort reserved;
            public ushort vKey;
            public uint message;
            public uint extraInfomation;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            //public RAWKEYBOARD keyboard;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct RAWINPUTDEVICE
        {
            public ushort usagePage;
            public ushort usage;
            public uint flags;
            public IntPtr hWndTarget;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct WNDCLASSEX
        {
            public int cbSize;
            public uint style;
            public WndProcDelegate lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;

        }

        [StructLayout( LayoutKind.Sequential )]
        private struct POINT
        {
            public int x;
            public int y;
        }
        [StructLayout( LayoutKind.Sequential )]
        private struct MSG
        {
            public IntPtr hWnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT point;
        }
        [DllImport( "Kernel32.dll" )] private static extern bool QueryPerformanceCounter( out long performanceCount );
        [DllImport( "Kernel32.dll" )] private static extern bool QueryPerformanceFrequency( out long frequency );
        [DllImport( "user32.dll" )] private static extern uint GetRawInputData( IntPtr hRawInput, uint command, IntPtr data, ref uint size, uint sizeHeader );
        [DllImport( "user32.dll" )] private static extern uint GetRawInputBuffer( IntPtr data, ref uint size, uint sizeHeader );
        [DllImport( "user32.dll" )] private static extern IntPtr DefWindowProc( IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam );
        [DllImport( "user32.dll" )] private static extern bool PeekMessage( out MSG msg, IntPtr hWnd, uint msgFilterMin, uint msgFilterMax, uint removeMsg );
        [DllImport( "user32.dll" )] private static extern bool TranslateMessage( ref MSG msg );
        [DllImport( "user32.dll" )] private static extern IntPtr DispatchMessage( ref MSG msg );
        [DllImport( "user32.dll" )] private static extern bool RegisterRawInputDevices( RAWINPUTDEVICE[] rawInputDevices, uint numDevices, uint size );
        [DllImport( "user32.dll" )] private static extern ushort RegisterClassEx( [In] ref WNDCLASSEX lpwcx );
        [DllImport( "user32.dll" )] private static extern bool UnregisterClass( string className, IntPtr hInstance );
        [DllImport( "user32.dll" )] private static extern bool DestroyWindow( IntPtr hWnd );
        [DllImport( "user32.dll" )]
        private static extern IntPtr CreateWindowEx(
            uint exStyle, string className, string windowName,
            uint style, int x, int y, int width, int height,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam );
        // ================================================== //

        public struct KeyEvent
        {
            public int vKey;
            public bool isDown;
            public long timestamp;
        }

        private const uint QS_RAWINPUT = 0x0400;
        private const uint INFINITE    = 0xFFFFFFFF;

        private IntPtr hiddenWnd;
        private WndProcDelegate wndProcDelegate;

        private Thread inputThread;
        private bool isRunning = true;
        private long startTick;

        public static ConcurrentQueue<KeyEvent> InputQueue = new();

        private void Start()
        {
            inputThread = new Thread( RawInputWorker );
            inputThread.IsBackground = true;
            inputThread.Priority = System.Threading.ThreadPriority.Highest;
            inputThread.Start();

            QueryPerformanceCounter( out startTick );

        }

        private void Update()
        {
            while ( InputQueue.TryDequeue( out var input ) )
            {
                if ( input.isDown )
                    Debug.Log( $"{input.vKey}  Key {( input.isDown ? "Down" : "Up" )}  {input.timestamp}" );
            }
        }

        private void OnDestroy()
        {
            isRunning = false;
            inputThread?.Join();

            if ( hiddenWnd != IntPtr.Zero )
            {
                DestroyWindow( hiddenWnd );
                hiddenWnd = IntPtr.Zero;
            }
            UnregisterClass( className, IntPtr.Zero );
        }

        private void RawInputWorker()
        {
            IntPtr hWnd = CreateHiddenWindow();
            RegisterDevice( hWnd );

            QueryPerformanceFrequency( out long frequency );
            uint headerSize = ( uint )Marshal.SizeOf( typeof( RAWINPUTHEADER ) );
            Debug.Log( "Input Thread Ω√¿€" );
            while ( isRunning )
            {
                uint bufferSize = 0;
                uint count      = GetRawInputBuffer( IntPtr.Zero, ref bufferSize, headerSize );
                if ( bufferSize == 0 )
                    continue;

                IntPtr buffer = Marshal.AllocHGlobal( ( int )bufferSize );
                try
                {
                    uint copies = GetRawInputBuffer( buffer, ref bufferSize, headerSize );

                    QueryPerformanceCounter( out long endTick );
                    double time = ( ( double )( endTick - startTick ) / frequency ) * 1000d;

                    int offset = 0;
                    int totalbytes = ( int )bufferSize;
                    while ( offset < totalbytes )
                    {
                        IntPtr curPtr = IntPtr.Add( buffer, offset );

                        RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>( curPtr );
                        int entrySize = ( int )header.size;
                        if ( entrySize <= 0 || offset + entrySize > totalbytes )
                            break;

                        IntPtr keyboardPtr = IntPtr.Add( curPtr, Marshal.SizeOf( typeof( RAWINPUTHEADER ) ) );
                        RAWKEYBOARD rk = Marshal.PtrToStructure<RAWKEYBOARD>( keyboardPtr );

                        if ( header.type == 1 ) // RIM_TYPEKEYBOARD
                            ProcessRawInput( rk, time );

                        offset += entrySize;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal( buffer );
                }
            }
        }

        private IntPtr CreateHiddenWindow()
        {
            wndProcDelegate = WndProc;

            WNDCLASSEX wndClass    = new WNDCLASSEX();
            wndClass.cbSize = Marshal.SizeOf( typeof( WNDCLASSEX ) );
            wndClass.lpfnWndProc = wndProcDelegate;
            wndClass.lpszClassName = className;
            RegisterClassEx( ref wndClass );

            hiddenWnd = CreateWindowEx( 0, className, "", 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero );
            Debug.Log( "Create Hidden Window" );
            return hiddenWnd;
        }

        private void RegisterDevice( IntPtr _hWnd )
        {
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
            rid[0].usagePage = 0x01; // Generic Desktop
            rid[0].usage = 0x06; // keyboard
            rid[0].flags = 0x00000100;
            rid[0].hWndTarget = _hWnd;

            RegisterRawInputDevices( rid, 1, ( uint ) Marshal.SizeOf( typeof( RAWINPUTDEVICE ) ) );
        }

        private IntPtr WndProc( IntPtr _hWnd, uint _msg, IntPtr _wParam, IntPtr _lParam )
        {
            Debug.Log( "Get Message " );
            //const uint WM_INPUT = 0x00FF;
            //if ( _msg == WM_INPUT )
            //     ProcessRawInput( _lParam );

            return DefWindowProc( _hWnd, _msg, _wParam, _lParam );
        }

        private readonly bool[] keyStates = new bool[256];
        private void ProcessRawInput( RAWKEYBOARD _input, double _time )
        {
            bool isDown = _input.message == 0x0100; // WM_KEYDOWN
            bool isUp   = _input.message == 0x0101; // WM_KEYUP
            int  vKey   = _input.vKey;

            if ( isDown )
            {
                if ( keyStates[vKey] )
                    return;

                keyStates[vKey] = true;
            }
            else if ( isUp )
            {
                keyStates[vKey] = false;
            }
            else return;

            InputQueue.Enqueue( new KeyEvent {
                vKey = vKey,
                isDown = isDown,
                timestamp = ( uint ) _time,
            } );
        }
    }
}
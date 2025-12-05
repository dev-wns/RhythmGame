using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Temprorary
{
    public class CSharp_RawInput : MonoBehaviour
    {
        public static CSharp_RawInput Inst;

        public struct RawKeyEvent
        {
            public KeyCode key;
            public bool isDown;
            public long timestamp;
        }

        public Queue<RawKeyEvent> InputQueue = new ();

        private const int RIDEV_INPUTSINK  = 0x00000100;
        private const int RIM_TYPEKEYBOARD = 1;
        private IntPtr hWnd;

        [StructLayout( LayoutKind.Sequential )]
        private struct RAWINPUTDEVICE
        {
            public ushort usagePage;
            public ushort usage;
            public int flags;
            public IntPtr hWndTarget;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct RAWINPUTHEADER
        {
            public int dwType;
            public int dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
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


        [DllImport( "user32.dll" )]
        private static extern IntPtr GetActiveWindow();
        [DllImport( "User32.dll" )]
        private static extern bool RegisterRawInputDevices( RAWINPUTDEVICE[] _rawInputDevice, uint _numDevices, uint _size );
        [DllImport( "User32.dll" )]
        private static extern uint GetRawInputData( IntPtr _hRawInput, uint _command, IntPtr _data, ref uint _size, uint sizeHeader );


        private void Awake()
        {
            Inst = this;
            RegisterKeyboardDevice();
        }

        private void RegisterKeyboardDevice()
        {
            hWnd = GetActiveWindow();

            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
            rid[0].usagePage = 0x01;
            rid[0].usage = 0x06; // keyboard
            rid[0].flags = RIDEV_INPUTSINK;
            rid[0].hWndTarget = hWnd;

            bool ok = RegisterRawInputDevices( rid, 1, ( uint )Marshal.SizeOf( typeof( RAWINPUTDEVICE ) ) );
            Debug.Log( $"RawInput Registered : {ok}" );
        }

        public void OnRawInput( IntPtr _rawInputPtr )
        {
            uint size = 0;
            GetRawInputData( _rawInputPtr, 0x10000003, IntPtr.Zero, ref size, ( uint ) Marshal.SizeOf( typeof( RAWINPUTHEADER ) ) );

            IntPtr buffer = Marshal.AllocHGlobal( ( int )size );
            if ( GetRawInputData( _rawInputPtr, 0x10000003, buffer, ref size, ( uint ) Marshal.SizeOf( typeof( RAWINPUTHEADER ) ) ) != size )
            {
                Marshal.FreeHGlobal( buffer );
                return;
            }

            RAWINPUTHEADER header = Marshal.PtrToStructure<RAWINPUTHEADER>( buffer );
            if ( header.dwType == RIM_TYPEKEYBOARD )
            {
                RAWKEYBOARD kb = Marshal.PtrToStructure<RAWKEYBOARD>( IntPtr.Add( buffer, Marshal.SizeOf( typeof( RAWINPUTHEADER ) ) ) );

                bool isDown = kb.message == 0x0100 || kb.message == 0x0104;
                bool isUp   = kb.message == 0x0101 || kb.message == 0x0105;

                if ( isDown || isUp )
                {
                    InputQueue.Enqueue( new RawKeyEvent {
                        key = ( KeyCode ) kb.vKey,
                        isDown = isDown,
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    } );

                    if ( isDown ) Debug.Log( $"{FreeStyleMainScroll.Playback}  {kb.vKey} Down" );
                    if ( isUp ) Debug.Log( $"{FreeStyleMainScroll.Playback}  {kb.vKey} Up" );
                }
            }

            Marshal.FreeHGlobal( buffer );
        }
    }
}
using FMOD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public enum SoundBuffer { _64, _128, _256, _512, _1024, Count, }
public enum SFX
{
    MainSelect, MainClick, MainHover, Slider,
    MenuSelect, MenuClick, MenuHover, MenuExit,
}

public struct SoundDriver : IEquatable<SoundDriver>
    {
        public int index; // OUTPUTTYPE에 해당하는 출력장치 인덱스
        public OUTPUTTYPE outputType;
        public Guid guid;
        public string name;
        public int systemRate, speakModeChannels;
        public SPEAKERMODE mode;

        public bool Equals( SoundDriver _other ) => index == _other.index;
        public override bool Equals( object _obj ) => Equals( ( SoundDriver )_obj );
        public override int GetHashCode() => base.GetHashCode();
    }

public enum ChannelType : byte { Master, Clap, BGM, SFX, Count, };
public class AudioManager : Singleton<AudioManager>
{
    private static readonly int MaxSoftwareChannel = 128;
    private static readonly int MaxVirtualChannel  = 1000;
    
    private FMOD.System system;
    private Dictionary<ChannelType, ChannelGroup> groups = new ();
    private Dictionary<DSP_TYPE, DSP>             dsps   = new ();

    public ReadOnlyCollection<SoundDriver> Drivers { get; private set; }
    public static Sound   MainSound                { get; private set; }
    public static Channel MainChannel              { get; private set; }

    public static event Action OnReload;
    public static event Action<float> OnUpdatePitch;

    [Header( "Property" )]
    public uint  Length => MainSound.getLength( out uint length, TIMEUNIT.MS ) == RESULT.OK ? length : uint.MaxValue;
    public uint  Position
    {
        get
        {
            ErrorCheck( MainChannel.getPosition( out uint pos, TIMEUNIT.MS ) );
            return pos;
        }

        set => ErrorCheck( MainChannel.setPosition( value, TIMEUNIT.MS ) );
    }
    public float Volume
    {
        get
        {
            if ( MainChannel.isPlaying( out bool isPlaying ) != RESULT.OK )
                 return 0f;
                
            ErrorCheck( MainChannel.getVolume( out float volume ) );
            return volume;
        }

        set => ErrorCheck( MainChannel.setVolume( value ) );
    }
    public bool  Pause
    {
        get
        {
            ErrorCheck( groups[ChannelType.BGM].getPaused( out bool isPause ) );
            return isPause;
        }

        set => ErrorCheck( groups[ChannelType.BGM].setPaused( value ) );
    }
    public float Pitch
    {
        set
        {
            ErrorCheck( groups[ChannelType.BGM].setPitch( value ) );
            UpdatePitchShift();

            OnUpdatePitch?.Invoke( value );
        }
    }
    public int   ChannelsInUse
    {
        get
        {
            ErrorCheck( system.getChannelsPlaying( out int channels ) );
            return channels;
        }
    }
    public bool  IsStop { get; private set; }
    public int CurrentDriverIndex
    {
        get => curDriverIndex;
        set
        {
            if ( Drivers.Count <= value || curDriverIndex == value )
            {
                Debug.LogWarning( "SoundDriver Index is Out of Range or Duplicated Value" );
                return;
            }

            ErrorCheck( system.setOutput( Drivers[value].outputType ) );
            ErrorCheck( system.setDriver( Drivers[value].index ) );
            curDriverIndex = value;
        }
    }
    private int curDriverIndex = -1;


    #region System
    public void Initialize()
    {
        // System
        ErrorCheck( Factory.System_Create( out system ) );
        ErrorCheck( system.setOutput( OUTPUTTYPE.AUTODETECT ) );

        // To do Before System Initialize
        ErrorCheck( system.getSoftwareFormat( out int sampleRate, out SPEAKERMODE mode, out int numRawSpeakers ) );
        ErrorCheck( system.setSoftwareFormat( sampleRate, SPEAKERMODE.STEREO, numRawSpeakers ) );
        ErrorCheck( system.setSoftwareChannels( MaxSoftwareChannel ) );
        ErrorCheck( system.setDSPBufferSize( uint.Parse( SystemSetting.CurrentSoundBufferString ), 4 ) );

        // System Initialize
        IntPtr extraDriverData = new IntPtr();
        ErrorCheck( system.init( MaxVirtualChannel, INITFLAGS.NORMAL, extraDriverData ) );

        ErrorCheck( system.getVersion( out uint version ) );
        if ( version < VERSION.number )
             Debug.LogWarning( "Using the old version" );

        // Sound Driver
        List<SoundDriver> drivers = new List<SoundDriver>();
        CreateDriverInfo( OUTPUTTYPE.WASAPI, drivers );
        //MakeDriverInfomation( OUTPUTTYPE.ASIO,   drivers );
        Drivers = new ReadOnlyCollection<SoundDriver>( drivers );
        if ( curDriverIndex < 0 )
        {
            ErrorCheck( system.getDriver( out int driverIndex ) );
            ErrorCheck( system.getDriverInfo( driverIndex, out string name, 256, out Guid guid, out int rate,
                                              out SPEAKERMODE driverSpeakMode, out int channels ) );
            for ( int i = 0; i < Drivers.Count; i++ )
            {
                if ( guid.CompareTo( Drivers[i].guid ) == 0 )
                {
                    curDriverIndex = i;
                    break;
                }
            }
        }

        // ChannelGroup
        for ( int i = 0; i < ( int )ChannelType.Count; i++ )
        {
            ChannelType type = ( ChannelType )i;
            ErrorCheck( system.createChannelGroup( type.ToString(), out ChannelGroup group ) );
            if ( type != ChannelType.Master )
                ErrorCheck( groups[ChannelType.Master].addGroup( group ) );

            groups.Add( type, group );
        }

        // DSP
        if ( !dsps.ContainsKey( DSP_TYPE.FFT ) )
        {
            ErrorCheck( system.createDSPByType( DSP_TYPE.FFT, out DSP dsp ) );
            ErrorCheck( dsp.setParameterInt( ( int ) DSP_FFT.WINDOWSIZE, 4096 ) );
            ErrorCheck( dsp.setParameterInt( ( int ) DSP_FFT.WINDOWTYPE, ( int ) DSP_FFT_WINDOW.BLACKMANHARRIS ) );
            dsps.Add( DSP_TYPE.FFT, dsp );
        }

        if ( !dsps.ContainsKey( DSP_TYPE.PITCHSHIFT ) )
        {
            ErrorCheck( system.createDSPByType( DSP_TYPE.PITCHSHIFT, out DSP dsp ) );
            ErrorCheck( dsp.setParameterFloat( ( int ) DSP_PITCHSHIFT.MAXCHANNELS, 0 ) );
            ErrorCheck( dsp.setParameterFloat( ( int ) DSP_PITCHSHIFT.FFTSIZE, 2048 ) );
            dsps.Add( DSP_TYPE.PITCHSHIFT, dsp );
        }

        // Details
        SetVolume( 1f, ChannelType.Master );
        SetVolume( .15f, ChannelType.BGM  );
        SetVolume( .3f, ChannelType.SFX   );
        SetVolume( .8f, ChannelType.Clap  );

        Debug.Log( $"AudioManager Initialization" );
        Debug.Log( $"Sound Device : {Drivers[curDriverIndex].name}" );
    }

    private void CreateDriverInfo( OUTPUTTYPE _type, List<SoundDriver> _list )
    {
        ErrorCheck( system.getOutput( out OUTPUTTYPE prevType ) );
        ErrorCheck( system.setOutput( _type ) );

        ErrorCheck( system.getNumDrivers( out int numDriver ) );
        for ( int i = 0; i < numDriver; i++ )
        {
            SoundDriver driver;
            if ( ErrorCheck( system.getDriverInfo( i, out driver.name, 256, out driver.guid, out driver.systemRate, out driver.mode, out driver.speakModeChannels ) ) )
            {
                driver.index = i;
                driver.outputType = _type;
                _list.Add( driver );
            }
        }

        ErrorCheck( system.setOutput( prevType ) );
    }


    public void Release()
    {
        IsStop = true;

        // Sounds
        //foreach ( var sfx in sfxSounds.Values )
        //{
        //    if ( sfx.hasHandle() )
        //    {
        //        ErrorCheck( sfx.release() );
        //        sfx.clearHandle();
        //    }
        //}
        //sfxSounds.Clear();

        // ChannelGroup
        AllStop();
        for ( int i = 1; i < ( int )ChannelType.Count; i++ )
        {
            foreach ( var dsp in dsps.Values )
            {
                ErrorCheck( groups[( ChannelType )i].removeDSP( dsp ) );
            }

            //ErrorCheck( groups[( ChannelType ) i].getNumChannels( out int numChannels ) );
            //for ( int j = 0; j < numChannels; j++ )
            //{
            //    ErrorCheck( groups[( ChannelType ) i].getChannel( j, out Channel channel ) );
            //    ErrorCheck( channel.stop() );
            //    ErrorCheck( channel.getCurrentSound( out Sound sound ) );
            //    ErrorCheck( sound.release() );
            //    sound.clearHandle();
            //}

            ErrorCheck( groups[( ChannelType )i].release() );
        }

        ErrorCheck( groups[ChannelType.Master].release() );
        groups.Clear();

        // DSP
        foreach ( var dsp in dsps.Values )
        {
            ErrorCheck( dsp.release() );
            dsp.clearHandle();
        }
        dsps.Clear();

        // System
        ErrorCheck( system.release() ); // 내부에서 close 함.
        system.clearHandle();

        Debug.Log( "AudioManager Release" );
    }

    public void ReLoad()
    {
        IsStop = true;
        AllStop();

        // caching
        float[] volumes = new float[groups.Count];
        int groupCount = 0;
        foreach ( var group in groups.Values )
            ErrorCheck( group.getVolume( out volumes[groupCount++] ) );

        // reload
        Release();
        Initialize();
        OnReload?.Invoke();

        // rollback
        ErrorCheck( system.setOutput( Drivers[curDriverIndex].outputType ) );
        ErrorCheck( system.setDriver( Drivers[curDriverIndex].index ) );
        groupCount = 0;
        foreach ( var group in groups.Values )
            ErrorCheck( group.setVolume( volumes[groupCount++] ) );

        IsStop = false;
    }
    #endregion

    #region Unity Event Function
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    public void SystemUpdate()
    {
        if ( !IsStop )
             system.update();
    }

    private void OnApplicationQuit()
    {
        IsStop = true;
    }

    private void OnDestroy()
    {
        // OnApplicationQuit -> OnDisable -> OnDestroy 순으로 호출 되기 때문에
        // 타 클래스에서 OnDisable, OnApplicationQuit로 사운드 관련 처리를 마친 후
        // AudioManager OnDestroy가 실행될 수 있도록 한다.
        Release();
    }
    #endregion

    public bool Load( string _path, out Sound _sound, bool _isStream = false )
    {
        _sound = new Sound();
        if ( !System.IO.File.Exists( @_path ) )
              return false;

        MODE mode  = _isStream     ? MODE.CREATESTREAM : MODE.CREATESAMPLE;
             mode |= MODE.LOOP_OFF | MODE.ACCURATETIME | MODE.LOWMEM; // | MODE.VIRTUAL_PLAYFROMSTART

        ErrorCheck( system.createSound( _path, mode, out _sound ) );
        MainSound = _sound;
        return true;
    }

    public void Play( Sound _sound, float _volume = 1f )
    {
        if ( ErrorCheck( system.playSound( _sound, groups[ChannelType.BGM], true, out Channel channel ) ) )
        {
            MainChannel = channel;
            ErrorCheck( channel.setVolume( _volume ) );
            ErrorCheck( channel.setPaused(  false  ) );
        }
    }


    public void Play( SFX _type )
    {
        if ( DataStorage.Inst.GetSound( _type, out Sound sound ) )
             ErrorCheck( system.playSound( sound, groups[ChannelType.SFX], false, out Channel channel ) );
    }

    public void Fade( Channel _channel, float _start, float _end, float _t, Action _OnCompleted = null )
    {
        // https://qa.fmod.com/t/fmod-isplaying-question-please-help/11481
        // isPlaying이 INVALID_HANDLE을 반환할 때 false와 동일하게 취급한다.
        if ( _channel.isPlaying( out bool isPlaying ) != RESULT.OK )
             return;
        StartCoroutine( FadeVolume( _channel, _start, _end, _t, _OnCompleted ) );
    }

    public IEnumerator FadeVolume( Channel _channel, float _start, float _end, float _t, Action _OnCompleted )
    {
        // 같은 값일 때 계산 없이 종료
        if ( Global.Math.Abs( _start - _end ) < float.Epsilon )
        {
            ErrorCheck( _channel.setVolume( _end ) );
            yield break;
        }

        float elapsedVolume = _start;
        float offset = _end - _start;
        ErrorCheck( _channel.setVolume( _start ) );
        while ( _start < _end ? elapsedVolume < _end : // FADEIN
                                elapsedVolume > _end ) // FADEOUT
        {
            yield return YieldCache.WaitForEndOfFrame;
            elapsedVolume += ( offset * Time.deltaTime ) / _t;
            ErrorCheck( _channel.setVolume( elapsedVolume ) );
        }

        // 페이드 인 기준으로 반복문이 끝난 시점에서 볼륨이 _end 값을 넘어가기 때문에 초기화해준다.
        ErrorCheck( _channel.setVolume( _end ) );
        _OnCompleted?.Invoke();
    }

    public void PitchReset()
    {
        foreach ( var group in groups.Values )
        {
            ErrorCheck( group.setPitch( 1f ) );
            UpdatePitchShift();
        }
    }

    public float GetVolume( ChannelType _type )
    {
        ErrorCheck( groups[_type].getVolume( out float chlVolume ) );
        return chlVolume;
    }

    public void SetVolume( float _value, ChannelType _type )
    {
        float chlVolume = _value < 0f ? 0f :
                          _value > 1f ? 1f : _value;

        ErrorCheck( groups[_type].setVolume( chlVolume ) );
    }

    public void Release( Sound _sound )
    {
        if ( _sound.hasHandle() )
        {
            ErrorCheck( _sound.release() );
            _sound.clearHandle();
        }
    }

    public void AllStop()
    {
        foreach ( var group in groups )
        {
            if ( groups[group.Key].isPlaying( out bool isPlaying ) == RESULT.OK ) 
                 ErrorCheck( group.Value.stop() );
        }
    }

    #region DSP
    public bool GetDSP( DSP_TYPE _type, out DSP _dsp )
    {
        if ( !dsps.ContainsKey( _type ) )
        {
            Debug.LogError( "DSP is not loaded" );
            _dsp = new DSP();
            return false;
        }

        _dsp = dsps[_type];
        return true;
    }

    public void AddDSP( DSP_TYPE _dspType, ChannelType _channelType )
    {
        if ( !dsps.ContainsKey( _dspType ) )
             return;

        ErrorCheck( groups[_channelType].addDSP( CHANNELCONTROL_DSP_INDEX.TAIL, dsps[_dspType] ) );
    }

    public void RemoveDSP( DSP_TYPE _dspType, ChannelType _channelType )
    {
        if ( !dsps.ContainsKey( _dspType ) )
            return;

        ErrorCheck( groups[_channelType].removeDSP( dsps[_dspType] ) );
    }

    public void UpdatePitchShift()
    {
        if ( dsps.ContainsKey( DSP_TYPE.PITCHSHIFT ) )
        {
            float offset = GameSetting.CurrentPitchType == PitchType.Normalize ? 1f    / GameSetting.CurrentPitch :
                           GameSetting.CurrentPitchType == PitchType.Nightcore ? 1.1f  / GameSetting.CurrentPitch : 1f;

            ErrorCheck( dsps[DSP_TYPE.PITCHSHIFT].setParameterFloat( ( int ) DSP_PITCHSHIFT.PITCH, offset ) );
        }
    }
    #endregion

    private static bool ErrorCheck( RESULT _result )
    {
        #if UNITY_EDITOR
        if ( RESULT.OK != _result )
        {
            Debug.LogError( FMOD.Error.String( _result ) );
            return false;
        }

        return true;
        #else
        // 임시 로직.
        return FMOD.RESULT.OK == _result;
        // 에러일시 로그 파일 생성 후 파일에 쓰도록 구현하기
        #endif
    }
}

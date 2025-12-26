using System;

public enum ChannelType : byte { Master, Clap, BGM, SFX, Count, };
public enum SFX
{
    MainSelect, MainClick, MainHover, Slider,
    MenuSelect, MenuClick, MenuHover, MenuExit,
    keyboard_Input, Keyboard_Backspace,
}

public struct AudioGroup
{
    public FMOD.Sound   sound;
    public FMOD.Channel channel;

    public void Release()
    {
        channel.stop();
        AudioManager.Inst.Release( sound );
    }

    public AudioGroup( FMOD.Sound _sound, FMOD.Channel _channel )
    {
        sound = _sound;
        channel = _channel;
    }
}

public struct SoundDriver : IEquatable<SoundDriver>
{
    public int index; // OUTPUTTYPE에 해당하는 출력장치 인덱스
    public FMOD.OUTPUTTYPE outputType;
    public Guid guid;
    public string name;
    public int systemRate, speakModeChannels;
    public FMOD.SPEAKERMODE mode;

    public bool Equals( SoundDriver _other )   => index == _other.index;
    public override bool Equals( object _obj ) => Equals( ( SoundDriver ) _obj );
    public override int GetHashCode()          => base.GetHashCode();
}
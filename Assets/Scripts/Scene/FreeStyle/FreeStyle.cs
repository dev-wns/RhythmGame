using System;
using UnityEngine;

public class FreeStyle : Scene
{
    public GameObject exit;
    public GameObject gameSetting;
    public GameObject systemSetting;
    public FreeStyleKeySetting keySetting;
    public FreeStyleReLoad reload;
    public FreeStyleSearch search;

    //public event Action OnChangedSpeed;

    protected override void Awake()
    {
        base.Awake();
        // ½Ì±ÛÅæ È°¼ºÈ­
        NowPlaying    nowPlaying    = NowPlaying.Inst;
        Config        config        = Config.Inst;
        GameSetting   gameSetting   = GameSetting.Inst;
        SystemSetting systemSetting = SystemSetting.Inst;
        Network       network       = Network.Inst;
        AudioManager  audioManager  = AudioManager.Inst;
        InputManager  inputManager  = InputManager.Inst;
        DataStorage   dataStorage   = DataStorage.Inst;
        Judgement     judgement     = Judgement.Inst;
        
        AudioManager.OnReload += Connect;
        OnKeyBind += KeyBind;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        AudioManager.OnReload -= Connect;
        OnKeyBind -= KeyBind;
    }

    protected override void Connect()
    {
        AudioManager.Inst.Pitch = GameSetting.Pitch;
        AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.FFT,        ChannelType.BGM );
    }

    protected override void Disconnect()
    {
        AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.FFT,        ChannelType.BGM );
    }

    //private void ScrollSpeedControl( bool _isRise )
    //{
    //    if ( _isRise )
    //    {
    //        if ( GameSetting.ScrollSpeed >= 50 )
    //            return;

    //        AudioManager.Inst.Play( SFX.Slider );
    //        GameSetting.ScrollSpeed += 1;
    //        OnChangedSpeed?.Invoke();
    //    }
    //    else
    //    {
    //        if ( GameSetting.ScrollSpeed <= 1 )
    //            return;

    //        AudioManager.Inst.Play( SFX.Slider );
    //        GameSetting.ScrollSpeed -= 1;
    //        OnChangedSpeed?.Invoke();
    //    }
    //}

    public void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.Main, KeyCode.F3, new InputRepeater( () => { GameSetting.ScrollSpeed -= 1; } ) );
        _input.Bind( SceneAction.Main, KeyCode.F4, new InputRepeater( () => { GameSetting.ScrollSpeed += 1; } ) );
    }
}
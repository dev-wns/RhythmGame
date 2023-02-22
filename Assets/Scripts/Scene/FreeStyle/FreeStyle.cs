using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FreeStyle : Scene
{
    public OptionController    gameSetting, systemSetting, exit;
    public FreeStyleKeySetting keySetting;
    public GameObject          reload;
    public GameObject          search;
    public RecordSystem        record;
    public TextMeshProUGUI speedText;

    protected override void Awake()
    {
        base.Awake();

        SoundManager.Inst.OnReload += Connect;

        var judge = GameObject.FindGameObjectWithTag( "Judgement" );
        if ( judge ) Destroy( judge );

        OnScrollChange += () => speedText.text = $"{GameSetting.ScrollSpeed:F1}";
    }

    public override void Connect()
    {
        SoundManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.BGM );
        SoundManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT,   ChannelType.BGM );
        SoundManager.Inst.AddDSP( FMOD.DSP_TYPE.FFT,          ChannelType.BGM );
    }

    public override void Disconnect()
    {
        SoundManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        SoundManager.Inst.RemoveDSP( FMOD.DSP_TYPE.FFT,        ChannelType.BGM );
    }

    public void Quit() => Application.Quit();

    public void ExitCancel() => DisableCanvas( ActionType.Main, exit );

    public override void KeyBind()
    {
        // Main
        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Main, InputType.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );

        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Main, InputType.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );

        // GameSetting
        Bind( ActionType.Main,       KeyCode.Space,     () => { EnableCanvas(  ActionType.GameOption, gameSetting ); } );
        Bind( ActionType.GameOption, KeyCode.Space,     () => { DisableCanvas( ActionType.Main,       gameSetting ); } );
        Bind( ActionType.GameOption, KeyCode.Escape,    () => { DisableCanvas( ActionType.Main,       gameSetting ); } );
        Bind( ActionType.GameOption, KeyCode.DownArrow, () => { MoveToNextOption( gameSetting ); } );
        Bind( ActionType.GameOption, KeyCode.UpArrow,   () => { MoveToPrevOption( gameSetting ); } );

        // SystemSetting
        Bind( ActionType.Main,         KeyCode.F10,       () => { EnableCanvas(  ActionType.SystemOption, systemSetting, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.F10,       () => { DisableCanvas( ActionType.Main,         systemSetting, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.Space,     () => { DisableCanvas( ActionType.Main,         systemSetting, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.Escape,    () => { DisableCanvas( ActionType.Main,         systemSetting, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.DownArrow, () => { MoveToNextOption( systemSetting ); } );
        Bind( ActionType.SystemOption, KeyCode.UpArrow,   () => { MoveToPrevOption( systemSetting ); } );

        // KeySetting
        Bind( ActionType.Main,       KeyCode.F11,        () => { EnableCanvas(  ActionType.KeySetting, keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.F11,        () => { DisableCanvas( ActionType.Main,       keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.Escape,     () => { DisableCanvas( ActionType.Main,       keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.RightArrow, () => { MoveToNextOption( keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.LeftArrow,  () => { MoveToPrevOption( keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.Tab,                keySetting.ChangeButtonCount );

        // Search
        Bind( ActionType.Main,   KeyCode.F2,     () => { EnableCanvas(  ActionType.Search, search ); } );
        Bind( ActionType.Search, KeyCode.F2,     () => { DisableCanvas( ActionType.Main,   search ); } );
        Bind( ActionType.Search, KeyCode.Escape, () => { DisableCanvas( ActionType.Main,   search ); } );

        // ReLoad
        Bind( ActionType.Main,   KeyCode.F5,     () => { EnableCanvas(  ActionType.ReLoad, reload ); } );
        Bind( ActionType.ReLoad, KeyCode.Escape, () => { DisableCanvas( ActionType.Main,   reload ); } );

        // Record
        Bind( ActionType.Main, KeyCode.Tab, record.HideRecordInfomation );

        // Exit
        Bind( ActionType.Main, KeyCode.Escape,     () => { EnableCanvas( ActionType.Exit, exit ); } );
        Bind( ActionType.Exit, KeyCode.Escape,             ExitCancel );
        Bind( ActionType.Exit, KeyCode.RightArrow, () => { MoveToNextOption( exit ); } );
        Bind( ActionType.Exit, KeyCode.LeftArrow,  () => { MoveToPrevOption( exit ); } );
    }
}
using TMPro;
using UnityEngine;

public class FreeStyle : Scene
{
    public OptionController    gameSetting, systemSetting, exit;
    public FreeStyleKeySetting keySetting;
    public FreeStyleReLoad     reload;
    public FreeStyleSearch     search;
    public FreeStyleComment    comment;
    public RecordSystem        record;

    public IconController gameIcon, systemIcon, keyIcon, exitIcon, reloadIcon, searchIcon, commentIcon, recordIcon;
    public TextMeshProUGUI speedText;

    protected override void Awake()
    {
        base.Awake();

        QualitySettings.antiAliasing = 0;

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

    public void ExitCancel() => DisableCanvas( ActionType.Main, exit, exitIcon );

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
        Bind( ActionType.Main,       KeyCode.Space,     () => { EnableCanvas(  ActionType.GameOption, gameSetting, gameIcon ); } );
        Bind( ActionType.GameOption, KeyCode.Space,     () => { DisableCanvas( ActionType.Main,       gameSetting, gameIcon ); } );
        Bind( ActionType.GameOption, KeyCode.Escape,    () => { DisableCanvas( ActionType.Main,       gameSetting, gameIcon ); } );
        Bind( ActionType.GameOption, KeyCode.DownArrow, () => { MoveToNextOption( gameSetting ); } );
        Bind( ActionType.GameOption, KeyCode.UpArrow,   () => { MoveToPrevOption( gameSetting ); } );

        // SystemSetting
        Bind( ActionType.Main,         KeyCode.F10,       () => { EnableCanvas(  ActionType.SystemOption, systemSetting, systemIcon, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.F10,       () => { DisableCanvas( ActionType.Main,         systemSetting, systemIcon, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.Space,     () => { DisableCanvas( ActionType.Main,         systemSetting, systemIcon, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.Escape,    () => { DisableCanvas( ActionType.Main,         systemSetting, systemIcon, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.DownArrow, () => { MoveToNextOption( systemSetting ); } );
        Bind( ActionType.SystemOption, KeyCode.UpArrow,   () => { MoveToPrevOption( systemSetting ); } );

        // KeySetting
        Bind( ActionType.Main,       KeyCode.F11,        () => { EnableCanvas(  ActionType.KeySetting, keySetting, keyIcon ); } );
        Bind( ActionType.KeySetting, KeyCode.F11,        () => { DisableCanvas( ActionType.Main,       keySetting, keyIcon ); } );
        Bind( ActionType.KeySetting, KeyCode.Escape,     () => { DisableCanvas( ActionType.Main,       keySetting, keyIcon ); } );
        Bind( ActionType.KeySetting, KeyCode.RightArrow, () => { MoveToNextOption( keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.LeftArrow,  () => { MoveToPrevOption( keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.Tab,                keySetting.ChangeButtonCount );

        // Search
        Bind( ActionType.Main,   KeyCode.F2,     () => { EnableCanvas(  ActionType.Search, search.canvas, searchIcon ); } );
        Bind( ActionType.Main,   KeyCode.F2,             search.EnableInputField  );
        Bind( ActionType.Search, KeyCode.F2,             search.DisableInputField );
        Bind( ActionType.Search, KeyCode.F2,     () => { DisableCanvas( ActionType.Main, search.canvas, searchIcon ); } );
        Bind( ActionType.Search, KeyCode.Escape,         search.DisableInputField );
        Bind( ActionType.Search, KeyCode.Escape, () => { DisableCanvas( ActionType.Main, search.canvas, searchIcon ); } );

        // Comment
        Bind( ActionType.Main, KeyCode.F3,        () => { EnableCanvas( ActionType.Comment, comment.canvas, commentIcon ); } );
        Bind( ActionType.Main, KeyCode.F3,        comment.EnableInputField  );
        Bind( ActionType.Comment, KeyCode.F3,     () => { DisableCanvas( ActionType.Main, comment.canvas, commentIcon ); } );
        Bind( ActionType.Comment, KeyCode.F3,     comment.DisableInputField );
        Bind( ActionType.Comment, KeyCode.F3,     comment.ReviseComment );
        Bind( ActionType.Comment, KeyCode.Escape, () => { DisableCanvas( ActionType.Main, comment.canvas, commentIcon ); } );
        Bind( ActionType.Comment, KeyCode.Escape, comment.DisableInputField );
        Bind( ActionType.Comment, KeyCode.Escape, comment.ReviseComment );

        // ReLoad
        Bind( ActionType.Main,   KeyCode.F5,     () => { EnableCanvas(  ActionType.ReLoad, reload.gameObject, reloadIcon ); } );
        Bind( ActionType.ReLoad, KeyCode.Escape, () => { DisableCanvas( ActionType.Main,   reload.gameObject, reloadIcon ); } );

        // Record
        Bind( ActionType.Main, KeyCode.Tab, record.HideRecordInfomation );

        // Exit
        Bind( ActionType.Main, KeyCode.Escape,     () => { EnableCanvas( ActionType.Exit, exit, exitIcon ); } );
        Bind( ActionType.Exit, KeyCode.Escape,             ExitCancel );
        Bind( ActionType.Exit, KeyCode.RightArrow, () => { MoveToNextOption( exit ); } );
        Bind( ActionType.Exit, KeyCode.LeftArrow,  () => { MoveToPrevOption( exit ); } );
    }
}
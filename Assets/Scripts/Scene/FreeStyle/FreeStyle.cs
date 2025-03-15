using TMPro;
using UnityEngine;

public class FreeStyle : Scene
{
    public GameObject exit;
    public GameObject gameSetting;
    public GameObject systemSetting;
    public FreeStyleKeySetting keySetting;
    public FreeStyleReLoad     reload;
    public FreeStyleSearch     search;
    public FreeStyleComment    comment;

    public TextMeshProUGUI speedText;

    protected override void Awake()
    {
        base.Awake();

        if ( !Network.Inst.IsConnected )
             Network.Inst.Connect( "127.0.0.1" );

        QualitySettings.antiAliasing = 0;

        AudioManager.Inst.OnReload += Connect;

        var judge = GameObject.FindGameObjectWithTag( "Judgement" );
        if ( judge ) Destroy( judge );

        OnScrollChange += () => speedText.text = $"{GameSetting.ScrollSpeed:F1}";
    }

    public override void Connect()
    {
        AudioManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.BGM );
        AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT,   ChannelType.BGM );
        AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.FFT,          ChannelType.BGM );
        Debug.Log( $"Applied DSP : {AudioManager.Inst.GetAppliedDSPName()}" );
    }

    public override void Disconnect()
    {
        AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.FFT,        ChannelType.BGM );
    }

    public void Quit() => Application.Quit();

    public void ExitCancel() => DisableCanvas( ActionType.Main, exit );

    #region Mouse Click ( Enable Canvas )
    public void EnableGameSettingCanvas()   => EnableCanvas( ActionType.GameOption, gameSetting );
    public void EnableSystemSettingCanvas() => EnableCanvas( ActionType.SystemOption, systemSetting, true, false );
    public void EnableKeySettingCanvas()    => EnableCanvas( ActionType.KeySetting, keySetting );
    public void EnableReloadCanvas()        => EnableCanvas( ActionType.ReLoad, reload.gameObject );
    public void EnableExitCanvas()          => EnableCanvas( ActionType.Exit, exit );
    public void MoveToMultiPlay()
    {
        AudioManager.Inst.Play( SFX.MenuClick );
        LoadScene( SceneType.MultiPlay );
    }

    public void EnableSearchCanvas()
    {
        EnableCanvas( ActionType.Search, search.canvas );
        search.EnableInputField();
    }
    public void EnableCommentCanvas()
    {
        EnableCanvas( ActionType.Comment, comment.canvas );
        comment.EnableInputField();
    }
    #endregion

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
        Bind( ActionType.Main,       KeyCode.Space,     () => { EnableCanvas( ActionType.GameOption, gameSetting ); } );
        Bind( ActionType.GameOption, KeyCode.Space,     () => { DisableCanvas( ActionType.Main,       gameSetting ); } );
        Bind( ActionType.GameOption, KeyCode.Escape,    () => { DisableCanvas( ActionType.Main,       gameSetting ); } );
        //Bind( ActionType.GameOption, KeyCode.DownArrow, () => { MoveToNextOption( gameSetting ); } );
        //Bind( ActionType.GameOption, KeyCode.UpArrow,   () => { MoveToPrevOption( gameSetting ); } );

        // SystemSetting
        Bind( ActionType.Main,         KeyCode.F10,       () => { EnableCanvas(  ActionType.SystemOption, systemSetting, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.F10,       () => { DisableCanvas( ActionType.Main,         systemSetting, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.Space,     () => { DisableCanvas( ActionType.Main,         systemSetting, true, false ); } );
        Bind( ActionType.SystemOption, KeyCode.Escape,    () => { DisableCanvas( ActionType.Main,         systemSetting, true, false ); } );
        //Bind( ActionType.SystemOption, KeyCode.DownArrow, () => { MoveToNextOption( systemSetting ); } );
        //Bind( ActionType.SystemOption, KeyCode.UpArrow,   () => { MoveToPrevOption( systemSetting ); } );

        // KeySetting
        Bind( ActionType.Main,       KeyCode.F11,        () => { EnableCanvas(  ActionType.KeySetting, keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.F11,        () => { DisableCanvas( ActionType.Main,       keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.Escape,     () => { DisableCanvas( ActionType.Main,       keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.RightArrow, () => { MoveToNextOption( keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.LeftArrow,  () => { MoveToPrevOption( keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.Tab,                keySetting.ChangeButtonCount );

        // Search
        Bind( ActionType.Main,   KeyCode.F2,     () => { EnableCanvas(  ActionType.Search, search.canvas ); } );
        Bind( ActionType.Main,   KeyCode.F2,             search.EnableInputField  );
        Bind( ActionType.Search, KeyCode.F2,             search.DisableInputField );
        Bind( ActionType.Search, KeyCode.F2,     () => { DisableCanvas( ActionType.Main, search.canvas ); } );
        Bind( ActionType.Search, KeyCode.Escape,         search.DisableInputField );
        Bind( ActionType.Search, KeyCode.Escape, () => { DisableCanvas( ActionType.Main, search.canvas ); } );

        // Comment
        Bind( ActionType.Main,    KeyCode.F3,     () => { EnableCanvas( ActionType.Comment, comment.canvas ); } );
        Bind( ActionType.Main,    KeyCode.F3,     comment.EnableInputField );
        Bind( ActionType.Comment, KeyCode.F3,     () => { DisableCanvas( ActionType.Main, comment.canvas ); } );
        Bind( ActionType.Comment, KeyCode.F3,     comment.DisableInputField );
        Bind( ActionType.Comment, KeyCode.F3,     comment.ReviseComment );
        Bind( ActionType.Comment, KeyCode.Escape, () => { DisableCanvas( ActionType.Main, comment.canvas ); } );
        Bind( ActionType.Comment, KeyCode.Escape, comment.DisableInputField );
        Bind( ActionType.Comment, KeyCode.Escape, comment.ReviseComment );

        //ReLoad
        Bind( ActionType.Main,   KeyCode.F5,     () => { EnableCanvas( ActionType.ReLoad, reload.gameObject ); } );
        Bind( ActionType.ReLoad, KeyCode.Escape, () => { DisableCanvas( ActionType.Main, reload.gameObject ); } );

        //// Record
        //Bind( ActionType.Main, KeyCode.Tab, record.HideRecordInfomation );

        // Exit
        Bind( ActionType.Main, KeyCode.Escape,     () => { EnableCanvas( ActionType.Exit, exit ); } );
        Bind( ActionType.Exit, KeyCode.Escape,             ExitCancel );
        //Bind( ActionType.Exit, KeyCode.RightArrow, () => { MoveToNextOption( exit ); } );
        //Bind( ActionType.Exit, KeyCode.LeftArrow,  () => { MoveToPrevOption( exit ); } );
    }
}
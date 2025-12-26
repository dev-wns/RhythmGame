using Cysharp.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine;

public class FreeStyle : Scene
{
    public GameObject exit;
    public GameObject gameSetting;
    public GameObject systemSetting;
    public FreeStyleKeySetting keySetting;
    public FreeStyleReLoad reload;
    public FreeStyleSearch search;


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
    }

    protected override void Start()
    {
        base.Start();
        AudioManager.OnReload += Connect;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        AudioManager.OnReload -= Connect;
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

    #region Bind Unity Events
    public void EnableGameSettingCanvas()   => EnableCanvas( SceneAction.GameOption, gameSetting );
    public void EnableSystemSettingCanvas() => EnableCanvas( SceneAction.SystemOption, systemSetting, true, false );
    public void EnableKeySettingCanvas()    => EnableCanvas( SceneAction.KeySetting, keySetting );
    public void EnableReloadCanvas()        => EnableCanvas( SceneAction.ReLoad, reload.gameObject );
    public void EnableExitCanvas()          => EnableCanvas( SceneAction.Exit, exit );
    public void DisableGameSetting()
    {
        DisableCanvas( SceneAction.Main, gameSetting );

        // Game Mode
        Config.Inst.Write( ConfigType.Random,       ( int )GameSetting.CurrentRandom );
        Config.Inst.Write( ConfigType.AutoPlay,     GameSetting.HasFlag( GameMode.AutoPlay   ) );
        Config.Inst.Write( ConfigType.NoFailed,     GameSetting.HasFlag( GameMode.NoFail     ) );
        Config.Inst.Write( ConfigType.NoSlider,     GameSetting.HasFlag( GameMode.NoSlider   ) );
        Config.Inst.Write( ConfigType.FixedBPM,     GameSetting.HasFlag( GameMode.FixedBPM   ) );
        Config.Inst.Write( ConfigType.ConvertKey,   GameSetting.HasFlag( GameMode.ConvertKey ) );

        // Game Setting
        Config.Inst.Write( ConfigType.NoteModel,    ( int )GameSetting.CurrentNoteModel );
        Config.Inst.Write( ConfigType.ScrollSpeed,  GameSetting.ScrollSpeed  );
        Config.Inst.Write( ConfigType.JudgeOffset,  GameSetting.JudgeOffset  );
        Config.Inst.Write( ConfigType.SliderOffset, GameSetting.SliderOffset );

        // Sound
        Config.Inst.Write( ConfigType.SoundOffset,  GameSetting.SoundOffset );

        // UI
        Config.Inst.Write( ConfigType.BGAOpacity,   GameSetting.BGAOpacity   );
        Config.Inst.Write( ConfigType.PanelOpacity, GameSetting.PanelOpacity );
        Config.Inst.Write( ConfigType.GearOffsetX,  GameSetting.GearOffsetX  );
        Config.Inst.Write( ConfigType.Measure,      GameSetting.HasFlag( VisualFlag.ShowMeasure ) );
        Config.Inst.Write( ConfigType.HitEffect,    GameSetting.HasFlag( VisualFlag.HitEffect   ) );
        Config.Inst.Write( ConfigType.LaneEffect,   GameSetting.HasFlag( VisualFlag.LaneEffect  ) );
    }
    public void DisableSystemOption()
    {
        DisableCanvas( SceneAction.Main, systemSetting, true, false );
        Config.Inst.Write( ConfigType.Resolution,   SystemSetting.CurrentResolution   );
        Config.Inst.Write( ConfigType.FrameLimit,   SystemSetting.CurrentFrameRate    );
        Config.Inst.Write( ConfigType.AntiAliasing, SystemSetting.CurrentAntiAliasing );
        Config.Inst.Write( ConfigType.ScreenMode,   SystemSetting.CurrentScreenMode   );
        Config.Inst.Write( ConfigType.PollingRate,  SystemSetting.CurrentPollingRate  );

        Config.Inst.Write( ConfigType.SoundBuffer, SystemSetting.CurrentSoundBuffer                  );
        Config.Inst.Write( ConfigType.Master,      AudioManager.Inst.GetVolume( ChannelType.Master ) );
        Config.Inst.Write( ConfigType.BGM,         AudioManager.Inst.GetVolume( ChannelType.BGM    ) );
        Config.Inst.Write( ConfigType.SFX,         AudioManager.Inst.GetVolume( ChannelType.SFX    ) );
    }
    public void DisableKeySetting()
    {
        DisableCanvas( SceneAction.Main, keySetting );
        Config.Inst.Write( ConfigType._4K, InputManager.Keys[GameKeyCount._4] );
        Config.Inst.Write( ConfigType._6K, InputManager.Keys[GameKeyCount._6] );
        Config.Inst.Write( ConfigType._7K, InputManager.Keys[GameKeyCount._7] );
    }
    public void DisableReload()
    {
        DisableCanvas( SceneAction.Main, reload.gameObject );
    }

    public void MoveToLobby()
    {
        //AudioManager.Inst.Play( SFX.MenuClick );
        //LoadScene( SceneType.Lobby );
    }
    public void ExitCancel() => DisableCanvas( SceneAction.Main, exit );
    public void Quit() => Application.Quit();
    #endregion

    public override void KeyBind()
    {
        // Main
        Bind( SceneAction.Main, KeyState.Down, KeyCode.F3, () => SpeedControlProcess( false ) );
        Bind( SceneAction.Main, KeyState.Hold, KeyCode.F3, () => PressedSpeedControl( false ) );
        Bind( SceneAction.Main, KeyState.Up,   KeyCode.F3, () => UpedSpeedControl() );

        Bind( SceneAction.Main, KeyState.Down, KeyCode.F4, () => SpeedControlProcess( true ) );
        Bind( SceneAction.Main, KeyState.Hold, KeyCode.F4, () => PressedSpeedControl( true ) );
        Bind( SceneAction.Main, KeyState.Up,   KeyCode.F4, () => UpedSpeedControl() );

        Bind( SceneAction.GameOption,   KeyCode.Escape, DisableGameSetting );
        Bind( SceneAction.SystemOption, KeyCode.Escape, DisableSystemOption );

        // KeySetting
        Bind( SceneAction.KeySetting, KeyCode.RightArrow, () => { MoveToNextOption( keySetting ); } );
        Bind( SceneAction.KeySetting, KeyCode.LeftArrow,  () => { MoveToPrevOption( keySetting ); } );
        Bind( SceneAction.KeySetting, KeyCode.Escape,     DisableKeySetting );

        if ( !DataStorage.IsMultiPlaying )
        {
            //ReLoad
            Bind( SceneAction.ReLoad, KeyCode.Escape, DisableReload );
        }

        // Exit
        Bind( SceneAction.Main, KeyCode.Escape, () => 
        {
            if ( !FreeStyleSearch.IsSearching )
                 EnableCanvas( SceneAction.Exit, exit ); 
        } );
        Bind( SceneAction.Exit, KeyCode.Escape, ExitCancel );
    }
}
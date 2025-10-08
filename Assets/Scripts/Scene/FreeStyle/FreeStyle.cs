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

    public TextMeshProUGUI speedText;
    public TextMeshProUGUI audioFPSText;

    protected override void Awake()
    {
        base.Awake();
        OnScrollChange += () => speedText.text = $"{GameSetting.ScrollSpeed}";

        StartCoroutine( UpdateAudioFPSTexts() );
    }

    protected override void Start()
    {
        base.Start();
        AudioManager.OnReload += Connect;
    }

    private IEnumerator UpdateAudioFPSTexts()
    {
        int curFPS = 0, prevFPS = 0;
        double deltaTime = 0d;
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .075f );

            deltaTime += ( AudioManager.DeltaTime - deltaTime );
            curFPS     = ( int ) ( 1d / deltaTime );

            if ( curFPS != prevFPS )
                 audioFPSText.text = $"{curFPS} FPS ({( deltaTime * 1000d ):F2} ms)";

            prevFPS = curFPS;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        AudioManager.OnReload -= Connect;
    }

    public override void Connect()
    {
        AudioManager.Inst.Pitch = GameSetting.CurrentPitch;
        AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.FFT,        ChannelType.BGM );
    }

    public override void Disconnect()
    {
        AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.FFT,        ChannelType.BGM );
    }

    #region Bind Unity Events
    public void EnableGameSettingCanvas()   => EnableCanvas( ActionType.GameOption, gameSetting );
    public void EnableSystemSettingCanvas() => EnableCanvas( ActionType.SystemOption, systemSetting, true, false );
    public void EnableKeySettingCanvas()    => EnableCanvas( ActionType.KeySetting, keySetting );
    public void EnableReloadCanvas()        => EnableCanvas( ActionType.ReLoad, reload.gameObject );
    public void EnableExitCanvas()          => EnableCanvas( ActionType.Exit, exit );
    public void MoveToLobby()
    {
        AudioManager.Inst.Play( SFX.MenuClick );
        LoadScene( SceneType.Lobby );
    }
    public void ExitCancel() => DisableCanvas( ActionType.Main, exit );
    public void Quit() => Application.Quit();
    #endregion

    public override void KeyBind()
    {
        // Main
        Bind( ActionType.Main, KeyState.Down, KeyCode.F3, () => SpeedControlProcess( false ) );
        Bind( ActionType.Main, KeyState.Hold, KeyCode.F3, () => PressedSpeedControl( false ) );
        Bind( ActionType.Main, KeyState.Up,   KeyCode.F3, () => UpedSpeedControl() );

        Bind( ActionType.Main, KeyState.Down, KeyCode.F4, () => SpeedControlProcess( true ) );
        Bind( ActionType.Main, KeyState.Hold, KeyCode.F4, () => PressedSpeedControl( true ) );
        Bind( ActionType.Main, KeyState.Up,   KeyCode.F4, () => UpedSpeedControl() );

        // GameSetting
        Bind( ActionType.GameOption, KeyCode.Escape, () => 
        {
            DisableCanvas( ActionType.Main, gameSetting );
            Config.Inst.Write( ConfigType.SoundOffset,  GameSetting.SoundOffset  );
            Config.Inst.Write( ConfigType.JudgeOffset,  GameSetting.JudgeOffset  );
            Config.Inst.Write( ConfigType.BGAOpacity,   GameSetting.BGAOpacity   );
            Config.Inst.Write( ConfigType.PanelOpacity, GameSetting.PanelOpacity );
            Config.Inst.Write( ConfigType.GearOffsetX,  GameSetting.GearOffsetX  );
            Config.Inst.Write( ConfigType.AutoPlay,     GameSetting.HasFlag( GameMode.AutoPlay      ) );
            Config.Inst.Write( ConfigType.NoFailed,     GameSetting.HasFlag( GameMode.NoFail        ) );
            Config.Inst.Write( ConfigType.Measure,      GameSetting.HasFlag( VisualFlag.ShowMeasure ) );
            Config.Inst.Write( ConfigType.HitEffect,    GameSetting.HasFlag( VisualFlag.HitEffect   ) );
            Config.Inst.Write( ConfigType.LaneEffect,   GameSetting.HasFlag( VisualFlag.LaneEffect  ) );
        } );

        // SystemSetting
        Bind( ActionType.SystemOption, KeyCode.Escape, () => 
        {
            DisableCanvas( ActionType.Main, systemSetting, true, false );
            Config.Inst.Write( ConfigType.Resolution,   SystemSetting.CurrentResolution   );
            Config.Inst.Write( ConfigType.FrameLimit,   SystemSetting.CurrentFrameRate    );
            Config.Inst.Write( ConfigType.AntiAliasing, SystemSetting.CurrentAntiAliasing );
            Config.Inst.Write( ConfigType.ScreenMode,   SystemSetting.CurrentScreenMode   );
            
            Config.Inst.Write( ConfigType.SoundBuffer, SystemSetting.CurrentSoundBuffer                  );
            Config.Inst.Write( ConfigType.Master,      AudioManager.Inst.GetVolume( ChannelType.Master ) );
            Config.Inst.Write( ConfigType.BGM,         AudioManager.Inst.GetVolume( ChannelType.BGM    ) );
            Config.Inst.Write( ConfigType.SFX,         AudioManager.Inst.GetVolume( ChannelType.SFX    ) );
        } );

        // KeySetting
        Bind( ActionType.KeySetting, KeyCode.RightArrow, () => { MoveToNextOption( keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.LeftArrow,  () => { MoveToPrevOption( keySetting ); } );
        Bind( ActionType.KeySetting, KeyCode.Escape,     () => 
        {
            DisableCanvas( ActionType.Main, keySetting );
            Config.Inst.Write( ConfigType._4K, InputManager.Keys[GameKeyCount._4] );
            Config.Inst.Write( ConfigType._6K, InputManager.Keys[GameKeyCount._6] );
            Config.Inst.Write( ConfigType._7K, InputManager.Keys[GameKeyCount._7] );
        } );

        if ( !DataStorage.IsMultiPlaying )
        {
            //ReLoad
            Bind( ActionType.Main,   KeyCode.F5,     () => { EnableCanvas( ActionType.ReLoad, reload.gameObject ); } );
            Bind( ActionType.ReLoad, KeyCode.Escape, () => { DisableCanvas( ActionType.Main,  reload.gameObject ); } );
        }

        // Exit
        Bind( ActionType.Main, KeyCode.Escape, () => 
        {
            if ( !FreeStyleSearch.IsSearching )
                 EnableCanvas( ActionType.Exit, exit ); 
        } );
        Bind( ActionType.Exit, KeyCode.Escape, ExitCancel );
    }
}
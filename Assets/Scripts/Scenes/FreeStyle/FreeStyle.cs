using UnityEngine;
using TMPro;
using DG.Tweening;

public class FreeStyle : Scene
{
    public OptionController gameOption, systemOption;
    public TextMeshProUGUI speedText;

    protected override void Awake()
    {
        base.Awake();

        var judge = GameObject.FindGameObjectWithTag( "Judgement" );
        if ( judge ) Destroy( judge );

        OnScrollChange += () => speedText.text = $"{GameSetting.ScrollSpeed:F1}";
    }

    public override void Connect()
    {
        SoundManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.BGM );
        SoundManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        SoundManager.Inst.AddDSP( FMOD.DSP_TYPE.FFT, ChannelType.BGM );
    }

    public override void Disconnect()
    {
        SoundManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        SoundManager.Inst.RemoveDSP( FMOD.DSP_TYPE.FFT, ChannelType.BGM );
    }


    private void ScrollDown( OptionController _controller )
    {
        _controller.PrevMove();
        SoundManager.Inst.Play( SoundSfxType.MenuSelect );
    }

    private void ScrollUp( OptionController _controller )
    {
        _controller.NextMove();
        SoundManager.Inst.Play( SoundSfxType.MenuSelect );
    }

    private void EnableOption( ActionType _changeType, OptionController _controller )
    {
        GameObject root = _controller.transform.root.gameObject;
        root.SetActive( true );
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 0f;
            DOTween.To( () => 0f, x => group.alpha = x, 1f, Global.Const.OptionFadeDuration );
        }

        ChangeAction( _changeType );
        SoundManager.Inst.Play( SoundSfxType.MenuClick );
        SoundManager.Inst.FadeVolume( SoundManager.Inst.GetVolume( ChannelType.BGM ), SoundManager.Inst.Volume * .5f, .5f );
    }

    private void DisableOption( ActionType _changeType, OptionController _controller )
    {
        DOTween.Clear();
        GameObject root = _controller.transform.root.gameObject;
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 1f;
            DOTween.To( () => 1f, x => group.alpha = x, 0f, Global.Const.OptionFadeDuration ).OnComplete( () => root.SetActive( false ) );
        }
        else
        {
            root.SetActive( false );
        }

        ChangeAction( _changeType );
        SoundManager.Inst.Play( SoundSfxType.MenuHover );
        SoundManager.Inst.FadeVolume( SoundManager.Inst.GetVolume( ChannelType.BGM ), SoundManager.Inst.Volume, .5f );
    }

    public override void KeyBind()
    {
        // Main
        //Bind( ActionType.Main, KeyCode.Escape, () => LoadScene( SceneType.Lobby ) );
        //Bind( ActionType.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MainHover ) );

        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Main, InputType.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );

        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Main, InputType.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );

        // GameOption
        Bind( ActionType.Main,       KeyCode.Space,     () => { EnableOption(  ActionType.GameOption, gameOption ); } );
        Bind( ActionType.GameOption, KeyCode.Escape,    () => { DisableOption( ActionType.Main,       gameOption ); } );
        Bind( ActionType.GameOption, KeyCode.Space,     () => { DisableOption( ActionType.Main,       gameOption ); } );
        Bind( ActionType.GameOption, KeyCode.DownArrow, () => { ScrollUp(   gameOption ); } );
        Bind( ActionType.GameOption, KeyCode.UpArrow,   () => { ScrollDown( gameOption ); } );

        // SystemOption
        Bind( ActionType.Main,         KeyCode.F10,       () => { EnableOption(  ActionType.SystemOption, systemOption ); } );
        Bind( ActionType.SystemOption, KeyCode.Escape,    () => { DisableOption( ActionType.Main,         systemOption ); } );
        Bind( ActionType.SystemOption, KeyCode.Space,     () => { DisableOption( ActionType.Main,         systemOption ); } );
        Bind( ActionType.SystemOption, KeyCode.DownArrow, () => { ScrollUp(   systemOption ); } );
        Bind( ActionType.SystemOption, KeyCode.UpArrow,   () => { ScrollDown( systemOption ); } );
    }
}
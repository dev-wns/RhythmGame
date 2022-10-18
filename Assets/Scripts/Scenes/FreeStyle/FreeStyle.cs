using UnityEngine;
using TMPro;
using DG.Tweening;

public class FreeStyle : Scene
{
    public GameObject optionCanvas;
    private CanvasGroup optionGroup;
    public TextMeshProUGUI speedText;

    protected override void Awake()
    {
        base.Awake();

        optionGroup = optionCanvas.GetComponent<CanvasGroup>();
        var judge = GameObject.FindGameObjectWithTag( "Judgement" );
        if ( judge ) Destroy( judge );

        OnScrollChanged += () => speedText.text = $"{GameSetting.ScrollSpeed:F1}";

        FMOD.DSP pitchShift;
        SoundManager.Inst.GetDSP( FMOD.DSP_TYPE.PITCHSHIFT, out pitchShift );
        SoundManager.Inst.AddDSP( in pitchShift, ChannelType.BGM );
        SoundManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.BGM );
    }

    private void ShowOption()
    {
        optionGroup.alpha = 0f;
        optionCanvas.SetActive( true );
        DOTween.To( () => 0f, x => optionGroup.alpha = x, 1f, Global.Const.OptionFadeDuration );
        ChangeAction( ActionType.Option );
        SoundManager.Inst.Play( SoundSfxType.MenuClick );
        SoundManager.Inst.FadeOut( SoundManager.Inst.GetVolume( ChannelType.BGM ) * .5f, .5f );
    }

    public override void KeyBind()
    {
        Bind( ActionType.Main, KeyCode.Space, ShowOption );

        Bind( ActionType.Main, KeyCode.Escape, () => LoadScene( SceneType.Lobby ) );
        Bind( ActionType.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MainHover ) );

        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Main, InputType.Up, KeyCode.Alpha1,   () => UpedSpeedControl() );

        Bind( ActionType.Main, InputType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Main, InputType.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Main, InputType.Up, KeyCode.Alpha2,   () => UpedSpeedControl() );
    }
}
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
        ChangeAction( SceneAction.Option );
        SoundManager.Inst.Play( SoundSfxType.MenuClick );
        SoundManager.Inst.FadeOut( SoundManager.Inst.GetVolume( ChannelType.BGM ) * .5f, .5f );
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Space, ShowOption );

        Bind( SceneAction.Main, KeyCode.Escape, () => LoadScene( SceneType.Lobby ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MainHover ) );

        Bind( SceneAction.Main, KeyType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( SceneAction.Main, KeyType.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( SceneAction.Main, KeyType.Up, KeyCode.Alpha1,   () => UpedSpeedControl() );

        Bind( SceneAction.Main, KeyType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( SceneAction.Main, KeyType.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( SceneAction.Main, KeyType.Up, KeyCode.Alpha2,   () => UpedSpeedControl() );
    }
}
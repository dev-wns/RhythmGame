using UnityEngine;
using TMPro;

public class FreeStyle : Scene
{
    public GameObject optionCanvas;
    public TextMeshProUGUI speedText;

    protected override void Awake()
    {
        base.Awake();

        var judge = GameObject.FindGameObjectWithTag( "Judgement" );
        if ( judge ) Destroy( judge );

        OnScrollChanged += () => speedText.text = $"{GameSetting.ScrollSpeed:F1}";
    }

    public override void KeyBind()
    {
        Bind( SceneAction.Main, KeyCode.Space, () => optionCanvas.SetActive( true ) );
        Bind( SceneAction.Main, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( true ) );
        Bind( SceneAction.Main, KeyCode.Space, () => ChangeAction( SceneAction.Option ) );
        Bind( SceneAction.Main, KeyCode.Space, () => SoundManager.Inst.Play( SoundSfxType.MenuClick ) );
        Bind( SceneAction.Main, KeyCode.Space, () => SoundManager.Inst.FadeOut( SoundManager.Inst.GetVolume( ChannelType.BGM ) * .35f, .5f ) );

        Bind( SceneAction.Main, KeyCode.Escape, () => LoadScene( SceneType.Lobby ) );
        Bind( SceneAction.Main, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MainHover ) );

        Bind( SceneAction.Main, KeyType.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( SceneAction.Main, KeyType.Hold, KeyCode.Alpha1, () => PressdSpeedControl( false ) );
        Bind( SceneAction.Main, KeyType.Up, KeyCode.Alpha1,   () => UpedSpeedControl() );

        Bind( SceneAction.Main, KeyType.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( SceneAction.Main, KeyType.Hold, KeyCode.Alpha2, () => PressdSpeedControl( true ) );
        Bind( SceneAction.Main, KeyType.Up, KeyCode.Alpha2,   () => UpedSpeedControl() );
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePause : ScrollOption, IKeyBind
{
    public RectTransform selectUI;

    private InGame scene;

    protected override void Awake()
    {
        base.Awake();
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        KeyBind();
        IsLoop = true;
    }

    private void MoveSelectPosition()
    {
        var option = CurrentOption.transform as RectTransform;
        selectUI.SetParent( option );
        selectUI.anchoredPosition = Vector2.zero;
    }

    private void OnEnable()
    {
        Select( 0 );
        MoveSelectPosition();
    }
    public override void PrevMove()
    {
        base.PrevMove();

        MoveSelectPosition();
        SoundManager.Inst.PlaySfx( SoundSfxType.Move );
    }

    public override void NextMove()
    {
        base.NextMove();

        MoveSelectPosition();
        SoundManager.Inst.PlaySfx( SoundSfxType.Move );
    }

    public void Continue()
    {
        gameObject.SetActive( false );
        scene.ChangeAction( SceneAction.Main );
        NowPlaying.Inst.Pause( false );
    }

    public void KeyBind()
    {
        scene.Bind( SceneAction.Option, KeyCode.UpArrow, () => PrevMove() );
        scene.Bind( SceneAction.Option, KeyCode.DownArrow, () => NextMove() );

        scene.Bind( SceneAction.Option, KeyCode.Escape, Continue );
        scene.Bind( SceneAction.Option, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SoundSfxType.Escape ) );

        scene.Bind( SceneAction.Option, KeyCode.Return, () => CurrentOption.Process() );
        scene.Bind( SceneAction.Option, KeyCode.Return, () => SoundManager.Inst.PlaySfx( SoundSfxType.Return ) );
    }
}

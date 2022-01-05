using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleOption : VerticalScroll, IKeyBind
{
    public RectTransform outline;
    private GameObject optionCanvas;
    private Scene currentScene;

    protected override void Awake()
    {
        base.Awake();

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        optionCanvas = scrollRect.gameObject;
        currentScene = scene.GetComponent<Scene>();
        KeyBind();
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        OptionProcess();
    }

    private void OptionProcess()
    {
        IOptionA previous = prevOption.GetComponent<IOptionA>();
        if ( previous != null )
        {
            var keyControl = previous as IKeyControl;
            keyControl.KeyRemove();

            var outline = previous as OptionBase;
            outline.ActiveOutline( false );
        }

        IOptionA current = curOption.GetComponent<IOptionA>();
        if ( current != null )
        {
            var keyControl = current as IKeyControl;
            keyControl.KeyBind();

            var outline = current as OptionBase;
            outline.ActiveOutline( true );
        }
    }

    public void KeyBind()
    {
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, () => PrevMove() );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => optionCanvas.SetActive( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => SoundManager.Inst.UseLowEqualizer( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.FreeStyle ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => optionCanvas.SetActive( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => currentScene.ChangeAction( SceneAction.FreeStyle ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
    }
}

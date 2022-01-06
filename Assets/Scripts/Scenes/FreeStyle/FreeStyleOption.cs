using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleOption : HideScroll, IKeyBind
{
    private GameObject optionCanvas;
    private Scene currentScene;

    protected override void Awake()
    {
        base.Awake();

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        //optionCanvas = scrollRect.gameObject;
        currentScene = scene.GetComponent<Scene>();
        KeyBind();
    }

    protected override  void Start()
    {
        base.Start();

        OptionProcess();
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
        if ( prevOption != null )
        {
            var keyControl = prevOption as IKeyControl;
            keyControl.KeyRemove();

            var outline = prevOption as OptionBase;
            outline.ActiveOutline( false );
        }

        if ( curOption != null )
        {
            var keyControl = curOption as IKeyControl;
            keyControl.KeyBind();

            var outline = curOption as OptionBase;
            outline.ActiveOutline( true );
        }
    }

    public void KeyBind()
    {
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, () => PrevMove() );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => SoundManager.Inst.UseLowEqualizer( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.FreeStyle ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => gameObject.SetActive( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => SoundManager.Inst.UseLowEqualizer( false ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => currentScene.ChangeAction( SceneAction.FreeStyle ) );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.Space, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
    }
}

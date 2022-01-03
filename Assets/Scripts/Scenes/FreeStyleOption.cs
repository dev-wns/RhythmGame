using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyleOption : VerticalScroll, IKeyBind
{
    public RectTransform outline;
    private GameObject optionCanvas;
    private StaticSceneKeyAction keyAction = new StaticSceneKeyAction();
    private Scene currentScene;

    protected override void Awake()
    {
        base.Awake();

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        optionCanvas = scrollRect.gameObject;
        currentScene = scene.GetComponent<Scene>();
        KeyBind();
    }

    private void Start()
    {
        currentScene.KeyBind( SceneAction.FreeStyleOption, keyAction );
    }

    public override void PrevMove()
    {
        base.PrevMove();

        if ( !IsLoop && IsDuplicate ) return;

        SetOutline();
    }

    public override void NextMove()
    {
        base.NextMove();

        if ( !IsLoop && IsDuplicate ) return;

        SetOutline();
    }
    private void SetOutline()
    {
        outline.transform.SetParent( curOption.transform );
        outline.anchoredPosition = Vector2.zero;
    }

    public void KeyBind()
    {
        keyAction.Bind( KeyCode.UpArrow, KeyType.Down, () => PrevMove() );
        keyAction.Bind( KeyCode.UpArrow, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        keyAction.Bind( KeyCode.DownArrow, KeyType.Down, () => NextMove() );
        keyAction.Bind( KeyCode.DownArrow, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => optionCanvas.SetActive( false ) );
        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => SoundManager.Inst.UseLowEqualizer( false ) );
        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => currentScene.ChangeKeyAction( SceneAction.FreeStyle ) );
        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );

        keyAction.Bind( KeyCode.Space, KeyType.Down, () => optionCanvas.SetActive( false ) );
        keyAction.Bind( KeyCode.Space, KeyType.Down, () => SoundManager.Inst.UseLowEqualizer( false ) );
        keyAction.Bind( KeyCode.Space, KeyType.Down, () => currentScene.ChangeKeyAction( SceneAction.FreeStyle ) );
        keyAction.Bind( KeyCode.Space, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyExit : ScrollBase, IKeyBind
{
    public RectTransform outline;
    public GameObject exitCanvas;

    private StaticSceneKeyAction keyAction = new StaticSceneKeyAction();
    private Scene currentScene;

    protected override void Awake()
    {
        base.Awake();

        IsLoop = true;

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        currentScene = scene.GetComponent<Scene>();
        KeyBind();
    }

    private void Start()
    {
        currentScene.KeyBind( SceneAction.Exit, keyAction );
    }

    private void SetOutline()
    {
        outline.transform.SetParent( curOption.transform );
        outline.anchoredPosition = Vector2.zero;
    }

    private void ButtonProcess()
    {
        if ( curOption == null ) return;

        IOption option = curOption.GetComponent<IOption>();
        if ( option.type != OptionType.Button ) return;

        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN );

        var button = option as IOptionButton;
        button.Process();
    }

    public override void PrevMove()
    {
        base.PrevMove();
        SetOutline();
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE );

    }

    public override void NextMove()
    {
        base.NextMove();
        SetOutline();
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE );

    }

    public void KeyBind()
    {
        keyAction.Bind( KeyCode.LeftArrow, KeyType.Down, () => PrevMove() );
        keyAction.Bind( KeyCode.RightArrow, KeyType.Down, () => NextMove() );
        
        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => Cancel() );
        
        keyAction.Bind( KeyCode.Return, KeyType.Down, () => ButtonProcess() );
    }

    public void Cancel()
    {
        currentScene.ChangeKeyAction( SceneAction.Lobby );
        exitCanvas.SetActive( false );
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN );
    }

    public void Exit()
    {
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN );
        Application.Quit();
    }
}

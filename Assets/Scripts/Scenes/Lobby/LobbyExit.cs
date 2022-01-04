using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyExit : ScrollOption, IKeyBind
{
    public RectTransform outline;
    public GameObject exitCanvas;

    private Scene currentScene;

    protected override void Awake()
    {
        base.Awake();

        IsLoop = true;

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        currentScene = scene.GetComponent<Scene>();
        KeyBind();
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
        currentScene.Bind( SceneAction.Exit, KeyCode.LeftArrow, () => PrevMove() );
        currentScene.Bind( SceneAction.Exit, KeyCode.RightArrow, () => NextMove() );
        currentScene.Bind( SceneAction.Exit, KeyCode.Escape, () => Cancel() );
        currentScene.Bind( SceneAction.Exit, KeyCode.Return, () => ButtonProcess() );
    }

    public void Cancel()
    {
        currentScene.ChangeAction( SceneAction.Lobby );
        exitCanvas.SetActive( false );
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN );
    }

    public void Exit()
    {
        SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN );
        Application.Quit();
    }
}

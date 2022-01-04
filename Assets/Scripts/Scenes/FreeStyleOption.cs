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

        IOptionA prev = prevOption.GetComponent<IOptionA>(); 
        if ( prev == null ) return;
        if ( prev.type == OptionType.Text ||
             prev.type == OptionType.Slider )
        {
            var arrowBind = prev as OptionArrowBindBase;
            arrowBind.KeyRemove();
        }

        IOptionA current = curOption.GetComponent<IOptionA>();
        if ( current == null ) return;
        if ( current.type == OptionType.Text ||
             current.type == OptionType.Slider )
        {
            var arrowBind = current as OptionArrowBindBase;
            arrowBind.KeyBind();
        }
        SetOutline();
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        IOptionA prev = prevOption.GetComponent<IOptionA>();
        if ( prev == null ) return;
        if ( prev.type == OptionType.Text ||
             prev.type == OptionType.Slider )
        {
            var arrowBind = prev as OptionArrowBindBase;
            arrowBind.KeyRemove();
        }

        IOptionA current = curOption.GetComponent<IOptionA>();
        if ( current == null ) return;
        if ( current.type == OptionType.Text ||
             current.type == OptionType.Slider )
        {
            var arrowBind = current as OptionArrowBindBase;
            arrowBind.KeyBind();
        }

        SetOutline();
    }

    public void OptionProcess()
    {
        if ( curOption == null ) return;

        IOptionA option = curOption.GetComponent<IOptionA>();
        option?.Process();
    }

    private void SetOutline()
    {
        outline.transform.SetParent( curOption.transform );
        outline.anchoredPosition = Vector2.zero;
    }

    public void KeyBind()
    {
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, () => PrevMove() );
        //currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, () => OptionProcess() );
        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.DownArrow, () => NextMove() );
        //currentScene.Bind( SceneAction.FreeStyleOption, KeyCode.UpArrow, ()   => OptionProcess() );
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

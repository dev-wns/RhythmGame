using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyOption : ScrollBase, IKeyBind
{
    public RectTransform outline;
    public GameObject optionCanvas, subOptionCanvas;
    private Scene currentScene;

    protected override void Awake()
    {
        base.Awake();

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

    private void SliderProcess( int _value )
    {
        if ( curOption == null ) return;

        IOption option = curOption.GetComponent<IOption>();
        if ( option.type != OptionType.Slider ) return;

        if ( _value > 0 ) SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.INCREASE );
        else              SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.DECREASE );

        var slider = option as IOptionSlider;
        slider.Process( _value );
    }

    public override void PrevMove()
    {
        base.PrevMove();

        if ( IsDuplicate ) return;

        SetOutline();
    }

    public override void NextMove()
    {
        base.NextMove();

        if ( IsDuplicate ) return;

        SetOutline();
    }

    public void KeyBind()
    {
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.UpArrow,   () => PrevMove() );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.UpArrow,   () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );
        
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Escape, () => currentScene.ChangeAction( SceneAction.Lobby ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Escape, () => optionCanvas.SetActive( false ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
        
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Space, () => currentScene.ChangeAction( SceneAction.Lobby ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Space, () => optionCanvas.SetActive( false ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Space, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
        
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.Return, () => ButtonProcess() );
        
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.RightArrow, () => SliderProcess( 10 ) );
        currentScene.Bind( SceneAction.LobbyOption, KeyCode.LeftArrow,  () => SliderProcess( -10 ) );
    }

    public void ActiveSubOption( GameObject _obj )
    {
        subOptionCanvas.SetActive( true );
        _obj.SetActive( true );

        currentScene.ChangeAction( SceneAction.LobbySubOption );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyOption : ScrollBase, IKeyBind
{
    public RectTransform outline;
    public GameObject optionCanvas, subOptionCanvas;
    private StaticSceneKeyAction keyAction = new StaticSceneKeyAction();
    private Scene currentScene;

    protected override void Awake()
    {
        base.Awake();

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        currentScene = scene.GetComponent<Scene>();
        KeyBind();
    }

    private void Start()
    {
        currentScene.KeyBind( SceneAction.LobbyOption, keyAction );
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
        keyAction.Bind( KeyCode.UpArrow,   KeyType.Down, () => PrevMove() );
        keyAction.Bind( KeyCode.UpArrow,   KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );
        keyAction.Bind( KeyCode.DownArrow, KeyType.Down, () => NextMove() );
        keyAction.Bind( KeyCode.DownArrow, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );

        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => currentScene.ChangeKeyAction( SceneAction.Lobby ) );
        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => optionCanvas.SetActive( false ) );
        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );

        keyAction.Bind( KeyCode.Space, KeyType.Down, () => currentScene.ChangeKeyAction( SceneAction.Lobby ) );
        keyAction.Bind( KeyCode.Space, KeyType.Down, () => optionCanvas.SetActive( false ) );
        keyAction.Bind( KeyCode.Space, KeyType.Down, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );

        keyAction.Bind( KeyCode.Return, KeyType.Down, () => ButtonProcess() );
        
        keyAction.Bind( KeyCode.RightArrow, KeyType.Down, () => SliderProcess( 10 ) );
        keyAction.Bind( KeyCode.LeftArrow,  KeyType.Down, () => SliderProcess( -10 ) );
    }

    public void ActiveSubOption( GameObject _obj )
    {
        subOptionCanvas.SetActive( true );
        _obj.SetActive( true );

        currentScene.ChangeKeyAction( SceneAction.LobbySubOption );
    }
}

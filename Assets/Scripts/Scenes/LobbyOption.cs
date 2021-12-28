using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyOption : ScrollBase, IKeyBind
{
    public RectTransform outline;

    public GameObject optionCanvas, subOptionCanvas;

    private StaticSceneKeyAction keyAction = new StaticSceneKeyAction();
    private Scene currentScene;
    private LobbySubOption subOption;

    protected override void Awake()
    {
        base.Awake();

        GameObject scene = GameObject.FindGameObjectWithTag( "Scene" );
        currentScene = scene.GetComponent<Scene>();
        subOption    = scene.GetComponent<LobbySubOption>();
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

        subOptionCanvas.SetActive( true );
        currentScene.ChangeKeyAction( SceneAction.LobbySubOption );

        var subOptionContent = subOptionCanvas.transform.Find( curOption.name );
        subOption.Initialize( subOptionContent );
    }

    private void SliderProcess( int _value )
    {
        if ( curOption == null ) return;

        IOption option = curOption.GetComponent<IOption>();
        if ( option.type != OptionType.Slider ) return;

        var button = option as LobbyOptionSlider;
        button.Process( _value );
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
        keyAction.Bind( KeyCode.DownArrow, KeyType.Down, () => NextMove() );

        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => currentScene.ChangeKeyAction( SceneAction.Lobby ) );
        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => optionCanvas.SetActive( false ) );

        keyAction.Bind( KeyCode.Space,  KeyType.Down, () => currentScene.ChangeKeyAction( SceneAction.Lobby ) );
        keyAction.Bind( KeyCode.Space,  KeyType.Down, () => optionCanvas.SetActive( false ) );

        keyAction.Bind( KeyCode.Return,     KeyType.Down, () => ButtonProcess() );
        keyAction.Bind( KeyCode.RightArrow, KeyType.Down, () => SliderProcess( 10 ) );
        keyAction.Bind( KeyCode.LeftArrow,  KeyType.Down, () => SliderProcess( -10 ) );


        currentScene.KeyBind( SceneAction.LobbyOption, keyAction );
    }
}

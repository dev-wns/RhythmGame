using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbySubOption : ScrollOption, IKeyBind
{
    public SceneAction prevKeyAction;

    public GameObject subOptionCanvas;
    public GameObject outline;
    public TextMeshProUGUI currentValueText;
    public GameObject driver;

    protected Scene currentScene;
    public GameObject curSubOption;

    public GameObject buttonPrefab;

    protected override void Awake()
    {
        base.Awake();

        currentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();

        var drivers = SoundManager.Inst.soundDrivers;
        for ( int i = 0; i < drivers.Count; i++ )
        {
            GameObject obj = Instantiate( buttonPrefab, driver.transform );
            obj.name = drivers[i].name;

            var buttonText = obj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = drivers[i].name;
        }

        KeyBind();
    }

    public override void PrevMove()
    {
        base.PrevMove();
        OutlineUpdate();
    }

    public override void NextMove()
    {
        base.NextMove();
        OutlineUpdate();
    }

    private void OutlineUpdate()
    {
        outline.transform.SetParent( curOption.transform );
        RectTransform rt = outline.transform as RectTransform;
        rt.anchoredPosition = Vector2.zero;
        rt.pivot = new Vector2( .5f, .5f );
    }

    public void ContentsUpdate( GameObject _obj )
    {
        curSubOption = _obj;

        contents.Clear();
        int length = _obj.transform.childCount;
        contents.Capacity = length;

        for ( int i = 0; i < length; i++ )
        {
            if ( i >= contents.Count ) contents.Add( _obj.transform.GetChild( i ).gameObject );
            else contents[i] = _obj.transform.GetChild( i ).gameObject;
        }

        SelectPosition( 0 );
        ContentValueTextUpdate();
        OutlineUpdate();
    }

    public void ContentProcess()
    {
        IOption option = curSubOption.GetComponent<IOption>();
        if ( option.type != OptionType.Button ) return;

        var content = option as CustomButton;
        content.key = curIndex;
        content.Process();
        ContentValueTextUpdate();
    }

    private void ContentValueTextUpdate()
    {
        if ( curSubOption.name == "Key Setting" )
        {
            currentValueText.gameObject.SetActive( false );
            currentValueText.text = string.Empty;
        }
        else
        {
            currentValueText.gameObject.SetActive( true );
            currentValueText.text = curOption?.name ?? string.Empty;
        }
    }

    public void KeyBind()
    {
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.UpArrow,   () => PrevMove() );
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.UpArrow,   () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.DownArrow, () => NextMove() );
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.DownArrow, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.MOVE ) );
                           
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.Escape, () => currentScene.ChangeAction( prevKeyAction ) );
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.Escape, () => curSubOption.SetActive( false ) );
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.Escape, () => subOptionCanvas.SetActive( false ) );
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.Escape, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.ESCAPE ) );
                           
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.Return, () => ContentProcess() );
        currentScene.Bind( SceneAction.LobbySubOption, KeyCode.Return, () => SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN ) );
    }
}

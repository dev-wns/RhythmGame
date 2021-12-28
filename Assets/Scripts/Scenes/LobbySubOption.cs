using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbySubOption : ScrollBase, IKeyBind
{
    public OptionType type { get; } = OptionType.Button;

    public SceneAction prevKeyAction;
    protected StaticSceneKeyAction keyAction = new StaticSceneKeyAction();

    public GameObject subOptionCanvas;
    public GameObject outline;
    public TextMeshProUGUI currentValueText;

    protected Scene currentScene;
    public GameObject curSubOption;

    public GameObject buttonPrefab;

    protected override void Awake()
    {
        base.Awake();

        currentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();

        GameObject driver = GetContent( "Driver" );
        var drivers = SoundManager.Inst.soundDrivers;
        for( int i = 0; i < drivers.Count; i++ )
        {
            GameObject obj = Instantiate( buttonPrefab, driver.transform );
            obj.name = drivers[i].name;

            var buttonText  = obj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = drivers[i].name;
        }

        KeyBind();
    }

    protected virtual void Start()
    {
        currentScene.KeyBind( SceneAction.LobbySubOption, keyAction );
    }

    public override void PrevMove()
    {
        base.PrevMove();
        OutlineSetting();
    }

    public override void NextMove()
    {
        base.NextMove();
        OutlineSetting();
    }

    private void OutlineSetting()
    {
        outline.transform.SetParent( curOption.transform );
        RectTransform rt = outline.transform as RectTransform;
        rt.anchoredPosition = Vector2.zero;
        rt.pivot = new Vector2( .5f, .5f );
    }

    public void Initialize( Transform _contentParent )
    {
        curSubOption = _contentParent.gameObject;
        curSubOption.SetActive( true );

        int length = _contentParent.childCount;
        contents.Capacity = length;
        for ( int i = 0; i < length; i++ )
        {
            contents.Add( _contentParent.GetChild( i ).gameObject );
        }

        SelectPosition( 0 );

        if ( curSubOption.name == "Key Setting" )
            currentValueText.text = "";
        else
            currentValueText.text = curOption?.name ?? "";

        OutlineSetting();
    }

    public void ContentProcess()
    {
        IOption option = curSubOption.GetComponent<IOption>();
        if ( option.type != OptionType.Button ) return;

        var button = option as LobbyOptionButton;
        button.key = curIndex;
        button.Process();

        if ( curSubOption.name == "Key Setting" )
             currentValueText.text = "";
        else 
             currentValueText.text = curOption?.name ?? "";
    }

    public void KeyBind()
    {
        keyAction.Bind( KeyCode.UpArrow, KeyType.Down, () => PrevMove() );
        keyAction.Bind( KeyCode.DownArrow, KeyType.Down, () => NextMove() );

        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => currentScene.ChangeKeyAction( prevKeyAction ) );
        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => curSubOption.SetActive( false ) );
        keyAction.Bind( KeyCode.Escape, KeyType.Down, () => subOptionCanvas.SetActive( false ) );

        keyAction.Bind( KeyCode.Return, KeyType.Down, () => ContentProcess() );
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;


public class KeySetting : CustomButton
{
    private Scene currentScene;

    public GameObject keySettingPanel;
    public TextMeshProUGUI panelText;

    private KeyCode prevKeyCode, keyCode;

    private List<TextMeshProUGUI> trackTexts = new List<TextMeshProUGUI>();

    private void Awake()
    {
        currentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();

        trackTexts.Capacity = 6;
        for ( int i = 0; i < 6; i++ )
        {
            Transform obj = transform.GetChild( i );
            trackTexts.Add( obj.transform.GetChild( 0 ).GetComponent<TextMeshProUGUI>() );

            trackTexts[i].text = GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )i].ToString();
        }
    }

    public override void Process()
    {
        base.Process();
        keySettingPanel.SetActive( true );
        panelText.text = GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )key].ToString();

        currentScene.InputLock( true );
        StartCoroutine( ChangeGameKey() );
    }

    public IEnumerator ChangeGameKey()
    {
        keyCode = GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )key];
        while ( true )
        {
            yield return null;

            if ( prevKeyCode != keyCode )
            {
                panelText.text = keyCode.ToString();
            }

            if ( Input.GetKeyDown( KeyCode.Return ) )
            {
                for( int i = 0; i < 6; i++ )
                {
                    if ( keyCode == GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )i] )
                    {
                        GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )i] = KeyCode.None;
                        trackTexts[i].text = KeyCode.None.ToString();
                    }
                }

                GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )key] = keyCode;
                trackTexts[key].text = keyCode.ToString();
                Debug.Log( $"Key : {keyCode}" );
                break;
            }
        }

        currentScene.InputLock( false );
        keySettingPanel.SetActive( false );
    }

    private void OnGUI()
    {
        Event e = Event.current;
        if ( e.isKey && e.keyCode != KeyCode.None && e.keyCode != KeyCode.Return )
        {
            prevKeyCode = keyCode;
            keyCode = e.keyCode;
        }
    }
}

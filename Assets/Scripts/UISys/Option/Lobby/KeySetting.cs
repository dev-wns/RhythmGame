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

    private KeyCode currentKeyCode;

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
        currentKeyCode = GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )key];
        while ( true )
        {
            yield return null;

            if ( Input.anyKeyDown && !Input.GetKeyDown( KeyCode.Return ) && !Input.GetKeyDown( KeyCode.Escape ) )
                 SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.INCREASE );

            if ( Input.GetKeyDown( KeyCode.Return ) )
            {
                for( int i = 0; i < 6; i++ )
                {
                    if ( currentKeyCode == GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )i] )
                    {
                        GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )i] = KeyCode.None;
                        trackTexts[i].text = KeyCode.None.ToString();
                    }
                }

                GlobalKeySetting.Inst.Keys[( GAME_KEY_ACTION )key] = currentKeyCode;
                trackTexts[key].text = currentKeyCode.ToString();
                SoundManager.Inst.PlaySfx( SOUND_SFX_TYPE.RETURN );
                Debug.Log( $"Key : {currentKeyCode}" );
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
            if ( e.keyCode != currentKeyCode )
            {
                panelText.text = e.keyCode.ToString();
                currentKeyCode = e.keyCode;
            }
        }
    }
}

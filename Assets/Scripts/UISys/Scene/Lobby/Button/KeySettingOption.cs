using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeySettingOption : OptionButton
{
    public GameObject keySettingPanel;
    public TextMeshProUGUI panelText;
    private KeyCode curKeyCode;

    public int lane;
    public List<TextMeshProUGUI> trackText;

    private void OnEnable()
    {
        trackText[lane].text = GameSetting.Inst.Keys[( GameKeyAction )lane].ToString();
    }

    public override void Process()
    {
        keySettingPanel.SetActive( true );
        panelText.text = GameSetting.Inst.Keys[( GameKeyAction )lane].ToString();

        CurrentScene?.InputLock( true );
        StartCoroutine( ChangeGameKey() );
    }

    public IEnumerator ChangeGameKey()
    {
        curKeyCode = GameSetting.Inst.Keys[( GameKeyAction )lane];
        while ( true )
        {
            yield return null;

            if ( Input.anyKeyDown && !Input.GetKeyDown( KeyCode.Return ) && !Input.GetKeyDown( KeyCode.Escape ) )
                SoundManager.Inst.Play( SoundSfxType.Increase );

            if ( Input.GetKeyDown( KeyCode.Return ) )
            {
                for ( int i = 0; i < 6; i++ )
                {
                    if ( curKeyCode == GameSetting.Inst.Keys[( GameKeyAction )i] )
                    {
                        GameSetting.Inst.Keys[( GameKeyAction )i] = KeyCode.None;
                        trackText[i].text = KeyCode.None.ToString();
                    }
                }

                GameSetting.Inst.Keys[( GameKeyAction )lane] = curKeyCode;
                trackText[lane].text = curKeyCode.ToString();
                SoundManager.Inst.Play( SoundSfxType.Return );
                Debug.Log( $"Key : {curKeyCode}" );
                break;
            }
        }

        CurrentScene?.InputLock( false );
        keySettingPanel.SetActive( false );
    }

    private void OnGUI()
    {
        Event e = Event.current;
        if ( e.isKey && e.keyCode != KeyCode.None && e.keyCode != KeyCode.Return )
        {
            if ( e.keyCode != curKeyCode )
            {
                panelText.text = e.keyCode.ToString();
                curKeyCode = e.keyCode;
            }
        }
    }
}

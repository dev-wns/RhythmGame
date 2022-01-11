using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeySettingOption : OptionButton
{
    public GameObject keySettingPanel;
    public TextMeshProUGUI panelText;
    private KeyCode currentKeyCode;

    public int lane;
    public List<TextMeshProUGUI> trackText;

    protected override void Awake()
    {
        base.Awake();
        trackText[lane].text = GlobalKeySetting.Inst.Keys[( GameKeyAction )lane].ToString();
    }

    public override void Process()
    {
        keySettingPanel.SetActive( true );
        panelText.text = GlobalKeySetting.Inst.Keys[( GameKeyAction )lane].ToString();

        currentScene?.InputLock( true );
        StartCoroutine( ChangeGameKey() );
    }

    public IEnumerator ChangeGameKey()
    {
        currentKeyCode = GlobalKeySetting.Inst.Keys[( GameKeyAction )lane];
        while ( true )
        {
            yield return null;

            if ( Input.anyKeyDown && !Input.GetKeyDown( KeyCode.Return ) && !Input.GetKeyDown( KeyCode.Escape ) )
                SoundManager.Inst.PlaySfx( SoundSfxType.Increase );

            if ( Input.GetKeyDown( KeyCode.Return ) )
            {
                for ( int i = 0; i < 6; i++ )
                {
                    if ( currentKeyCode == GlobalKeySetting.Inst.Keys[( GameKeyAction )i] )
                    {
                        GlobalKeySetting.Inst.Keys[( GameKeyAction )i] = KeyCode.None;
                        trackText[i].text = KeyCode.None.ToString();
                    }
                }

                GlobalKeySetting.Inst.Keys[( GameKeyAction )lane] = currentKeyCode;
                trackText[lane].text = currentKeyCode.ToString();
                SoundManager.Inst.PlaySfx( SoundSfxType.Return );
                Debug.Log( $"Key : {currentKeyCode}" );
                break;
            }
        }

        currentScene?.InputLock( false );
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

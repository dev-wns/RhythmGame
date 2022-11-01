using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeySettingOption : OptionButton
{
    private KeyCode curKeyCode;

    public int lane;
    public TextMeshProUGUI keyText;

    public void Change( KeyCode _key )
    {
        curKeyCode = _key;
        KeySetting.Inst.Keys[( GameKeyCount )6][lane] = curKeyCode;
        keyText.text = KeySetting.Inst.KeyCodeToString( KeySetting.Inst.Keys[( GameKeyCount )6][lane] );
    }

    private void OnEnable()
    {
        Change( KeySetting.Inst.Keys[( GameKeyCount )6][lane] );
    }

    public override void Process()
    {

    }
}

using TMPro;
using UnityEngine;

public class KeySettingOption : OptionButton
{
    private KeyCode curKeyCode;

    public int lane;
    public TextMeshProUGUI keyText;

    public void Change( GameKeyCount _keyCount, KeyCode _key )
    {
        curKeyCode = _key;
        KeySetting.Inst.Keys[_keyCount][lane] = curKeyCode;
        keyText.text = KeySetting.Inst.KeyCodeToString( KeySetting.Inst.Keys[_keyCount][lane] );
    }

    public override void Process()
    {

    }
}
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
    private Dictionary<KeyCode, string> codeList = new Dictionary<KeyCode, string>();

    private void KeyCodeInit()
    {
        codeList.Add( KeyCode.Numlock, "Numlock" );

        codeList.Add( KeyCode.LeftShift, "LeftShift" );
        codeList.Add( KeyCode.LeftAlt, "LeftAlt" );
        codeList.Add( KeyCode.LeftControl, "LeftCtrl" );

        codeList.Add( KeyCode.RightShift, "RightShift" );
        codeList.Add( KeyCode.RightAlt, "RightAlt" );
        codeList.Add( KeyCode.RightControl, "RightCtrl" );

        codeList.Add( KeyCode.BackQuote, "`" );
        codeList.Add( KeyCode.Alpha0, "0" );
        codeList.Add( KeyCode.Alpha1, "1" );
        codeList.Add( KeyCode.Alpha2, "2" );
        codeList.Add( KeyCode.Alpha3, "3" );
        codeList.Add( KeyCode.Alpha4, "4" );
        codeList.Add( KeyCode.Alpha5, "5" );
        codeList.Add( KeyCode.Alpha6, "6" );
        codeList.Add( KeyCode.Alpha7, "7" );
        codeList.Add( KeyCode.Alpha8, "8" );
        codeList.Add( KeyCode.Alpha9, "9" );
        codeList.Add( KeyCode.Minus, "-" );
        codeList.Add( KeyCode.Equals, "=" );

        codeList.Add( KeyCode.Keypad0, "Pad 0" );
        codeList.Add( KeyCode.Keypad1, "Pad 1" );
        codeList.Add( KeyCode.Keypad2, "Pad 2" );
        codeList.Add( KeyCode.Keypad3, "Pad 3" );
        codeList.Add( KeyCode.Keypad4, "Pad 4" );
        codeList.Add( KeyCode.Keypad5, "Pad 5" );
        codeList.Add( KeyCode.Keypad6, "Pad 6" );
        codeList.Add( KeyCode.Keypad7, "Pad 7" );
        codeList.Add( KeyCode.Keypad8, "Pad 8" );
        codeList.Add( KeyCode.Keypad9, "Pad 9" );
        codeList.Add( KeyCode.KeypadPeriod, "Pad ." );
        codeList.Add( KeyCode.KeypadDivide, "Pad /" );
        codeList.Add( KeyCode.KeypadMultiply, "Pad *" );
        codeList.Add( KeyCode.KeypadMinus, "Pad -" );
        codeList.Add( KeyCode.KeypadPlus, "Pad +" );
        codeList.Add( KeyCode.KeypadEquals, "Pad =" );

        codeList.Add( KeyCode.Q, "Q" );
        codeList.Add( KeyCode.W, "W" );
        codeList.Add( KeyCode.E, "E" );
        codeList.Add( KeyCode.R, "R" );
        codeList.Add( KeyCode.T, "T" );
        codeList.Add( KeyCode.Y, "Y" );
        codeList.Add( KeyCode.U, "U" );
        codeList.Add( KeyCode.I, "I" );
        codeList.Add( KeyCode.O, "O" );
        codeList.Add( KeyCode.P, "P" );
        codeList.Add( KeyCode.LeftBracket, "[" );
        codeList.Add( KeyCode.RightBracket, "]" );
        codeList.Add( KeyCode.Backslash, "\\" );

        codeList.Add( KeyCode.A, "A" );
        codeList.Add( KeyCode.S, "S" );
        codeList.Add( KeyCode.D, "D" );
        codeList.Add( KeyCode.F, "F" );
        codeList.Add( KeyCode.G, "G" );
        codeList.Add( KeyCode.H, "H" );
        codeList.Add( KeyCode.J, "J" );
        codeList.Add( KeyCode.K, "K" );
        codeList.Add( KeyCode.L, "L" );
        codeList.Add( KeyCode.Semicolon, ";" );
        codeList.Add( KeyCode.Quote, "\'" );

        codeList.Add( KeyCode.Z, "Z" );
        codeList.Add( KeyCode.X, "X" );
        codeList.Add( KeyCode.C, "C" );
        codeList.Add( KeyCode.V, "V" );
        codeList.Add( KeyCode.B, "B" );
        codeList.Add( KeyCode.N, "N" );
        codeList.Add( KeyCode.M, "M" );
        codeList.Add( KeyCode.Comma, "," );
        codeList.Add( KeyCode.Period, "." );
        codeList.Add( KeyCode.Slash, "/" );

        codeList.Add( KeyCode.Home,     "Home" );
        codeList.Add( KeyCode.Insert,   "Insert" );
        codeList.Add( KeyCode.PageUp,   "PageUp" );
        codeList.Add( KeyCode.Delete,   "Delete" );
        codeList.Add( KeyCode.End,      "End" );
        codeList.Add( KeyCode.PageDown, "PageDown" );
    }

    protected override void Awake()
    {
        base.Awake();

        KeyCodeInit();
    }

    public string GetKeyCodeToString( KeyCode _code )
    {
        if ( codeList.ContainsKey( _code ) )
        {
            return codeList[_code];
        }

        return "None";
    }

    private void OnEnable()
    {
        trackText[lane].text = GetKeyCodeToString( GameSetting.Inst.Keys[( GameKeyAction )lane] );
    }

    public override void Process()
    {
        keySettingPanel.SetActive( true );
        panelText.text = GetKeyCodeToString( GameSetting.Inst.Keys[( GameKeyAction )lane] );

        CurrentScene?.InputLock( true );
        StartCoroutine( ChangeGameKey() );
    }

    public IEnumerator ChangeGameKey()
    {
        curKeyCode = GameSetting.Inst.Keys[( GameKeyAction )lane];
        while ( true )
        {
            yield return new WaitUntil( () => Input.anyKeyDown && !Input.GetKeyDown( KeyCode.Return ) );

            SoundManager.Inst.Play( SoundSfxType.MenuSelect );

            for ( int i = 0; i < 6; i++ )
            {
                if ( curKeyCode == GameSetting.Inst.Keys[( GameKeyAction )i] )
                {
                    GameSetting.Inst.Keys[( GameKeyAction )i] = KeyCode.None;
                    trackText[i].text = KeyCode.None.ToString();
                }
            }

            GameSetting.Inst.Keys[( GameKeyAction )lane] = curKeyCode;
            trackText[lane].text = GetKeyCodeToString( curKeyCode );
            break;
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

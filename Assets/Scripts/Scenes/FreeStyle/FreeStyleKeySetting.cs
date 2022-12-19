using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FreeStyleKeySetting : OptionController
{
    public TextMeshProUGUI KeyCountText;
    private CustomHorizontalLayoutGroup layoutGroup;
    private List<KeySettingOption> tracks = new List<KeySettingOption>();
    private KeyCode curKeyCode;
    private GameKeyCount[] changeKeyCount = new GameKeyCount[] {GameKeyCount._4,  GameKeyCount._6, GameKeyCount._7};
    private GameKeyCount curKeyCount;
    private int curKeyIndex;

    protected override void Awake()
    {
        base.Awake();
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();
    }

    protected override void CreateOptions()
    {
        foreach ( var option in options )
        {
            tracks.Add( option.GetComponent<KeySettingOption>() );
        }
    }

    private void OnEnable()
    {
        Initialize( 0 );
    }

    private void Initialize( int _curIndex )
    {
        curKeyIndex = _curIndex;
        curKeyCount = changeKeyCount[curKeyIndex];

        Length = KeySetting.Inst.Keys[curKeyCount].Length;
        for ( int i = 0; i < 7; i++ )
        {
            tracks[i].KeyRemove();
            tracks[i].ActiveOutline( false );
            if ( i < Length )
            {
                tracks[i].gameObject.SetActive( true );
                tracks[i].Change( curKeyCount, KeySetting.Inst.Keys[curKeyCount][i] );
            }
            else
            {
                tracks[i].gameObject.SetActive( false );
            }
        }

        Select( 0 );
        tracks[0].ActiveOutline( true );
        layoutGroup.SetLayoutHorizontal();
        KeyCountText.text = $"{Length}K Setting";
    }


    public void ChangeButtonCount()
    {
        SoundManager.Inst.Play( SoundSfxType.MenuClick );
        Initialize( curKeyIndex + 1 < changeKeyCount.Length ? curKeyIndex + 1 : 0 );
    }

    private void Process( KeyCode _key )
    {
        if ( KeySetting.Inst.IsAvailableKey( _key ) )
        {
            for ( int i = 0; i < KeySetting.Inst.Keys[curKeyCount].Length; i++ )
            {
                if ( KeySetting.Inst.Keys[curKeyCount][i] == _key )
                     tracks[i].Change( curKeyCount, KeyCode.None );
            }

            SoundManager.Inst.Play( SoundSfxType.MenuSelect );
            tracks[CurrentIndex].Change( curKeyCount, _key );

            NextMove();
        }
    }

    private void Update()
    {
        foreach ( var key in KeySetting.Inst.AvailableKeys )
        {
            var keyCode = key.Key;
            if ( Input.GetKeyDown( keyCode ) )
            {
                curKeyCode = keyCode;
                Process( curKeyCode );
                break;
            }
        }
    }
}

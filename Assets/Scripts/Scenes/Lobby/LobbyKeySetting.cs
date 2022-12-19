using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyKeySetting : SceneOptionBase
{
    public GameObject keySettingCanvas;
    public TextMeshProUGUI KeyCountText;
    private CustomHorizontalLayoutGroup layoutGroup;
    private List<KeySettingOption> tracks = new List<KeySettingOption>();
    private KeyCode curKeyCode;
    private GameKeyCount[] changeKeyCounts = new GameKeyCount[] {GameKeyCount._4,  GameKeyCount._6, GameKeyCount._7};
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
        CurrentScene.ChangeAction( ActionType.KeySetting );
        curKeyIndex = -1;
        ChangeButtonCount();
    }

    private void ChangeButtonCount()
    {
        curKeyIndex = curKeyIndex + 1 < changeKeyCounts.Length ? curKeyIndex + 1 : 0;
        curKeyCount = changeKeyCounts[curKeyIndex];
        Length = KeySetting.Inst.Keys[curKeyCount].Length;
        for ( int i = 0; i < 7; i++ )
        {
            tracks[i].KeyRemove();
            tracks[i].ActiveOutline( false );
            bool isActive = i < Length;
            tracks[i].gameObject.SetActive( isActive );

            if ( isActive )
                 tracks[i].Change( curKeyCount, KeySetting.Inst.Keys[curKeyCount][i] );
        }

        Select( 0 );
        tracks[0].ActiveOutline( true );
        layoutGroup.SetLayoutHorizontal();
        KeyCountText.text = $"{Length}K Setting";
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

    public override void KeyBind()
    {
        CurrentScene.Bind( ActionType.KeySetting, KeyCode.LeftArrow, () => PrevMove() );
        CurrentScene.Bind( ActionType.KeySetting, KeyCode.LeftArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( ActionType.KeySetting, KeyCode.RightArrow, () => NextMove() );
        CurrentScene.Bind( ActionType.KeySetting, KeyCode.RightArrow, () => SoundManager.Inst.Play( SoundSfxType.MenuSelect ) );

        CurrentScene.Bind( ActionType.KeySetting, KeyCode.Tab, () => ChangeButtonCount() );

        CurrentScene.Bind( ActionType.KeySetting, KeyCode.Escape, () => CurrentScene.ChangeAction( ActionType.SystemOption ) );
        CurrentScene.Bind( ActionType.KeySetting, KeyCode.Escape, () => keySettingCanvas.SetActive( false ) );
        CurrentScene.Bind( ActionType.KeySetting, KeyCode.Escape, () => SoundManager.Inst.Play( SoundSfxType.MenuHover ) );
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

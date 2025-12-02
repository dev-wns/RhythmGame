using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FreeStyleKeySetting : OptionController
{
    [Header("KeySetting")]
    public TextMeshProUGUI KeyCountText;
    private CustomHorizontalLayoutGroup layoutGroup;
    private List<KeySettingOption> tracks = new List<KeySettingOption>();
    private KeyCode curKeyCode;
    private GameKeyCount[] keyCount = new GameKeyCount[] {GameKeyCount._4,  GameKeyCount._6, GameKeyCount._7};
    private GameKeyCount curKeyCount;
    private int curKeyIndex;

    protected override void Awake()
    {
        base.Awake();
        IsLoop = true;
        if ( contents.TryGetComponent( out layoutGroup ) )
             layoutGroup.Initialize();

        foreach ( var option in options )
        {
            if ( option.TryGetComponent( out KeySettingOption keyOption ) )
                tracks.Add( keyOption );
            else
                Debug.LogWarning( $"The {option.name} does not have Option Component" );
        }
    }

    private void OnEnable() => Initialize( 0 );

    private void Initialize( int _index )
    {
        curKeyIndex = _index;
        curKeyCount = keyCount[curKeyIndex];

        Length = InputManager.Keys[curKeyCount].Length;
        for ( int i = 0; i < 7; i++ )
        {
            tracks[i].ActiveOutline( false );
            if ( i < Length )
            {
                tracks[i].gameObject.SetActive( true );
                tracks[i].Change( curKeyCount, InputManager.Keys[curKeyCount][i] );
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


    public void ChangeButtonCount( bool _isPrevious )
    {
        int index = 0;
        if ( _isPrevious ) index = curKeyIndex - 1 > -1              ? curKeyIndex - 1 : keyCount.Length - 1;
        else               index = curKeyIndex + 1 < keyCount.Length ? curKeyIndex + 1 : 0;

        Initialize( index );
        AudioManager.Inst.Play( SFX.MenuClick );
    }

    private void Process( KeyCode _key )
    {
        if ( InputManager.IsAvailable( _key ) )
        {
            for ( int i = 0; i < InputManager.Keys[curKeyCount].Length; i++ )
            {
                if ( InputManager.Keys[curKeyCount][i] == _key )
                     tracks[i].Change( curKeyCount, KeyCode.None );
            }

            AudioManager.Inst.Play( SFX.MenuSelect );
            tracks[CurrentIndex].Change( curKeyCount, _key );

            NextMove();
        }
    }

    protected override void Update()
    {
        base.Update();
        foreach ( var keyCode in InputManager.AvailableKeys )
        {
            if ( Input.GetKeyDown( keyCode ) )
            {
                curKeyCode = keyCode;
                Process( curKeyCode );
                break;
            }
        }
    }
}

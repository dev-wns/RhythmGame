using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoFailOption : OptionText
{
    private void OnEnable()
    {
        curIndex = GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail ) ? 1 : 0;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )BooleanOption.Count; i++ )
        {
            texts.Add( ( ( BooleanOption )i ).ToString() );
        }
    }

    public override void Process()
    {
        if ( curIndex == 0 ) GameSetting.CurrentGameMode &= ~GameMode.NoFail;
        else                 GameSetting.CurrentGameMode |=  GameMode.NoFail;
        Debug.Log( GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail ) );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlayOption : OptionText
{
    private void OnEnable()
    {
        curIndex = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay ) ? 1 : 0;
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
        if ( curIndex == 0 ) GameSetting.CurrentGameMode &= ~GameMode.AutoPlay;
        else                 GameSetting.CurrentGameMode |=  GameMode.AutoPlay;
    }
}

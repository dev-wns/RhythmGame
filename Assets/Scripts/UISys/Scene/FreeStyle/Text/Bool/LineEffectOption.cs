using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffectOption : OptionText
{
    private void OnEnable()
    {
        curIndex = GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.LaneEffect ) ? 1 : 0;
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
        if ( curIndex == 0 ) GameSetting.CurrentVisualFlag &= ~GameVisualFlag.LaneEffect;
        else                 GameSetting.CurrentVisualFlag |=  GameVisualFlag.LaneEffect;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffectOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = GameSetting.CurrentVisualMod.HasFlag( VisualMod.LineEffect ) ? 1 : 0;
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
        if ( curIndex == 0 ) GameSetting.CurrentVisualMod &= ~VisualMod.LineEffect;
        else                 GameSetting.CurrentVisualMod |=  VisualMod.LineEffect;
        Debug.Log( GameSetting.CurrentVisualMod.HasFlag( VisualMod.LineEffect ) );
    }
}

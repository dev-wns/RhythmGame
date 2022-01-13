using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMeasureOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowMeasure ) ? 1 : 0;
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
        if ( curIndex == 0 ) GameSetting.CurrentVisualFlag &= ~GameVisualFlag.ShowMeasure;
        else                 GameSetting.CurrentVisualFlag |=  GameVisualFlag.ShowMeasure;
        Debug.Log( GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowMeasure ) );
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowJudgeOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowJudge ) ? 1 : 0;
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
        if ( curIndex == 0 ) GameSetting.CurrentVisualFlag &= ~GameVisualFlag.ShowJudge;
        else                 GameSetting.CurrentVisualFlag |=  GameVisualFlag.ShowJudge;
        Debug.Log( GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowJudge ) );
    }
}

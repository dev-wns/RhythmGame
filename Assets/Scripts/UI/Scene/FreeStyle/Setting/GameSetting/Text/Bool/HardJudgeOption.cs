using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardJudgeOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = GameSetting.CurrentGameMode.HasFlag( GameMode.HardJudge ) ? 1 : 0;
        ChangeText( texts[CurrentIndex] );
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
        if ( CurrentIndex == 0 ) GameSetting.CurrentGameMode &= ~GameMode.HardJudge;
        else                     GameSetting.CurrentGameMode |=  GameMode.HardJudge;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureCreateOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = GameSetting.IsCreateMeasure ? 1 : 0;
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
        GameSetting.IsCreateMeasure = curIndex == 0 ? false : true;
        Debug.Log( GameSetting.IsCreateMeasure );
        Debug.Log( ( BooleanOption )curIndex );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearAlignmentOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )GameSetting.GearAlignment;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )Alignment.Count; i++ )
        {
            texts.Add( ( ( Alignment )i ).ToString() );
        }
    }

    public override void Process()
    {
        GameSetting.GearAlignment = ( Alignment )curIndex;
        Debug.Log( ( Alignment )curIndex );
    }
}

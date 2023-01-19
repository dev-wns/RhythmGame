using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearAlignmentOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = ( int )GameSetting.CurrentGearAlignment;
        ChangeText( texts[CurrentIndex] );
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
        GameSetting.CurrentGearAlignment = ( Alignment )CurrentIndex;
    }
}

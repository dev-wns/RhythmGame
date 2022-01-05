using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrameRateOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )GameSetting.GameFader;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )FRAME_RATE.Count; i++ )
        {
            texts.Add( ( ( FRAME_RATE )i ).ToString() );
        }
    }

    public override void Process()
    {
        SystemSetting.FrameRate = ( FRAME_RATE )curIndex;
        Debug.Log( ( FRAME_RATE )curIndex );
    }
}

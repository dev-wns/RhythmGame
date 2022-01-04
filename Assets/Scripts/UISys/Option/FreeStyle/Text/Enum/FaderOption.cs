using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaderOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )GameSetting.GameFader;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )FADER.Count; i++ )
        {
            texts.Add( ( ( FADER )i ).ToString() );
        }
    }

    public override void Process()
    {
        GameSetting.GameFader = ( FADER )curIndex;
        Debug.Log( ( FADER )curIndex );
    }
}

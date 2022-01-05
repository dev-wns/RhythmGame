using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResolutionOption : OptionText
{ 
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )GameSetting.GameFader;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )RESOLUTION.Count; i++ )
        {
            texts.Add( ( ( RESOLUTION )i ).ToString() );
        }
    }

    public override void Process()
    {
        SystemSetting.Resolution = ( RESOLUTION )curIndex;
        Debug.Log( ( RESOLUTION )curIndex );
    }
}

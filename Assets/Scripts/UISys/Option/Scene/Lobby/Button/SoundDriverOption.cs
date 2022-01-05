using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SoundDriverOption : OptionText
{ 
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )GameSetting.GameFader;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        var drivers = SoundManager.Inst.soundDrivers;
        for ( int i = 0; i < drivers.Count; i++ )
        {
            string newData = drivers[i].name;

            var split = newData.Split( '(' );
            if( split.Length > 1 )
                newData = split[0] + "\n(" + split[1];

            texts.Add( newData );
        }
    }

    public override void Process()
    {
        // GameSetting.GameFader = ( FADER )curIndex;
        Debug.Log( SoundManager.Inst.soundDrivers[curIndex].name );
    }
}

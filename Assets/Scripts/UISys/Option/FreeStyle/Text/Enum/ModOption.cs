using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )GameSetting.GameMod;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )MOD.Count; i++ )
        {
            texts.Add( ( ( MOD )i ).ToString() );
        }
    }

    public override void Process()
    {
        GameSetting.GameMod = ( MOD )curIndex;
        Debug.Log( ( MOD )curIndex );
    }
}

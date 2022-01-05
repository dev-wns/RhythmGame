using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public interface IOptionCreate
{
    public void CreateObjects();
}

public class ScreenModOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )GameSetting.GameFader;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        for ( int i = 0; i < ( int )SCREEN_MOD.Count; i++ )
        {
            texts.Add( ( ( SCREEN_MOD )i ).ToString() );
        }
    }

    public override void Process()
    {
        SystemSetting.ScreenMod = ( SCREEN_MOD )curIndex;
        Debug.Log( ( SCREEN_MOD )curIndex );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ModOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )GameSetting.Mod;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )GameMod.Count; i++ )
        {
            var text = ( ( GameMod )i ).ToString();
            builder.Clear();
            builder.Append( text.Replace( "_", " " ).Trim() );
            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        GameSetting.Mod = ( GameMod )curIndex;
        Debug.Log( ( GameMod )curIndex );
    }
}

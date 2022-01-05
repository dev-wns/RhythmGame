using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ScreenModOption : OptionText
{
    protected override void Awake()
    {
        base.Awake();

        curIndex = ( int )SystemSetting.ScreenMod;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )SCREEN_MOD.Count; i++ )
        {
            var text = ( ( SCREEN_MOD )i ).ToString();

            builder.Clear();
            builder.Append( text.Replace( "_", " " ).Trim() );

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        SystemSetting.ScreenMod = ( SCREEN_MOD )curIndex;
        Debug.Log( ( SCREEN_MOD )curIndex );
    }
}

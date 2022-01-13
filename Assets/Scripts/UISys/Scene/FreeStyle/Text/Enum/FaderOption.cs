using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class FaderOption : OptionText
{
    private void OnEnable()
    {
        curIndex = ( int )GameSetting.CurrentFader;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )GameFader.Count; i++ )
        {
            var text = ( ( GameFader )i ).ToString();

            builder.Clear();
            builder.Append( text.Replace( "_", " " ).Trim() );

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        GameSetting.CurrentFader = ( GameFader )curIndex;
        Debug.Log( ( GameFader )curIndex );
    }
}

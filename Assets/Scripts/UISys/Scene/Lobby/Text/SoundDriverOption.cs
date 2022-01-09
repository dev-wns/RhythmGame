using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class SoundDriverOption : OptionText
{ 
    protected override void Awake()
    {
        base.Awake();

        curIndex = SoundManager.Inst.CurrentDriverIndex;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        var drivers = SoundManager.Inst.soundDrivers;
        for ( int i = 0; i < drivers.Count; i++ )
        {
            string text = drivers[i].name;
            var split = text.Split( '(' );
            
            builder.Clear();
            builder.Append( split[0] );
            if ( split.Length > 1 )
            {
                builder.Append( "\n(" );
                builder.Append( split[1] );
            }

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        SoundManager.Inst.SetDriver( curIndex );
    }
}

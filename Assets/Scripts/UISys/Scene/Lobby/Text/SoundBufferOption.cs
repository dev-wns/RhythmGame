using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class SoundBufferOption : OptionText
{
    private void OnEnable()
    {
        curIndex = ( int )SystemSetting.CurrentSoundBuffer;
        ChangeText( texts[curIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )SoundBuffer.Count; i++ )
        {
            var text = ( ( SoundBuffer )i ).ToString();
            builder.Clear();
            builder.Append( text.Replace( "_", " " ).Trim() );

            if ( ( SoundBuffer )i == SoundBuffer._1024 )
                 builder.Append( "(±âº»°ª)" );

            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        SystemSetting.CurrentSoundBuffer = ( SoundBuffer )curIndex;
        SoundManager.Inst.ReLoad();
    }
}

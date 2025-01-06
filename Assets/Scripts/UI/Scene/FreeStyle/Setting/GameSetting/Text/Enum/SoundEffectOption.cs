using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class SoundEffectOption : OptionText
{
    private void OnEnable()
    {
        CurrentIndex = ( int )GameSetting.CurrentPitchType;
        ChangeText( texts[CurrentIndex] );
    }

    protected override void CreateObject()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )PitchType.Count; i++ )
        {
            var text = ( ( PitchType )i ).ToString();
            builder.Clear();
            builder.Append( text.Replace( "_", " " ).Trim() );
            texts.Add( builder.ToString() );
        }
    }

    public override void Process()
    {
        GameSetting.CurrentPitchType = ( PitchType )CurrentIndex;
        AudioManager.Inst.UpdatePitchShift();
    }
}

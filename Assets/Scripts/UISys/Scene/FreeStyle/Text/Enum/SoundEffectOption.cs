using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class SoundEffectOption : OptionText
{
    private void OnEnable()
    {
        curIndex = ( int )GameSetting.CurrentPitchType;
        ChangeText( texts[curIndex] );
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
        GameSetting.CurrentPitchType = ( PitchType )curIndex;
        
        switch ( GameSetting.CurrentPitchType )
        {
            case PitchType.None:
            // dsp Á¦°Å
            SoundManager.Inst.RemovePitchShift();
            break;

            case PitchType.Normalize:
            case PitchType.Nightcore:
            SoundManager.Inst.AddPitchShift();
            SoundManager.Inst.UpdatePitchShift();
            break;
        }
    }
}

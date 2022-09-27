using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeOption : OptionSlider
{
    public ChannelType groupType = ChannelType.Master;

    private void OnEnable()
    {
        curValue = Global.Math.Round( SoundManager.Inst.GetVolume( groupType ) * 100f );
        UpdateValue( curValue );
    }

    public override void Process()
    {
        SoundManager.Inst.SetVolume( curValue * .01f, groupType );
    }
}

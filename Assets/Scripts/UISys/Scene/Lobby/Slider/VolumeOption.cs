using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeOption : OptionSlider
{
    public ChannelGroupType groupType = ChannelGroupType.Master;

    protected override void Awake()
    {
        base.Awake();

        curValue = SoundManager.Inst.GetVolume( groupType ) * 100f;
        UpdateValue( curValue );
    }

    public override void Process()
    {
        SoundManager.Inst.SetVolume( curValue * .01f, groupType );
        Debug.Log( curValue );
    }
}

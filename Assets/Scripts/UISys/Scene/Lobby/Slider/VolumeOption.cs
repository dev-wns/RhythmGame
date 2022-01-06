using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeOption : OptionSlider
{
    public CHANNEL_GROUP_TYPE groupType = CHANNEL_GROUP_TYPE.MASTER;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriverSetting : CustomButton
{
    public override void Process()
    {
        base.Process();
        // SoundManager.Inst.SetDriver( key );
        Debug.Log( $"DriverSetting {transform.GetChild( key ).name} " );
    }
}

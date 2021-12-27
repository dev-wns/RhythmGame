using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriverSetting : LobbyOptionButton
{
    public override void Process()
    {
        // SoundManager.Inst.SetDriver( key );
        Debug.Log( $"DriverSetting {transform.GetChild( key ).name} " );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resolution : LobbyOptionButton
{
    public int width, height;

    public override void Process()
    {
        var splitData = transform.GetChild( key ).name.Split('x');
        width  = int.Parse( splitData[0] );
        height = int.Parse( splitData[1] );

        Debug.Log( $"Resolution {width}x{height}" );
        //Screen.SetResolution( width, height, false );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BmsParser : Parser
{
    public BmsParser( string _path ) : base( _path ) { song.type = ParseType.Bms; }

    public override Song PreRead()
    {
        throw new System.NotImplementedException();
    }

    public override Chart PostRead( Song _song )
    {
        throw new System.NotImplementedException();
    }
}

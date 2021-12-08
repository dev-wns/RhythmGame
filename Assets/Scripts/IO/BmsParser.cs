using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BmsParser : Parser
{
    public BmsParser( string _path ) : base( _path ) { }

    public override Song PreRead()
    {
        throw new System.NotImplementedException();
    }

    public override Chart PostRead()
    {
        throw new System.NotImplementedException();
    }
}

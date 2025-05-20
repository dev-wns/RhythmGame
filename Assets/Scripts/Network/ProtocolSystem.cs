using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ProtocolSystem : Singleton<ProtocolSystem>
{
    private Dictionary<PacketType, Action<Packet>> protocols = new Dictionary<PacketType, Action<Packet>>();
    private Queue<Packet> packets = new Queue<Packet>();

    protected override void Awake()
    {
        base.Awake();

        StartCoroutine( Process() );
    }

    public void Push( in Packet _packet ) => packets.Enqueue( _packet );

    public void Regist( PacketType _type, Action<Packet> _func )
    {
        if ( protocols.ContainsKey( _type ) )
        {
            protocols[_type] = _func;
            return;
        }

        protocols.Add( _type, _func );
    }

    private IEnumerator Process()
    {
        WaitUntil waitConnectNetwork = new WaitUntil( () => { return Network.Inst.IsConnected; } );
        WaitUntil waitReceivePackets = new WaitUntil( () => { return packets.Count > 0; } );
        while ( true )
        {
            if ( !Network.Inst.IsConnected )
                yield return waitConnectNetwork;

            yield return waitReceivePackets;
            while ( packets.Count > 0 )
            {
                Packet packet = packets.Dequeue();
                if ( !protocols.ContainsKey( packet.type ) )
                {
                    Debug.LogWarning( $"The {packet.type} protocol is not registered." );
                    continue;
                }

                protocols[packet.type]?.Invoke( packet );
            }
        }
    }
}
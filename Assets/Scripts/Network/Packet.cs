using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;

[StructLayout( LayoutKind.Sequential, Pack = 1 )]
public struct Packet
{
    public Error error;
    public PacketType type;
    public ushort size;
    public byte[] data;

    public Packet( Error _error, PacketType _type, ushort _size, byte[] _data )
    {
        error = _error;
        type = _type;
        size = _size;
        data = _data;
    }

    public Packet( PacketType _type )
    {
        error = Error.OK;
        type = _type;
        size = Network.HeaderSize;

        data = new byte[size];
        BitConverter.TryWriteBytes( new Span<byte>( data, 0, sizeof( ushort ) ), ( ushort )error );
        BitConverter.TryWriteBytes( new Span<byte>( data, 2, sizeof( ushort ) ), ( ushort )type );
        BitConverter.TryWriteBytes( new Span<byte>( data, 4, sizeof( ushort ) ), ( ushort )size );
    }

    public Packet( PacketType _type, IProtocol _protocol )
    {
        error = Error.OK;
        type = _type;
        byte[] json = System.Text.Encoding.UTF8.GetBytes( JsonConvert.SerializeObject( _protocol ) );
        size = json.Length > 2 ? ( ushort )( json.Length + Network.HeaderSize ) // 정상 패킷
                               : ( ushort )Network.HeaderSize;                  // 빈 패킷은 JSON 변환시 {} 2문자만 들어감

        data = new byte[size];
        BitConverter.TryWriteBytes( new Span<byte>( data, 0, sizeof( ushort ) ), ( ushort )error );
        BitConverter.TryWriteBytes( new Span<byte>( data, 2, sizeof( ushort ) ), ( ushort )type );
        BitConverter.TryWriteBytes( new Span<byte>( data, 4, sizeof( ushort ) ), ( ushort )size );

        if ( json.Length > 2 )
            Buffer.BlockCopy( json, 0, data, Network.HeaderSize, json.Length );
    }

    public static Type FromJson<Type>( in Packet _packet )
    {
        return JsonConvert.DeserializeObject<Type>( System.Text.Encoding.UTF8.GetString( _packet.data ) );
    }
}
#include "Network.h"

Network::Network( const SOCKET& _socket, const SOCKADDR_IN& _address ) 
	: socket( _socket ), address( _address ), wsaRecvBuffer{}, wsaSendBuffer{}, buffer{} { }

Network::~Network()
{
	CloseSocket();
}

bool Network::CloseSocket()
{
	return ::closesocket( socket ) != SOCKET_ERROR;
}

bool Network::Connect() const
{
	return ::connect( socket, ( sockaddr* )&address, sizeof( address ) ) != SOCKET_ERROR;
}

void Network::Send( const UPacket& _packet )
{
	if ( !LogText::Inst().ignoreData && _packet.type != PACKET_HEARTBEAT )
		 Debug.Log( "Send ( ", magic_enum::enum_name( _packet.type ).data(), ", ", _packet.size, "bytes ) ", _packet.data );
	
	DWORD transferred = 0;
	OVERLAPPEDEX* ov = new OVERLAPPEDEX( OVERLAPPEDEX::MODE_SEND );
	wsaSendBuffer.buf = ( char* )&_packet;
	wsaSendBuffer.len = _packet.size;

	if ( ::WSASend( socket, &wsaSendBuffer, 1, &transferred, 0, ( LPOVERLAPPED )ov, NULL ) == SOCKET_ERROR )
	{
		switch ( int error = ::WSAGetLastError() )
		{
			default:
			{
				Debug.LogError( "< Send LastError > ", ::WSAGetLastError() );
			} break;

			case WSA_IO_PENDING:
			{

			} break;

			case WSAECONNABORTED:
			{
				
			} break;
		}
	}
}

void Network::Recieve()
{
	DWORD flag = 0;
	DWORD transferred = 0;
	OVERLAPPEDEX* ov = new OVERLAPPEDEX( OVERLAPPEDEX::MODE_RECV );
	wsaRecvBuffer.buf = ( char* )buffer;
	wsaRecvBuffer.len = Global::HeaderSize + Global::MaxDataSize;
	if ( ::WSARecv( socket, &wsaRecvBuffer, 1, &transferred, &flag, ( LPOVERLAPPED )ov, NULL ) == SOCKET_ERROR )
	{
		switch ( int error = ::WSAGetLastError() )
		{
			default:
			{
				if ( ::WSAGetLastError() != WSA_IO_PENDING )
					 Debug.LogError( "< Recieve LastError > ", ::WSAGetLastError() );
			} break;

			case WSA_IO_PENDING:
			{
				
			} break;
		}
	}
}

const SOCKET& Network::GetSocket()
{
	return socket;
}

std::string Network::GetAddress() const
{
	std::string addr = ::inet_ntoa( address.sin_addr );
	size_t pos = addr.find_first_of( '.' );
	for ( size_t i = pos; i < addr.size(); i++ )
	{
		if ( addr[i] != '.' )
			 addr[i]  = '*';
	}
	return addr;
}

std::string Network::GetPort() const
{
	return std::to_string( ::ntohs( address.sin_port ) );
}

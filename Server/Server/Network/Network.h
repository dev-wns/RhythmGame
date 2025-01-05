#pragma once
#include "Global/Header.h"
#include "Packet/Packet.hpp"
#include "Global/LogText.hpp"

class Network
{
protected:
	struct OVERLAPPEDEX : OVERLAPPED
	{
		enum : u_short { MODE_RECV = 0, MODE_SEND, };
		u_short flag;

		OVERLAPPEDEX() : flag( MODE_RECV ) { ZeroMemory( this, sizeof( OVERLAPPED ) ); }
		OVERLAPPEDEX( u_short _flag ) : flag( _flag ) { ZeroMemory( this, sizeof( OVERLAPPED ) ); }
	};

	SOCKET socket;
	SOCKADDR_IN address;
	WSABUF wsaRecvBuffer;
	WSABUF wsaSendBuffer;

private:
	char buffer[Global::HeaderSize + Global::MaxDataSize];

public:
	Network() = default;
	Network( const SOCKET& _socket, const SOCKADDR_IN& _address );
	virtual ~Network();

public:
	bool CloseSocket();

public:
	bool Connect() const;
	void Send( const UPacket& _packet );
	void Recieve();

public:
	const SOCKET& GetSocket();
	std::string GetAddress() const;
	std::string GetPort() const;
};
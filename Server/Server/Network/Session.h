#pragma once
#include "Network/Network.h"

static const u_short MaxReceiveSize = 10000;

class Stage;
class Session : public Network
{
private:
	UPacket* packet;
	byte buffer[MaxReceiveSize];
	u_int startPos, writePos, readPos;

public:
	LOGIN_INFO loginInfo;
	Stage* stage;
	SerialType serial;

public:
	Session() = default;
	Session( const SOCKET& _socket, const SOCKADDR_IN& _address );
	virtual ~Session() override;

public:
	void Dispatch( const LPOVERLAPPED& _ov, DWORD _size );
};
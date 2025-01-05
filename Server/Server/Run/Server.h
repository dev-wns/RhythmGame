#pragma once
#include "Network/Acceptor.h"

class Server
{
private:
	Acceptor acceptor;
	HANDLE killEvent;


public:
	Server( const int _port, const char* _address = 0 );
	virtual ~Server() = default;
};
#pragma once
#include "Network/Network.h"

class Acceptor : public Network
{
public:
	Acceptor() = default;
	virtual ~Acceptor() override;

public:
	bool Accept( int _port, const char* _ip );

private:
	void WaitForClients()  const;
	bool Listen();
};
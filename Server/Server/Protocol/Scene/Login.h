#pragma once
#include "Management/ProtocolSystem.h"

class Login : public IScene
{
public:
	Login()          = default;
	virtual ~Login() = default;

public:
	virtual void Bind() override;

private:
	static void ConfirmMatchData( const Packet& _packet );
	static void AddToDatabase( const Packet& _packet );
	static void ConfirmDuplicateInfo( const Packet& _packet );
};
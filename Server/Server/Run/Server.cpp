#include "Server.h"
#include "Management/IOCP.h"
#include "Management/PacketSystem.h"
#include "Management/SessionManager.h"
#include "Database/Database.h"

Server::Server( const int _port, const char* _address )
{
	killEvent = ::CreateEvent( NULL, FALSE, FALSE, _T( "KillEvent" ) );

	if ( !Database::Inst().Initialize() )
	{
		std::cout << "MySQL connect failed" << std::endl;
	}

	if ( !IOCP::Inst().Initialize() )
	{
		std::cout << "IOCP initialize failed" << std::endl;
	}

	if ( !PacketSystem::Inst().Initialize() )
	{
		std::cout << "Packet processing failed" << std::endl;
	}

	if ( !acceptor.Accept( _port, _address ) )
	{
		std::cout << "Accept failed" << std::endl;
	}

	if ( ::WaitForSingleObject( killEvent, INFINITE ) == WAIT_FAILED )
	{

	}
}
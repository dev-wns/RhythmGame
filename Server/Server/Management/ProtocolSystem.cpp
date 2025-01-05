#include "ProtocolSystem.h"
#include "SessionManager.h"
#include "Protocol/Scene/Login.h"
#include "Protocol/Scene/Lobby.h"
#include "Protocol/Scene/InGame.h"

ProtocolSystem::~ProtocolSystem()
{
	for ( auto iter = scenes.begin(); iter != scenes.end(); iter++ )
		  Global::Memory::SafeDelete( *iter );
}

void ProtocolSystem::Initialize()
{
	scenes.push_back( new Login() );
	scenes.push_back( new Lobby() );
	scenes.push_back( new InGame() );

	Bind();
	std::cout << "Function binding completed for packet processing" << std::endl;
}

void ProtocolSystem::Bind()
{
	Regist( PACKET_HEARTBEAT, []( const Packet& ) {} );

	for ( auto iter = scenes.begin(); iter != scenes.end(); iter++ )
	{
		IScene* scene = *iter;
		if ( scene != nullptr )
			 scene->Bind();
	}
}

void ProtocolSystem::Process( const Packet& _packet )
{
	if ( !protocols.contains( _packet.type ) )
	{
		Debug.LogError( "< ", magic_enum::enum_name( _packet.type ).data(), " > protocol is not registered" );
		return;
	}

	protocols[_packet.type]( _packet );
}

void ProtocolSystem::Regist( const PacketType& _type, void( *_func )( const Packet& ) )
{
	if ( protocols.contains( _type ) )
	{
		Debug.LogError( "< ", magic_enum::enum_name( _type ).data(), " > protocol is duplicated" );
		return;
	}

	protocols[_type] = _func;
}

void ProtocolSystem::Broadcast( const Packet& _packet )
{
	for ( const auto& session : SessionManager::Inst().GetSessions() )
		  session->Send( _packet );
}

void ProtocolSystem::BroadcastWithoutSelf( const Packet& _packet )
{
	for ( const auto& session : SessionManager::Inst().GetSessions() )
	{
		if ( session->GetSocket() != _packet.session->GetSocket() )
			 session->Send( _packet );
	}
}
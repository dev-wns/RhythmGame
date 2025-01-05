#include "SessionManager.h"
#include "ProtocolSystem.h"
#include "Protocol/Protocol.hpp"
#include "StageManager.h"

SessionManager::~SessionManager()
{
	auto iter( std::begin( sessions ) );
	while ( iter++ != std::end( sessions ) )
	{
		Global::Memory::SafeDelete( ( *iter ) );
	}

	sessions.clear();
}

#pragma region Full Management
void SessionManager::Push( Session* _session )
{
	std::lock_guard<std::mutex> lock( mtx );
	if ( _session == nullptr )
		 return;

	_session->serial = Global::GetNewSerial();
	Debug.Log( "Register a new session ( ", _session->GetPort(), " ", _session->GetAddress(), " )" );
	sessions.push_back( _session );
}

void SessionManager::Erase( Session* _session )
{
	std::lock_guard<std::mutex> lock( mtx );
	if ( _session == nullptr )
		 return;
	
	Debug.Log( "The session has left ( ", _session->GetPort(), " ", _session->GetAddress(), " )" );
	if ( _session->stage != nullptr )
	{
		Stage* stage = _session->stage;
		stage->Exit( _session );

		if ( stage->IsExist() )
		{
			BroadcastWaitingRoom( _session, UPacket( UPDATE_STAGE_INFO, stage->info ) );
		}
		else
		{
			BroadcastWaitingRoom( _session, UPacket( DELETE_STAGE_INFO, stage->info ) );
			StageManager::Inst().Erase( stage );
		}
	}

	for ( std::list<Session*>::const_iterator iter = sessions.begin(); iter != sessions.end(); iter++ )
	{
		if ( ( *iter )->GetSocket() == _session->GetSocket() )
		{
			sessions.erase( iter );
			break;
		}
	}

	_session->CloseSocket();
	Global::Memory::SafeDelete( _session );
}

void SessionManager::Broadcast( const UPacket& _packet ) const
{
	for ( const auto& session : sessions )
		  session->Send( _packet );
}

void SessionManager::BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const
{
	for ( const auto& session : sessions )
	{
		if ( session->GetSocket() != _session->GetSocket() )
			 session->Send( _packet );
	}
}

void SessionManager::BroadcastWaitingRoom( const UPacket& _packet )
{
	for ( const auto& session : sessions )
	{
		if ( session->stage == nullptr )
			 session->Send( _packet );
	}
}

void SessionManager::BroadcastWaitingRoom( Session* _session, const UPacket& _packet )
{
	for ( const auto& session : sessions )
	{
		if ( session->stage == nullptr && session->GetSocket() != _session->GetSocket() )
			 session->Send( _packet );
	}
}

const std::list<Session*>& SessionManager::GetSessions() const
{
	return sessions;
}
#pragma endregion
#include "Stage.h"
#include "Management/SessionManager.h"

Stage::Stage( Session* _host, const STAGE_INFO& _info ) : host( _host ), info( _info ), isValid( true )
{
	Debug.Log( "Stage ", info.stageSerial, " has been created" );
	Debug.Log( "< ", _host->loginInfo.nickname, " > has entered Stage ", info.stageSerial );
	Debug.Log( "Host changed to < ", host->loginInfo.nickname, " > on stage ", info.stageSerial );

	sessions.push_back( host );
	_host->stage = this;
}

void Stage::Entry( Session* _session )
{
	if ( !isValid )
	{
		Debug.LogWarning( "The stage after the game" );
		throw Result::ERR_UNABLE_PROCESS;
	}

	if ( sessions.size() + 1 > info.personnel.maximum )
	{
		Debug.LogWarning( "The stage is full of people" );
		throw Result::ERR_UNABLE_PROCESS;
	}

	_session->stage = this;
	sessions.push_back( _session );
	info.personnel.current = ( int )sessions.size();
	Debug.Log( "< ", _session->loginInfo.nickname, " > has entered Stage ", info.stageSerial );
}

void Stage::Exit( Session* _session )
{
	if ( sessions.size() <= 0 )
	{
		Debug.LogWarning( "There's no one in the stage" );
		throw Result::ERR_UNABLE_PROCESS;
	}

	sessions.erase( std::find( sessions.begin(), sessions.end(), _session ) );
	info.personnel.current = ( int )sessions.size();
	Debug.Log( "The < ", _session->loginInfo.nickname, " > has left Stage ", info.stageSerial );

	if ( sessions.size() > 0 && host->GetSocket() == _session->GetSocket() )
	{
		 host = *sessions.begin();
		 info.hostSerial = host->serial;
		 Debug.Log( "Host changed to < ", host->loginInfo.nickname, " > on stage ", info.stageSerial );

		 SERIAL_INFO protocol;
		 protocol.serial = info.hostSerial;
		 _session->stage->BroadcastWithoutSelf( _session, UPacket( CHANGE_HOST_ACK, protocol ) );
	}

	_session->stage = nullptr;
}

void Stage::Clear()
{
	auto iter = sessions.begin();
	while ( iter != sessions.end() )
	{
		Exit( *iter );
		iter = sessions.begin();
	}
}

std::list<Session*>& Stage::GetSessions()
{
	return sessions;
}

void Stage::Broadcast( const UPacket& _packet ) const
{
	for ( auto iter = sessions.begin(); iter != sessions.end(); iter++ )
		( *iter )->Send( _packet );
}

void Stage::BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const
{
	for ( Session* session : sessions )
	{
		if ( session->GetSocket() != _session->GetSocket() )
			 session->Send( _packet );
	}
}

void Stage::Send( SOCKET _socket, const UPacket& _packet ) const
{
	for ( Session* session : sessions )
	{
		if ( session->GetSocket() == _socket )
		{
			session->Send( _packet );
			return;
		}
	}
}

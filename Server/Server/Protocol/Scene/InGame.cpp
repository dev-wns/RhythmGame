#include "InGame.h"
#include "Management/SessionManager.h"
#include "Management/StageManager.h"
#include "Database/Database.h"

void InGame::Bind()
{
	ProtocolSystem::Inst().Regist( PACKET_CHAT_MSG,	AckChatMessage );
	ProtocolSystem::Inst().Regist( EXIT_STAGE_REQ,	AckExitStage );
}

void InGame::AckChatMessage( const Packet& _packet )
{
	if ( _packet.session->stage == nullptr )
		 return;

	_packet.session->stage->Broadcast( _packet );
}

void InGame::AckExitStage( const Packet& _packet )
{
	try
	{
		const STAGE_INFO& data  = FromJson<STAGE_INFO>( _packet );
		Session* session        = _packet.session;
		Stage* stage            = StageManager::Inst().Find( data.stageSerial );

		stage->Exit( session );
		if ( stage->IsExist() )
		{
			SessionManager::Inst().BroadcastWaitingRoom( session, UPacket( UPDATE_STAGE_INFO, stage->info ) );
		}
		else
		{
			SessionManager::Inst().BroadcastWaitingRoom( session, UPacket( DELETE_STAGE_INFO, stage->info ) );
			StageManager::Inst().Erase( stage );
		}

		session->Send( UPacket( EXIT_STAGE_ACK ) );
	}
	catch ( Result )
	{

	}
}
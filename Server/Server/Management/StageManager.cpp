#include "StageManager.h"
#include "SessionManager.h"

void StageManager::Push( Stage* _stage )
{
	if ( _stage == nullptr )
		 return;

	std::lock_guard<std::mutex> lock( mtx );
	stages.push_back( _stage );
}

void StageManager::Erase( Stage* _stage )
{
	if ( _stage == nullptr )
		 return;

	Debug.Log( "Stage ", _stage->info.stageSerial, " has been removed" );

	std::lock_guard<std::mutex> lock( mtx );
	for ( std::list<Stage*>::const_iterator iter = stages.begin(); iter != stages.end(); iter++ )
	{
		Stage* stage = *iter;
		if ( stage->info.stageSerial == _stage->info.stageSerial )
		{
			stage->Clear();
			stages.erase( iter );
			break;
		}
	}

	Global::Memory::SafeDelete( _stage );
}

const std::list<Stage*>& StageManager::GetStages() const
{
	return stages;
}

bool StageManager::Contains( SerialType _serial ) const
{
	for ( Stage* stage : stages )
	{
		if ( stage->info.stageSerial == _serial )
			return true;
	}

	return false;
}

Stage* StageManager::Find( SerialType _serial ) const
{
	for ( Stage* stage : stages )
	{
		if ( stage->info.stageSerial == _serial )
		 	 return stage;
	}

	Debug.LogWarning( "The Stage ", _serial, " does not exist" );
	throw Result::ERR_NOT_EXIST_DATA;
}
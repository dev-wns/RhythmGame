#include "Login.h"
#include "Management/SessionManager.h"
#include <Database/Database.h>

void Login::Bind()
{
	ProtocolSystem::Inst().Regist( CONFIRM_LOGIN_REQ,   ConfirmMatchData );
	ProtocolSystem::Inst().Regist( CONFIRM_ACCOUNT_REQ, CreateNewUserData );
}

void Login::ConfirmMatchData( const Packet& _packet )
{
	USER_INFO data = FromJson<USER_INFO>( _packet );
	Debug.Log( "================ LOGIN ================" );
	Session* session = _packet.session;
	try
	{
		if ( Global::String::Trim( data.name ).empty() )
		{
			Debug.LogWarning( "The email is empty" );
			throw Result::ERR_INVALID_DATA;
		}

		USER_INFO info = Database::Inst().GetUserInfo( data.name );
		if ( data.name.compare( info.name ) != 0 || data.password.compare( info.password ) != 0 )
		{
			Debug.LogWarning( "User information does not match" );
			throw Result::ERR_INVALID_DATA;
		}

		session->userInfo = info;
		session->Send( UPacket( CONFIRM_LOGIN_ACK, info ) );

		Debug.Log( "< ", info.name, " > login completed" );
	}
	catch ( Result _error )
	{
		_packet.session->Send( UPacket( _error, CONFIRM_LOGIN_ACK ) );
	}
}

void Login::CreateNewUserData( const Packet& _packet )
{
	Debug.Log( "=============== ACCOUNT ===============" );
	try
	{
		USER_DATA data = FromJson<USER_DATA>( _packet );
		if ( Database::Inst().IsExist( data ) )
		{
			Debug.LogWarning( "User information exist" );
			throw Result::DB_ERR_DUPLICATE_DATA;
		}

		if ( Global::String::Trim( data.name ).empty() )
		{
			Debug.LogWarning( "The name is empty" );
			throw Result::ERR_INVALID_DATA;
		}

		Database::Inst().AddUser( data );
		Debug.Log( "Account creation completed" );
		_packet.session->Send( UPacket( CONFIRM_ACCOUNT_ACK ) );
	}
	catch ( Result _error )
	{
		_packet.session->Send( UPacket( _error, CONFIRM_ACCOUNT_ACK ) );
	}
}
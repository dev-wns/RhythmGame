#pragma once
#include "Global/Singleton.hpp"
#include "mysql.h"
#include "Protocol/Protocol.hpp"
#include "Global/LogText.hpp"

#define DATABASE Database::Inst()
class Database : public Singleton<Database>
{
private:
	MYSQL driver;
	MYSQL* conn;
	MYSQL_RES* result;

public:
	bool Initialize();
	void Query( const char* query, ... );

public:
	bool ExistLoginData( const LOGIN_DATA& _data );

	void CreateUserData( const std::string& _nickname, const std::string& _email, const std::string& _password );
	void DeleteUserData( int _uid );
	void UpdateUserData( int _uid, const USER_DATA& _data );

	// Getter
	LOGIN_DATA GetLoginData( const std::string& _email );
	LOGIN_DATA GetLoginData( int _uid );
	USER_DATA  GetUserData( int _uid );

public:
	Database() = default;
	virtual ~Database();
};
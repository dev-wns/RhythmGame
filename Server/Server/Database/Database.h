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
	bool IsExist( const USER_DATA& _data );

	void AddUser( const USER_DATA& _data );
	void DeleteUser( const USER_DATA& _data );
	void UpdateUser( const USER_DATA& _data );

	USER_DATA GetUserInfo( const std::string& _name );

public:
	Database() = default;
	virtual ~Database();
};
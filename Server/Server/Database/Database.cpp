#include "Database.h"

Database::~Database()
{
	::mysql_close( conn );
	Global::Memory::SafeDelete( conn );

	::mysql_free_result( result );
	Global::Memory::SafeDelete( result );
}

bool Database::Initialize()
{
	if ( ::mysql_init( &driver ) == nullptr )
	 	 return false;

	conn = ::mysql_real_connect( &driver, Global::DB::Host,     Global::DB::User,
									      Global::DB::Password, Global::DB::Schema, 3306, ( const char* )NULL, 0 );

	if ( conn == nullptr || ( ::mysql_select_db( conn, Global::DB::Schema ) != NULL ) )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		Global::Memory::SafeDelete( conn );
		return false;
	}

	std::cout << "Connected MySQL Server( " << conn->host << ":" << conn->port << " " << 
		                                       conn->user << " " << "****" << " " << conn->db << " )" << std::endl;

	return true;
}

void Database::Query( const char* _sentence, ... )
{
	if ( conn == nullptr )
		 throw Result::DB_ERR_DISCONNECTED;

	char sentence[1024] { 0, };
	size_t pos = 0;

	va_list vl;
	va_start( vl, _sentence );
	for ( int i = 0; _sentence[i] != '\0'; i++ )
	{
		if ( _sentence[i] == '%' )
		{
			switch ( _sentence[++i] )
			{
				default: 
				{
					sentence[pos]     = '%';
					sentence[pos + 1] = _sentence[i];
					pos += 2;
				} break;

				case 'd': 
				{
					std::string str = std::to_string( va_arg( vl, int ) );
					size_t size     = str.size();

					std::copy( &str[0], &str[size], &sentence[pos] );
					pos += size;
				} break;

				case 'f': 
				{
					std::string str = std::to_string( va_arg( vl, double ) );
					size_t size = str.size();

					std::copy( &str[0], &str[size], &sentence[pos] );
					pos += size;
				}
				break;

				case 'c': 
				{
					char* str = va_arg( vl, char* );
					size_t size = ::strlen( str );

					std::copy( &str[0], &str[size], &sentence[pos] );
					pos += size;
				}
				break;

				case 's':
				{
					std::string str = va_arg( vl, std::string );
					size_t size = str.size();
					
					std::copy( &str[0], &str[size], &sentence[pos] );
					pos += size;
				}
				break;
			}
		}
		else
		{
			sentence[pos] = _sentence[i];
			pos += 1;
		}
	}
	va_end( vl );

	Debug.Log( "Query <", sentence, ">" );
	if ( ::mysql_query( conn, sentence ) != NULL )
	{
		Debug.LogWarning( "DB Exception < ", ::mysql_error( conn ), " >" );
		throw Result::DB_ERR_INVALID_QUERY;
	}
}

bool Database::IsExist( const USER_DATA& _data )
{
	Query( R"Q( SELECT * FROM UserInfo WHERE UserName = '%s'; )Q", _data.name );
	//if ( ( result = ::mysql_store_result( conn ) ) == nullptr )
	//	   Debug.LogWarning( "DB Exception < ", ::mysql_error( conn ), " >" );

	return ::mysql_fetch_row( ::mysql_store_result( conn ) ) != nullptr;
}

void Database::AddUser( const USER_DATA& _data )
{
	Query( R"Q( Call AddUser( '%s', '%s' ); )Q", _data.name, _data.password );
}

void Database::DeleteUser( const USER_DATA& _data )
{
	Query( R"Q( Call DeleteUser( '%s' ); )Q", _data.name );
}

void Database::UpdateUser( const USER_DATA& _data )
{
	Query( R"Q( Call UpdateUser( '%s', %d, %f, %f, %d ); )Q",
							  _data.name, _data.level, _data.exp, _data.accuracy, _data.playCount );
}

USER_DATA Database::GetUserInfo( const std::string& _name )
{
	Query( R"Q( SELECT * FROM UserInfo WHERE UserName = '%s'; )Q", _name );

	if ( ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		throw Result::ERR_NOT_EXIST_DATA;
	}

	MYSQL_ROW row;
	if ( ( row = ::mysql_fetch_row( result ) ) == nullptr )
	{
		Debug.LogWarning( "User information does not exist" );
		throw Result::ERR_NOT_EXIST_DATA;
	}

	return USER_DATA{ row[0], row[1], ::atoi( row[2] ), ( float )::atof( row[3] ), ( float )::atof( row[4] ), ::atoi(row[5]) };
}
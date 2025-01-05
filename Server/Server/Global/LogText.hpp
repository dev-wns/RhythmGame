#pragma once
#include "Singleton.hpp"
#include "Synchronize/CriticalSection.h"

#define Debug ( LogText::Inst() << __FUNCTION__ << "( " << std::to_string( __LINE__ ) << " )" )
enum class LogAlignment { All, IgnoreLog, OnlyError, };
enum class LogWriteType { All, Console,   File, };
class LogText : public Singleton<LogText>
{
public:
	CriticalSection cs;
	LogAlignment alignment = LogAlignment::All;
	LogWriteType writeType = LogWriteType::All;
	bool ignoreData        = false;

private:
	enum LogType : short { _Log, _Warning, _Error, };
	static const u_short MaxLogSize = 1024;
	std::ofstream os;

	size_t pos;
	char   data[MaxLogSize];

	size_t infoPos;
	char   info[MaxLogSize];

public:
	LogText()
	{
		::memset( data, 0, MaxLogSize );
		::memset( info, 0, MaxLogSize );
		pos = infoPos = 0;

		char date[21] = { 0, };
		const std::time_t now = std::chrono::system_clock::to_time_t( std::chrono::system_clock::now() );
		std::strftime( &date[0], 21, "%Y-%m-%d_%H-%M-%S", std::localtime( &now ) );

		std::string path;
		path.append( "../Log/" ).append( date ).append( ".txt" );
		os.open( path, std::ios::out | std::ios::trunc );
		if ( !os.is_open() )
			 std::cout << "File open failed" << std::endl;
	}
	~LogText()
	{
		os.close();
	}

private:
	void Clear()
	{
		::memset( data, 0, MaxLogSize );
		pos = 0;
	}

	void BeginWrite( LogType _type )
	{
		if ( infoPos == 0 || !os.is_open() )
		{
			::memset( info, 0, MaxLogSize );
			infoPos = 0;
			return;
		}

		switch ( _type )
		{
			case LogType::_Log:
			{
				os << "# Log < " << info << " > #" << std::endl;
			}
			break;

			case LogType::_Warning:
			{
				os << "### Warning < " << info << " > ###" << std::endl;
			} break;

			case LogType::_Error:
			{
				os << "##### Error < " << info << " > #####" << std::endl;
			} break;
		}

		::memset( info, 0, MaxLogSize );
		infoPos = 0;
	}

	void Write( LogType _type )
	{
		if ( pos == 0 || !os.is_open() )
		{
			Clear();
			return;
		}

		if ( writeType != LogWriteType::Console ) 
			 os << data << std::endl << std::endl;

		if ( writeType != LogWriteType::File )
		{
			switch ( _type )
			{
				case LogType::_Log:
				{
					std::cout << "#   LOG   # " << data << std::endl;
				} break;

				case LogType::_Warning:
				{
					std::cout << "# WARNING # " << data << std::endl;
				} break;

				case LogType::_Error:
				{
					std::cout << "#  ERROR  # " << data << std::endl;
				} break;
			}
		}

		Clear();
	}

	void WriteAfterOverflowCheck( const std::string& _str )
	{
		size_t size = _str.size();
		if ( pos + size >= MaxLogSize )
		{
			if ( writeType != LogWriteType::Console ) os        << data;
			if ( writeType != LogWriteType::File )    std::cout << data;
			Clear();

			size_t startPos = 0;
			size_t endPos   = 0;
			size_t amount   = size; // ³²Àº ¾ç
			do
			{
				endPos += ( amount > MaxLogSize ? MaxLogSize : amount ) - 1;
				std::copy( &_str[startPos], &_str[endPos], &data[0] );
				
				data[MaxLogSize - 1] = '\0';
				if ( writeType != LogWriteType::Console ) os        << data;
				if ( writeType != LogWriteType::File )    std::cout << data;
				Clear();

				amount   -= ( endPos - startPos );
				startPos += ( endPos - startPos );
			}
			while ( amount >= MaxLogSize );

			std::copy( &_str[startPos], &_str[size], &data[0] );
			pos = amount;
		}
		else
		{
			std::copy( &_str[0], &_str[size], &data[pos] );
			pos += size;
		}
	}

public:
	void Log()        { }
	void LogWarning() { }
	void LogError()   { }

	template<typename T, typename... Args>
	void Log( T type, Args... _args )
	{
		cs.Lock();
		if ( alignment != LogAlignment::All )
			 return;

		BeginWrite( LogType::_Log );

		Copy( type );
		Log( _args... );

		Write( LogType::_Log );
		cs.UnLock();
	}

	template<typename T, typename... Args>
	void LogWarning( T type, Args... _args )
	{
		cs.Lock();
		if ( alignment == LogAlignment::OnlyError )
			 return;

		BeginWrite( LogType::_Warning );

		Copy( type );
		LogWarning( _args... );

		Write( LogType::_Warning );
		cs.UnLock();
	}

	template<typename T, typename... Args>
	void LogError( T type, Args... _args )
	{
		cs.Lock();
		BeginWrite( LogType::_Error );

		Copy( type );
		LogError( _args... );

		Write( LogType::_Error );
		cs.UnLock();
	}

	std::string ToString( float _value, int _maxDecimalPoint = 3 )
	{
		std::ostringstream out;
		out << std::setprecision( _maxDecimalPoint ) << _value;

		return out.str();
	}
	LogText& operator << ( const std::string& _arg )
	{
		std::copy( std::begin( _arg ), std::end( _arg ), &info[infoPos] );
		infoPos += _arg.size();

		return *this;
	}
	LogText& operator << ( const char* _arg )
	{
		if ( _arg != nullptr )
		{
			size_t size = ::strlen( _arg );
			std::copy( &_arg[0], &_arg[size], &info[infoPos] );
			infoPos += size;
		}
	
		return *this;
	}
	void Copy( int                  _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( unsigned int         _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( short                _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( unsigned short       _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( long                 _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( unsigned long        _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( long long            _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( unsigned long long   _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( float                _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( double               _arg )
	{
		WriteAfterOverflowCheck( std::to_string( _arg ) );
	}
	void Copy( const char*          _arg )
	{
		if ( _arg != nullptr )
			 WriteAfterOverflowCheck( _arg );
	}
	void Copy( const unsigned char* _arg )
	{
		if ( _arg != nullptr )
			 WriteAfterOverflowCheck( ( char* )_arg );
	}
	void Copy( const std::string&   _arg )
	{
		WriteAfterOverflowCheck( _arg );
	}
	void Copy( const Vector2&       _arg )
	{
		std::string str;
		str.reserve( 128 );
		str.append( "( " );
		str.append( ToString( _arg.x ) ).append( ", " );
		str.append( ToString( _arg.y ) ).append( " )" );

		WriteAfterOverflowCheck( str );
	}
	void Copy( const Vector3&       _arg )
	{
		std::string str;
		str.reserve( 128 );
		str.append( "( " );
		str.append( ToString( _arg.x ) ).append( ", " );
		str.append( ToString( _arg.y ) ).append( ", " );
		str.append( ToString( _arg.z ) ).append( " )" );

		WriteAfterOverflowCheck( str );
	}
	void Copy( const Vector4&       _arg )
	{
		std::string str;
		str.reserve( 128 );
		str.append( "( " );
		str.append( ToString( _arg.x ) ).append( ", " );
		str.append( ToString( _arg.y ) ).append( ", " );
		str.append( ToString( _arg.z ) ).append( ", " );
		str.append( ToString( _arg.w ) ).append( " )" );

		WriteAfterOverflowCheck( str );
	}
};


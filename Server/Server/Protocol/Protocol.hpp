#pragma once
#include "Global/Header.h"
#include "magic_enum.hpp"

// 서버에서 패킷타입을 문자열로 출력하기위해
// 타입은 0부터 순서대로 지정되도록 합니다.

enum PacketType : u_short
{
	NONE = 0,
	PACKET_HEARTBEAT,              // 주기적인 통신을 위한 패킷
	PACKET_CHAT_MSG,               // 채팅 메세지

	// Login
	CONFIRM_LOGIN_REQ,             // 로그인 요청
	CONFIRM_LOGIN_ACK,             // 로그인 응답
	CONFIRM_ACCOUNT_REQ,           // 계정 생성 요청
	CONFIRM_ACCOUNT_ACK,           // 계정 생성 응답
	DUPLICATE_EMAIL_REQ,           // 이메일 중복확인 요청
	DUPLICATE_EMAIL_ACK,           // 이메일 중복확인 응답

	// Stage
	STAGE_INFO_REQ,                // 방 정보 요청
	STAGE_INFO_ACK,                // 방 정보 응답
	CREATE_STAGE_REQ,              // 방 생성 요청
	CREATE_STAGE_ACK,              // 방 생성 응답
	UPDATE_STAGE_INFO,             // 방 정보가 갱신됨
	INSERT_STAGE_INFO,             // 방 정보가 추가됨
	DELETE_STAGE_INFO,             // 방 정보가 삭제됨
	ENTRY_STAGE_REQ,               // 방 입장 요청
	ENTRY_STAGE_ACK,               // 방 입장 응답
	EXIT_STAGE_REQ,                // 방 퇴장 요청
	EXIT_STAGE_ACK,                // 방 퇴장 응답
	CHANGE_HOST_ACK,               // 호스트 변경 응답

	INIT_SCENE_ACTORS_REQ,         // 씬에 배치된 Actor들 초기화 요청
	INIT_SCENE_ACTORS_ACK,         // 씬에 배치된 Actor들 초기화 응답
	INGAME_LOAD_DATA_REQ,          // InGame 입장시 데이터 요청
	GAME_OVER_ACK,                 // 게임 종료 응답
	UPDATE_RESULT_INFO_REQ,        // 게임 결과 갱신 요청
	UPDATE_RESULT_INFO_ACK,        // 게임 결과 갱신 응답
};

typedef struct SingleString
{
public:
	std::string message = "";

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( message ) );
	}
} MESSAGE;
typedef struct SingleBoolean
{
public:
	bool isCompleted;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( isCompleted ) );
	}
} CHECK;
typedef struct SingleSerialType
{
public:
	SerialType serial;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
	}
} SERIAL_INFO;
typedef struct SerialsType
{
public:
	std::vector<SerialType> serials;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serials ) );
	}
} SERIALS_INFO;
typedef struct SerialIntType
{
public:
	SerialType serial;
	int index;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( index ) );
	}
} INDEX_INFO;
typedef struct SerialFloatType
{
public:
	SerialType serial;
	float angle;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( angle ) );
	}
} LOOK_INFO;

typedef struct Vector2
{
public:
	float x, y;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( x ) );
		ar( CEREAL_NVP( y ) );
	}
} VECTOR2;
typedef struct Vector3
{
public:
	float x, y, z;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( x ) );
		ar( CEREAL_NVP( y ) );
		ar( CEREAL_NVP( z ) );
	}
} VECTOR3;
typedef struct Vector4
{
public:
	float x, y, z, w;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( x ) );
		ar( CEREAL_NVP( y ) );
		ar( CEREAL_NVP( z ) );
		ar( CEREAL_NVP( w ) );
	}
} Quaternion, QUATERNION;

#pragma region Database
typedef struct UserInfo
{
public:
	std::string name;
	std::string password;
	int level;
	float exp;
	float accuracy;
	int playCount;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( name ) );
		ar( CEREAL_NVP( password ) );
		ar( CEREAL_NVP( level ) );
		ar( CEREAL_NVP( exp ) );
		ar( CEREAL_NVP( accuracy ) );
		ar( CEREAL_NVP( playCount ) );
	}
} USER_INFO, UserData, USER_DATA;

typedef struct ResultInfo
{
	int uid;
	int kill, death;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( uid ) );
		ar( CEREAL_NVP( kill ) );
		ar( CEREAL_NVP( death ) );
	}
} RESULT_INFO, ResultData, RESULT_DATA;
#pragma endregion

#pragma region Lobby Infomation
struct Personnel
{
public:
	int current, maximum;

	Personnel() : current( 0 ), maximum( 0 ) { }
	Personnel( int _cur, int _max ) : current( _cur ), maximum( _max ) { }

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( current ) );
		ar( CEREAL_NVP( maximum ) );
	}
};
typedef struct StageInfo
{
public:
	SerialType stageSerial;
	SerialType hostSerial;
	std::string title;
	int targetKill;
	int currentKill;
	Personnel personnel;

public:
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( stageSerial ) );
		ar( CEREAL_NVP( hostSerial ) );
		ar( CEREAL_NVP( title ) );
		ar( CEREAL_NVP( targetKill ) );
		ar( CEREAL_NVP( currentKill ) );
		ar( CEREAL_NVP( personnel ) );
	}
} STAGE_INFO;
#pragma endregion

typedef struct ChatMessage
{
public:
	u_int serial;
	std::string nickname;
	std::string message;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( serial ) );
		ar( CEREAL_NVP( nickname ) );
		ar( CEREAL_NVP( message ) );
	}
} CHAT_MESSAGE;
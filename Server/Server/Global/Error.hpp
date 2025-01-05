#pragma once
enum class Result : unsigned short
{
    OK = 0,
    DB_ERR_DISCONNECTED,   // 서버에 연결되지않음
    DB_ERR_INVALID_QUERY,  // 쿼리 구문이 유효하지않음
    DB_ERR_DUPLICATE_DATA, // UNIQUE로 설정된 데이터가 이미 존재함

    ERR_NOT_EXIST_DATA,    // 데이터가 존재하지않음
    ERR_INVALID_DATA,      // 데이터가 유효하지않음
    ERR_UNABLE_PROCESS,    // 요청을 수행할 수 없음
};

// null이 포함된 데이터를 JSON으로 만들면 서버가 뻗습니다.
// string 같은 클래스는 꼭 초기화 해주세요.

// 서버에서 패킷타입을 문자열로 출력하기위해
// 타입은 0부터 순서대로 지정되도록 합니다.
public enum PacketType : ushort
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
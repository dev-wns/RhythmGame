#include "Acceptor.h"
#include "Management/SessionManager.h"
#include "Management/IOCP.h"

Acceptor::~Acceptor()
{
	std::cout << "Acceptor Destructor" << std::endl;
	::WSACleanup();
}

bool Acceptor::Accept( int _port, const char* _ip )
{
	WSADATA wsa;
	switch ( ::WSAStartup( MAKEWORD( 2, 2 ), &wsa ) )
	{
		case WSASYSNOTREADY:
			std::cout << "네트워크 통신에 대한 준비가 되지 않았습니다." << std::endl;
			break;

		case WSAVERNOTSUPPORTED:
			std::cout << "요청된 윈도우소켓 지원버전은 제공되지 않습니다." << std::endl;
			break;

		case WSAEINPROGRESS:
			std::cout << "윈도우소켓 1.1작업이 진행 중입니다." << std::endl;
			break;

		case WSAEPROCLIM:
			std::cout << "윈도우소켓 구현에서 지원하는 작업 수가 제한에 도달했습니다." << std::endl;
			break;

		case WSAEFAULT:
			std::cout << "WSAData가 유효하지 않습니다." << std::endl;
			break;
	}

	// INET : 인터넷 프로토콜
	// AF   : 주소 체계를 설정할 때 사용     ( Address Family )
	// PF   : 프로토콜 체계를 설정할 때 사용 ( Protocol Family )
	// 리눅스 매뉴얼에서 AF 통일 권장
	socket = ::socket( AF_INET/* IPv4 인터넷 프로토콜 */, SOCK_STREAM/* TCP 프로토콜 전송 방식 */, 0 );
	ZeroMemory( &address, sizeof( address ) );
	address.sin_family = AF_INET; // 소켓 생성 시와 통일

	// Little-Endian to Big-Endian
	// htons : host to network short
	// htonl : host to network long
	// 0x12345678 -> 0x12, 0x34, 0x56, 0x78 상위 비트부터 바이트 단위로 저장 ( 빅 엔디안 )

	// Big-Endian to Little-Endian
	// ntohs : network to host short
	// ntohl : network to host long
	// 0x12345678 -> 0x78, 0x56, 0x34, 0x12 하위 비트부터 바이트 단위로 저장 ( 리틀 엔디안 )
	if ( _ip == nullptr ) address.sin_addr.S_un.S_addr = ::htonl( INADDR_ANY );
	else                  address.sin_addr.S_un.S_addr = ::inet_addr( _ip );

	address.sin_port = ::htons( ( u_short )_port );

	std::cout << "Acceptor setup completed" << std::endl;
	Listen();

	return true;
}

bool Acceptor::Listen()
{
	if ( ::bind( socket, ( sockaddr* )&address, sizeof( address ) ) == SOCKET_ERROR ||
		 ::listen( socket, SOMAXCONN ) == SOCKET_ERROR )
	{
		CloseSocket();
		return false;
	}

	std::cout << "Create thread for listening" << std::endl;
	std::cout << "Waiting for a new session..." << std::endl;
	std::thread th( [&]() { Acceptor::WaitForClients(); } );
	th.detach();

	return true;
}

void Acceptor::WaitForClients() const
{
	SOCKET clientSocket;
	SOCKADDR_IN addr {};
	int size = sizeof( addr );
	while ( true )
	{
		clientSocket = ::accept( socket, ( sockaddr* )&addr, &size );

		Session* session = new Session( clientSocket, addr );
		IOCP::Inst().Bind( ( HANDLE )clientSocket, ( ULONG_PTR )session );
		SessionManager::Inst().Push( session );
		std::this_thread::sleep_for( std::chrono::milliseconds( 1 ) );
	}
}
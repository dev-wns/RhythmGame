#include "IOCP.h"
#include "SessionManager.h"

IOCP::IOCP() : handle( nullptr ) {}

bool IOCP::Initialize()
{
	std::cout << "Create threads for IOCP" << std::endl;
	std::cout << "Wait for data to be entered in the IO Completion Queue" << std::endl;
	handle = ::CreateIoCompletionPort( INVALID_HANDLE_VALUE, 0, 0, Global::WorkerThreadCount );
	for ( int i = 0; i < Global::WorkerThreadCount; i++ )
	{
		std::thread th( [&]() { IOCP::WaitCompletionStatus(); } );
		th.detach();
	}

	return true;
}

void IOCP::Bind( const HANDLE& _socket, const ULONG_PTR _key ) const
{
	::CreateIoCompletionPort( _socket, handle, _key, 0 );

	Session* session = ( Session* )_key;
	session->Recieve();
}

void IOCP::WaitCompletionStatus() const
{
	ULONG_PTR key;
	LPOVERLAPPED ov;
	DWORD transferred;
	
	while ( true )
	{
		//if ( ::WaitForSingleObject( Global::KillEvent, 0 ) == WAIT_OBJECT_0 )
		//	 break;

		if ( ::GetQueuedCompletionStatus( handle, &transferred, &key, &ov, INFINITE ) == TRUE )
		{
			Session* session = ( Session* )key;
			// 일반적으로 상대방 소켓이 끊어졌을 때 0byte를 read 한다.
			if ( transferred == 0 )
			{
				if ( session != nullptr )
					 SessionManager::Inst().Erase( session );
			}
			else
			{
				if ( session != nullptr )
				{
					if ( ov != NULL )
						 session->Dispatch( ov, transferred );
				}
			}
		}
		else
		{
			Session* session = ( Session* )key;
			switch ( DWORD error = ::GetLastError() )
			{
				default:
				{
					std::cout << "Queue LastError : " << error << std::endl;
					// 작업이 취소되었을 때 발생하는 오류
					if ( error != ERROR_OPERATION_ABORTED )
					{
						// 일반적으로 상대방 소켓이 끊어졌을 때 0byte를 read 한다.
						if ( session != nullptr && transferred == 0 )
							 SessionManager::Inst().Erase( session );
					}
				} break;

				// 서버에서 세션 강제 종료시켰을 때
				case ERROR_CONNECTION_ABORTED:
				{
					continue;
				} break;

				// 방화벽, 라우터, 랜뽑 등의 제한으로 인한 네트워크 단절이 발생했을 때
				case WAIT_TIMEOUT:
				{
					// 고쳐질 때까지 대기한다.
					continue;
				} break;

				// 상대방이 closesocket, shutdown을 호출하지않고 종료했을 때 0byte read가 발생하지 않는다.
				// 0byte read가 발생하지 않은 시점에서 Send/Recieve 작업을 시도했을 때
				// 상대방은 이미 종료한 상태로 해당 오류가 발생한다.
				case ERROR_NETNAME_DELETED:
				{
					if ( session != nullptr )
						 SessionManager::Inst().Erase( session );
				} break;
			}
		}
		std::this_thread::sleep_for( std::chrono::milliseconds( 1 ) );
	}
}
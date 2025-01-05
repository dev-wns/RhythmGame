#include "Session.h"
#include "Management/PacketSystem.h"

Session::Session( const SOCKET& _socket, const SOCKADDR_IN& _address )
				  : Network( _socket, _address ), packet( new Packet() ), 
	                buffer{}, startPos( 0 ), writePos( 0 ), readPos( 0 ),
					stage( nullptr ), serial( 0 ) { }

Session::~Session()
{
	::shutdown( socket, SD_SEND );
}

void Session::Dispatch( const LPOVERLAPPED& _ov, DWORD _size )
{
	OVERLAPPEDEX* overalapped = ( OVERLAPPEDEX* )_ov;
	if ( overalapped->flag == OVERLAPPEDEX::MODE_RECV )
	{
		// 버퍼에 여분이 없는 경우
		// 버퍼를 초기화하고 읽지않은 잔여 데이터를 가장 앞으로 이동
		if ( writePos + _size > MaxReceiveSize )
		{
			byte remain[MaxReceiveSize] = { 0, };
			::memcpy( remain, &buffer[startPos], readPos );

			ZeroMemory( buffer, MaxReceiveSize * sizeof( byte ) );
			::memcpy( buffer, remain, readPos );

			startPos = 0;
			writePos = readPos;
		}

		// 받은 데이터를 버퍼에 추가한다.
		::memcpy( &buffer[writePos], wsaRecvBuffer.buf, _size * sizeof( byte ) );
		writePos += _size; // 현재까지 버퍼에 쓰인 지점
		readPos  += _size; // 현재 읽지않은 데이터의 양

		// 읽어야하는 데이터가 최소 헤더( 6바이트 ) 이상은 되어야한다.
		if ( readPos < Global::HeaderSize )
		{
			ZeroMemory( &wsaRecvBuffer, sizeof( WSABUF ) );
			Recieve();
			return;
		}

		packet = ( UPacket* )&buffer[startPos];
		while ( readPos >= packet->size ) // 하나 이상의 패킷이 완성되었을 때
		{
			Packet newPacket;
			::memcpy( &newPacket, packet, packet->size );
			newPacket.session = this;

			PacketSystem::Inst().Push( newPacket );

			readPos  -= packet->size;
			startPos += packet->size; // 읽어야하는 시작 지점

			// 완성된 패킷이 더 있는지 확인
			if ( readPos < Global::HeaderSize ) break;
			packet = ( UPacket* )&buffer[startPos];

			if ( packet->size <= 0 ) break;
		}

		ZeroMemory( &wsaRecvBuffer, sizeof( WSABUF ) );
		Recieve();
	}
	else if ( overalapped->flag == OVERLAPPEDEX::MODE_SEND )
	{
		
	}

	Global::Memory::SafeDelete( overalapped );
}
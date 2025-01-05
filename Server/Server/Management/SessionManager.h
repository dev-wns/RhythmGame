#pragma once
#include "Network/Session.h"
#include "Global/LogText.hpp"
#include "Stage/Stage.h"

class SessionManager : public Singleton<SessionManager>
{
private:
	std::list<Session*> sessions;
	std::mutex mtx;

public:
	SessionManager() = default;
	virtual ~SessionManager();

public:
	void Push( Session* _session );
	void Erase( Session* _session );

	void Broadcast( const UPacket& _packet ) const;
	void BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const;
	void BroadcastWaitingRoom( const UPacket& _packet );
	void BroadcastWaitingRoom( Session* /* except Session */, const UPacket& _packet);
	const std::list<Session*>& GetSessions() const;
};
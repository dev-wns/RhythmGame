#pragma once
#include "Network/Session.h"


class Stage
{
public:
	STAGE_INFO info;
	Session* host;
	bool isValid;

private:
	std::list<Session*> sessions;

public:
	Stage( Session* _host, const STAGE_INFO& _info );
	virtual ~Stage() = default;

	std::list<Session*>& GetSessions();

	inline bool IsExist() { return sessions.size() > 0; }
	void Entry( Session* _session );
	void Exit( Session* _session );

	void Clear();

	void Broadcast( const UPacket& _packet ) const;
	void BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const;
	void Send( SOCKET _socket, const UPacket& _packet ) const;
};
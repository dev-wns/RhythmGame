#pragma once
#include "Global/Singleton.hpp"
#include "Packet/Packet.hpp"

class PacketSystem : public Singleton<PacketSystem>
{
private:
	std::queue<Packet> packets;
	std::condition_variable cv;
	std::mutex mtx;

public:
	PacketSystem()          = default;
	virtual ~PacketSystem() = default;

public:
	bool Initialize();
	void Push( const Packet& _packet );

private:
	void Process();
};
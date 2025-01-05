#pragma once
#include "Management/ProtocolSystem.h"
#include "Stage/Stage.h"

class Lobby : public IScene
{
public:
	Lobby()          = default;
	virtual ~Lobby() = default;

private:
	static void AckCreateStage( const Packet& _packet );
	static void AckStageList( const Packet& _packet );
	static void AckEntryStage( const Packet& _packet );

public:
	virtual void Bind() override;
};
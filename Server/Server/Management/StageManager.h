#pragma once
#include "Stage/Stage.h"

class StageManager : public Singleton<StageManager>
{
private:
	std::list<Stage*> stages;
	std::mutex mtx;

public:
	StageManager()          = default;
	virtual ~StageManager() = default;

	void Push(  Stage* _stage );
	void Erase( Stage* _stage );

	bool Contains( SerialType _serial ) const;
	Stage* Find( SerialType _serial ) const;

	const std::list<Stage*>& GetStages() const;
};
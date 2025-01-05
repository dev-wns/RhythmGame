#pragma once
#include "Global/Header.h"

class CriticalSection
{
private:
	CRITICAL_SECTION cs;

public:
	CriticalSection();
	virtual ~CriticalSection();

public:
	void Lock();
	void UnLock();
};
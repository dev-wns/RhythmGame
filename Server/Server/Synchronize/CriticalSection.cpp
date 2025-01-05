#include "CriticalSection.h"

CriticalSection::CriticalSection()
{
	::InitializeCriticalSection( &cs );
}

CriticalSection::~CriticalSection()
{
	::DeleteCriticalSection( &cs );
}

void CriticalSection::Lock()
{
	::EnterCriticalSection( &cs );
}

void CriticalSection::UnLock()
{
	::LeaveCriticalSection( &cs );
}
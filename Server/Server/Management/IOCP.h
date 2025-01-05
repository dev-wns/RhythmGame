#pragma once
#include "Global/Singleton.hpp"

class IOCP : public Singleton<IOCP>
{
private:
	HANDLE handle;

public:
	IOCP();
	virtual ~IOCP() = default;

public:
	bool Initialize();
	void Bind( const HANDLE& _sock, const ULONG_PTR _key ) const;

private:
	void WaitCompletionStatus() const;
};
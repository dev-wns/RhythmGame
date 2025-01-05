#pragma once
// Warning Ignore
#define _CRT_SECURE_NO_WARNINGS
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#pragma warning( disable : 4505 )
#pragma warning( disable : 26819 )
#pragma warning( disable : 26495 )

// Windows
#include <winsock2.h>
#include <WS2tcpip.h>
#include <windows.h>
#include <process.h>

// Character
#include <string>
#include <tchar.h>
#include <fstream>
#include <sstream>

// Data Structure
#include <vector>
#include <list>
#include <queue>
#include <map>

// Standard
#include <stdlib.h>
#include <iostream>
#include <cstdio>
#include <random>

// Thread
#include <thread>
#include <mutex>
#include <future>
#include <type_traits>

// Function
#include <condition_variable>
#include <functional>

// Time
#include <chrono>
#include <iomanip>
#include <ctime>

// Serialize, Deserialize
#include <cereal/cereal.hpp>
#include <cereal/archives/json.hpp>
#include <cereal/types/array.hpp>
#include <cereal/types/atomic.hpp>
#include <cereal/types/valarray.hpp>
#include <cereal/types/vector.hpp>
#include <cereal/types/deque.hpp>
#include <cereal/types/forward_list.hpp>
#include <cereal/types/list.hpp>
#include <cereal/types/string.hpp>
#include <cereal/types/map.hpp>
#include <cereal/types/queue.hpp>
#include <cereal/types/set.hpp>
#include <cereal/types/stack.hpp>
#include <cereal/types/unordered_map.hpp>
#include <cereal/types/unordered_set.hpp>
#include <cereal/types/utility.hpp>
#include <cereal/types/tuple.hpp>
#include <cereal/types/bitset.hpp>
#include <cereal/types/complex.hpp>
#include <cereal/types/chrono.hpp>
#include <cereal/types/polymorphic.hpp>

// Global Variables and Function
#include "Global.hpp"
#include "Error.hpp"

#pragma comment( lib, "ws2_32.lib" )
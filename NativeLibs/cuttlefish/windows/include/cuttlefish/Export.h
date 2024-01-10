#pragma once
#ifdef CUTTLEFISH_BUILD
#define CUTTLEFISH_EXPORT __declspec(dllexport)
#else
#define CUTTLEFISH_EXPORT __declspec(dllimport)
#endif

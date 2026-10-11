// Copyright (c) WAR WW3 project. All rights reserved.

#include "WARWW3GameMode.h"
#include "Modules/ModuleManager.h"
#include "Core/WARWW3Core.h"

DEFINE_LOG_CATEGORY_STATIC(LogWarWW3Module, Log, All);

class FWARWW3Module : public IModuleInterface
{
public:
	virtual void StartupModule() override
	{
		UE_LOG(LogWarWW3Module, Log, TEXT("WARWW3 module started"));
	}

	virtual void ShutdownModule() override
	{
		UE_LOG(LogWarWW3Module, Log, TEXT("WARWW3 module shut down"));
	}
};

IMPLEMENT_PRIMARY_GAME_MODULE(FWARWW3Module, WARWW3, "WARWW3");

// Copyright (c) WAR WW3 project. All rights reserved.

#include "WARWW3GameInstance.h"
#include "UI/WARWW3GameSettings.h"
#include "Core/WARWW3Core.h"

UWARWW3GameInstance::UWARWW3GameInstance()
{
	GameSettings = CreateDefaultSubobject<UWARWW3GameSettings>(TEXT("GameSettings"));
}

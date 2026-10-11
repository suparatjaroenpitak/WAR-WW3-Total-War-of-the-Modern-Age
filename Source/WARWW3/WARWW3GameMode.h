// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "WARWW3GameMode.generated.h"

/**
 * Base rules for the current WAR WW3 game mode.
 * Subclasses exist for Campaign (turn-based) and Battle (real-time).
 */
UCLASS()
class WARWW3_API AWARWW3GameMode : public AGameModeBase
{
	GENERATED_BODY()

public:
	AWARWW3GameMode();
};

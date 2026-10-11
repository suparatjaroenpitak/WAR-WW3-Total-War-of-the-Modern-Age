// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "Engine/GameInstance.h"
#include "WARWW3GameInstance.generated.h"

class UWARWW3GameSettings;

/**
 * Application-wide initialization: owns game settings, coordinates session start
 * (main menu -> campaign -> battle), and is the single place that stores the active
 * campaign controller reference for cross-level access.
 */
UCLASS()
class WARWW3_API UWARWW3GameInstance : public UGameInstance
{
	GENERATED_BODY()

public:
	UWARWW3GameInstance();

	UFUNCTION(BlueprintPure, Category = "WARWW3|Settings")
	UWARWW3GameSettings* GetGameSettings() const { return GameSettings; }

private:
	UPROPERTY()
	TObjectPtr<UWARWW3GameSettings> GameSettings;
};

// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "Engine/GameInstance.h"
#include "WARWW3GameSettings.generated.h"

/** Supported UI languages; maps to Locale asset + .locres files. */
UENUM(BlueprintType)
enum class EGameLanguage : uint8
{
	English,
	Thai
};

/**
 * Application-wide localization / settings state.
 * Attached to the GameInstance so language persists across level loads.
 */
UCLASS(Config = Game)
class WARWW3_API UWARWW3GameSettings : public UObject
{
	GENERATED_BODY()

public:
	/** Switch the live UI language and persist the choice in the user's GameUserSettings. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Localization")
	void SetLanguage(EGameLanguage NewLanguage, class UGameInstance* OwningInstance);

	/** Current UI language. */
	UFUNCTION(BlueprintPure, Category = "WARWW3|Localization")
	EGameLanguage GetLanguage() const { return CurrentLanguage; }

private:
	UPROPERTY(Config, SaveGame)
	EGameLanguage CurrentLanguage = EGameLanguage::English;
};

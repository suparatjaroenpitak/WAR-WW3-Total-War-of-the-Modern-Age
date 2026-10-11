// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "Campaign/CampaignTypes.h"
#include "WARWW3CampaignSaveGame.generated.h"

/** Schema version: bump when fields change and provide migration in USavasCampaignState::LoadCampaign. */
constexpr int32 WARWW3_SAVE_SCHEMA_VERSION = 1;

UCLASS()
class WARWW3_API UWARWW3CampaignSaveGame : public USaveGame
{
	GENERATED_BODY()

public:
	UPROPERTY(SaveGame)
	int32 SchemaVersion = WARWW3_SAVE_SCHEMA_VERSION;

	UPROPERTY(SaveGame)
	int32 CampaignTurn = 0;

	UPROPERTY(SaveGame)
	FName PlayerCountryId;

	UPROPERTY(SaveGame)
	TArray<FProvince> Provinces;

	UPROPERTY(SaveGame)
	TArray<FCountry> Countries;

	UPROPERTY(SaveGame)
	float PlayerTreasury = 0.f;

	/** Human-readable timestamp written at save time. */
	UPROPERTY(SaveGame)
	FDateTime SavedAtUtc;
};

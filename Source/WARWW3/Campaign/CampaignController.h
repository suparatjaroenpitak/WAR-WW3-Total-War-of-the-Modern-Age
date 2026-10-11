// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "Campaign/CampaignTypes.h"
#include "Campaign/TurnPipeline.h"
#include "CampaignController.generated.h"

class UWARWW3CampaignSaveGame;

/**
 * Authoritative campaign controller (server + single-player authority).
 * Guards End Turn so a repeated click cannot double-collect, and owns all save/load.
 */
UCLASS()
class WARWW3_API UCampaignController : public UObject
{
	GENERATED_BODY()

public:
	/** Set up a new campaign for the given player country. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Campaign")
	void StartNewCampaign(FName PlayerCountryId, const TArray<FCountry>& Countries, const TArray<FProvince>& Provinces);

	/** Returns false when a turn is already resolving (prevents the End Turn spam bug). */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Campaign")
	bool EndTurn(TArray<FTurnStepReport>& OutReports);

	UFUNCTION(BlueprintPure, Category = "WARWW3|Campaign")
	bool IsTurnResolving() const { return bTurnResolving; }

	UFUNCTION(BlueprintPure, Category = "WARWW3|Campaign")
	int32 GetCurrentTurn() const { return State.CurrentTurn; }

	UFUNCTION(BlueprintPure, Category = "WARWW3|Campaign")
	float GetTreasury() const { return State.Treasury; }

	/** Atomic save: write to temp file then rename onto the target slot. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Campaign")
	bool SaveCampaign(const FString& SlotName, FString& OutError);

	/** Load a campaign; returns false safely for missing/corrupt/incompatible saves. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Campaign")
	bool LoadCampaign(const FString& SlotName, FString& OutError);

	UFUNCTION(BlueprintPure, Category = "WARWW3|Campaign")
	static FString GetSaveSlotPath(const FString& SlotName);

private:
	UTurnPipeline::FStateView State;
	TArray<FCountry> Countries;
	TArray<FProvince> Provinces;
	FName PlayerCountryId;
	bool bTurnResolving = false;
	bool bCampaignActive = false;
};

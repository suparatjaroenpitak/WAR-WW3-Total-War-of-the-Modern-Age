// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "Templates/SubclassOf.h"
#include "TurnPipeline.generated.h"

/** Ordered steps a single campaign turn executes; each is deterministic and independently testable. */
UENUM(BlueprintType)
enum class ETurnStep : uint8
{
	CollectRevenue,    // 1. taxes + production income
	Construction,      // 2. finish / tick buildings
	Recruitment,       // 3. complete queued recruitment
	Research,          // 4. research progress
	Diplomacy,         // 5. relation decay, treaty ticks
	MilitaryOrders,    // 6. movement, battles trigger
	Finalize,          // 7. victory/defeat evaluation, turn++, notifications
};

/** Result of resolving one turn step — used by tests to assert exact outcomes. */
USTRUCT(BlueprintType)
struct FTurnStepReport
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadOnly)
	ETurnStep Step = ETurnStep::CollectRevenue;

	UPROPERTY(BlueprintReadOnly)
	float TreasuryDelta = 0.f;

	UPROPERTY(BlueprintReadOnly)
	int32 ProvincesGained = 0;

	UPROPERTY(BlueprintReadOnly)
	int32 ProvincesLost = 0;

	UPROPERTY(BlueprintReadOnly)
	bool bStepSucceeded = false;

	UPROPERTY(BlueprintReadOnly)
	FString Notes;
};

/**
 * Deterministic turn-resolution pipeline.
 * Runs every step in order; each step is a pure, idempotent call against the campaign state.
 * No randomness is permitted inside steps (use pre-seeded hashes where chaos is required).
 */
UCLASS(NotBlueprintable, NotPlaceable)
class WARWW3_API UTurnPipeline : public UObject
{
	GENERATED_BODY()

public:
	/** Campaign state contract the pipeline operates on; filled by USavasCampaignState and campaign AI. */
	struct FStateView
	{
		float Treasury = 0.f;
		int32 CurrentTurn = 0;
		int32 ProvinceCount = 0;
		float GrossIncome = 0.f;
		float UpkeepCost = 0.f;
		TArray<int32> PendingBattles; // placeholder until battle triggers are wired
	};

	DECLARE_DELEGATE_OneParam(FTurnStepFn, FStateView& /*InOutState*/);

	/** Execute the full pipeline in order, returning per-step reports. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Campaign")
	static TArray<FTurnStepReport> ExecuteTurn(FStateView& InOutState);

	/** Exact same signature as ExecuteTurn but processes only a single named step. Tests use this directly. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Campaign")
	static FTurnStepReport ExecuteStep(ETurnStep Step, FStateView& InOutState);

private:
	static FTurnStepReport StepCollectRevenue(FStateView& InOutState);
	static FTurnStepReport StepConstruction(FStateView& InOutState);
	static FTurnStepReport StepRecruitment(FStateView& InOutState);
	static FTurnStepReport StepResearch(FStateView& inOutState);
	static FTurnStepReport StepDiplomacy(FStateView& InOutState);
	static FTurnStepReport StepMilitaryOrders(FStateView& InOutState);
	static FTurnStepReport StepFinalize(FStateView& InOutState);
};

// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
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

/** Result of resolving one turn step - used by tests to assert exact outcomes. */
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
 * Mutable campaign state the pipeline operates on.
 * Plain C++ struct (not UHT): filled by the campaign controller and later by battle hooks.
 */
struct FTurnStateView
{
	float Treasury = 0.f;
	int32 CurrentTurn = 0;
	int32 ProvinceCount = 0;
	float GrossIncome = 0.f;
	float UpkeepCost = 0.f;
	TArray<int32> PendingBattles;
};

/**
 * Deterministic turn-resolution pipeline.
 * Runs every step in order; each step is a pure, idempotent call against the state.
 * No randomness is permitted inside steps (use pre-seeded hashes where chaos is required).
 */
UCLASS(NotBlueprintable, NotPlaceable)
class WARWW3_API UTurnPipeline : public UObject
{
	GENERATED_BODY()

public:
	/** Execute the full pipeline in order, returning per-step reports. */
	static TArray<FTurnStepReport> ExecuteTurn(FTurnStateView& InOutState);

	/** Process only a single named step; tests use this directly. */
	static FTurnStepReport ExecuteStep(ETurnStep Step, FTurnStateView& InOutState);

private:
	static FTurnStepReport StepCollectRevenue(FTurnStateView& InOutState);
	static FTurnStepReport StepConstruction(FTurnStateView& InOutState);
	static FTurnStepReport StepRecruitment(FTurnStateView& InOutState);
	static FTurnStepReport StepResearch(FTurnStateView& InOutState);
	static FTurnStepReport StepDiplomacy(FTurnStateView& InOutState);
	static FTurnStepReport StepMilitaryOrders(FTurnStateView& InOutState);
	static FTurnStepReport StepFinalize(FTurnStateView& InOutState);
};

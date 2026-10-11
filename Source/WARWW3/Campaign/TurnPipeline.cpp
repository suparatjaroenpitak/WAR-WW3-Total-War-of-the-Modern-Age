// Copyright (c) WAR WW3 project. All rights reserved.

#include "Campaign/TurnPipeline.h"
#include "Core/WARWW3Core.h"

TArray<FTurnStepReport> UTurnPipeline::ExecuteTurn(FTurnStateView& InOutState)
{
	TArray<FTurnStepReport> Reports;
	const TArray<ETurnStep> Order = {
		ETurnStep::CollectRevenue,
		ETurnStep::Construction,
		ETurnStep::Recruitment,
		ETurnStep::Research,
		ETurnStep::Diplomacy,
		ETurnStep::MilitaryOrders,
		ETurnStep::Finalize
	};

	for (ETurnStep Step : Order)
	{
		Reports.Add(ExecuteStep(Step, InOutState));
	}
	return Reports;
}

FTurnStepReport UTurnPipeline::ExecuteStep(ETurnStep Step, FTurnStateView& InOutState)
{
	switch (Step)
	{
	case ETurnStep::CollectRevenue: return StepCollectRevenue(InOutState);
	case ETurnStep::Construction:  return StepConstruction(InOutState);
	case ETurnStep::Recruitment:   return StepRecruitment(InOutState);
	case ETurnStep::Research:      return StepResearch(InOutState);
	case ETurnStep::Diplomacy:     return StepDiplomacy(InOutState);
	case ETurnStep::MilitaryOrders: return StepMilitaryOrders(InOutState);
	case ETurnStep::Finalize:      return StepFinalize(InOutState);
	default:
		FTurnStepReport Bad;
		Bad.Step = Step;
		Bad.bStepSucceeded = false;
		Bad.Notes = TEXT("Unknown step");
		return Bad;
	}
}

FTurnStepReport UTurnPipeline::StepCollectRevenue(FTurnStateView& InOutState)
{
	FTurnStepReport R;
	R.Step = ETurnStep::CollectRevenue;
	const float Delta = InOutState.GrossIncome - InOutState.UpkeepCost;
	InOutState.Treasury += Delta;
	R.TreasuryDelta = Delta;
	R.bStepSucceeded = true;
	R.Notes = FString::Printf(TEXT("Income %.1f - upkeep %.1f = %.1f"), InOutState.GrossIncome, InOutState.UpkeepCost, Delta);
	UE_LOG(LogWARWW3Campaign, Verbose, TEXT("CollectRevenue: +%s treasury"), *LexToString(Delta));
	return R;
}

FTurnStepReport UTurnPipeline::StepConstruction(FTurnStateView& InOutState)
{
	FTurnStepReport R;
	R.Step = ETurnStep::Construction;
	// Construction is data-driven (FBuildingBlueprint); nothing to tick in the placeholder state view yet.
	R.bStepSucceeded = true;
	R.Notes = TEXT("No queued construction to progress.");
	return R;
}

FTurnStepReport UTurnPipeline::StepRecruitment(FTurnStateView& InOutState)
{
	FTurnStepReport R;
	R.Step = ETurnStep::Recruitment;
	R.bStepSucceeded = true;
	R.Notes = TEXT("No recruitment queue yet.");
	return R;
}

FTurnStepReport UTurnPipeline::StepResearch(FTurnStateView& inOutState)
{
	FTurnStepReport R;
	R.Step = ETurnStep::Research;
	R.bStepSucceeded = true;
	R.Notes = TEXT("No research queue yet.");
	return R;
}

FTurnStepReport UTurnPipeline::StepDiplomacy(FTurnStateView& InOutState)
{
	FTurnStepReport R;
	R.Step = ETurnStep::Diplomacy;
	R.bStepSucceeded = true;
	R.Notes = TEXT("No diplomatic agreements in placeholder state view.");
	return R;
}

FTurnStepReport UTurnPipeline::StepMilitaryOrders(FTurnStateView& InOutState)
{
	FTurnStepReport R;
	R.Step = ETurnStep::MilitaryOrders;
	R.bStepSucceeded = true;
	R.Notes = TEXT("No military orders in placeholder state view.");
	return R;
}

FTurnStepReport UTurnPipeline::StepFinalize(FTurnStateView& InOutState)
{
	FTurnStepReport R;
	R.Step = ETurnStep::Finalize;
	InOutState.CurrentTurn++;
	R.bStepSucceeded = true;
	R.Notes = FString::Printf(TEXT("Turn advanced to %d"), InOutState.CurrentTurn);
	return R;
}

// Copyright (c) WAR WW3 project. All rights reserved.

#include "Battle/BattlePlayerController.h"
#include "Core/WARWW3Core.h"
#include "Battle/WARWW3Unit.h"

ABattlePlayerController::ABattlePlayerController()
{
	bReplicates = true;
}

void ABattlePlayerController::SelectUnitsInBox(const FVector2D& ScreenMin, const FVector2D& ScreenMax)
{
	// Phase 3 will project all visible unit positions and mark selection.
	// Query helpers live here once camera control lands.
	UE_LOG(LogWARWW3Battle, Verbose, TEXT("SelectUnitsInBox placeholder called"));
}

bool ABattlePlayerController::ServerIssueMoveOrder_Validate(const TArray<AWARWW3Unit*>& Units, const FVector& TargetLocation)
{
	// Cheap validation on both paths; full server-side ownership check in _Implementation.
	return Units.Num() > 0 && Units.Num() <= 256 && TargetLocation.IsFinite();
}

void ABattlePlayerController::ServerIssueMoveOrder_Implementation(const TArray<AWARWW3Unit*>& Units, const FVector& TargetLocation)
{
	// Server-authoritative ownership check: a client may not command another player's units.
	// TODO(Phase5): compare Unit->OwningTeamId against this controller's PlayerState team.

	for (AWARWW3Unit* Unit : Units)
	{
		if (!IsValid(Unit))
		{
			continue;
		}
		// TODO(Phase5): replace with real ownership check vs Unit->OwningTeamId.
		Unit->OrderMoveTo(TargetLocation);
	}
}

void ABattlePlayerController::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);
}

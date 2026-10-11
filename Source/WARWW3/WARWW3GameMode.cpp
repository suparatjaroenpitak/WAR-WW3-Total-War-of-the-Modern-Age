// Copyright (c) WAR WW3 project. All rights reserved.

#include "WARWW3GameMode.h"
#include "Core/WARWW3Core.h"

AWARWW3GameMode::AWARWW3GameMode()
{
	// Ensure the base Pawn/Controller combo works before specific modes are wired.
	DefaultPawnClass = nullptr;
	bStartPlayersAsSpectators = false;
	PrimaryActorTick.bCanEverTick = false;
}

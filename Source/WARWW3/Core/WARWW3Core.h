// Copyright (c) WAR WW3 project. All rights reserved.

#pragma once

#include "CoreMinimal.h"

/** Application-wide logging categories for WAR WW3. */
DECLARE_LOG_CATEGORY_EXTERN(LogWARWW3Core, Log, All);
DECLARE_LOG_CATEGORY_EXTERN(LogWARWW3Campaign, Log, All);
DECLARE_LOG_CATEGORY_EXTERN(LogWARWW3Battle, Log, All);
DECLARE_LOG_CATEGORY_EXTERN(LogWARWW3Net, Log, All);
DECLARE_LOG_CATEGORY_EXTERN(LogWARWW3AI, Log, All);

/** Shared constants for campaign and battle systems. */
namespace WARWW3Const
{
	/** Max campaign turn before the game force-resolves (safety valve, config can lower it). */
	constexpr int32 DefaultMaxCampaignTurn = 500;
	/** Per-tick budget for battle AI thinking, in milliseconds (profiling target). */
	constexpr float BattleAIBudgetMs = 2.0f;
}

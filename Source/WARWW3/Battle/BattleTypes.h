// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "BattleTypes.generated.h"

/** Unit archetypes present on the battlefield. */
UENUM(BlueprintType)
enum class EUnitClass : uint8
{
	Infantry,
	Recon,
	Tank,
	ArmoredPersonnelCarrier,
	Artillery,
	Helicopter,
	Transport,
	Support,
};

/** All combat-relevant data of a unit archetype; driven from a data table row. */
USTRUCT(BlueprintType)
struct FUnitDef
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FName UnitId;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FText UnitName;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	EUnitClass UnitClass = EUnitClass::Infantry;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	int32 MaxHealth = 100;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	float MaxSpeed = 100.f;

	/** Damage per shot vs light armor; values are placeholders until balance pass. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	float DamageVsLight = 10.f;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	float DamageVsHeavy = 5.f;

	/** Weapon effective range in unreal units (100 UU = 1 m). */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	float WeaponRange = 10000.f;

	/** 0..1 base chance to hit before cover modifiers. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	float BaseAccuracy = 0.6f;

	/** True when a unit needs line of sight before it may fire. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	bool bRequiresLineOfSight = true;

	/** Standard command-point cost of fielding this unit; drawn from the deployment budget. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	int32 CommandPointCost = 1;
};

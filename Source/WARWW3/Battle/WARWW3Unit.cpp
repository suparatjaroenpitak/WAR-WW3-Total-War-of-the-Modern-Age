// Copyright (c) WAR WW3 project. All rights reserved.

#include "Battle/WARWW3Unit.h"
#include "Core/WARWW3Core.h"
#include "Components/StaticMeshComponent.h"
#include "Net/UnrealNetwork.h"

AWARWW3Unit::AWARWW3Unit()
{
	PrimaryActorTick.bCanEverTick = true;
	PrimaryActorTick.TickInterval = 0.1f; // 10 Hz base; profiling may lower this further

	RootComponent = CreateDefaultSubobject<USceneComponent>(TEXT("Root"));
	MeshComponent = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Mesh"));
	MeshComponent->SetupAttachment(RootComponent);

	bReplicates = true;
}

void AWARWW3Unit::ApplyUnitDef(const FUnitDef& InDef)
{
	UnitDef = InDef;
	Health = InDef.MaxHealth;
}

float AWARWW3Unit::GetHealthPct() const
{
	return (UnitDef.MaxHealth > 0) ? Health / UnitDef.MaxHealth : 0.f;
}

bool AWARWW3Unit::ApplyDamage(float DamageAmount, AActor* DamageCauser)
{
	if (DamageAmount <= 0.f || Health <= 0.f)
	{
		return false;
	}
	Health = FMath::Max(0.f, Health - DamageAmount);
	UE_LOG(LogWARWW3Battle, Verbose, TEXT("%s took %.1f damage, health %.1f"),
		*GetName(), DamageAmount, Health);
	if (Health <= 0.f)
	{
		OnUnitDestroyed.Broadcast(DamageCauser);
		Destroy();
		return true;
	}
	return false;
}

void AWARWW3Unit::OrderMoveTo(const FVector& TargetLocation)
{
	MoveTarget = TargetLocation;
	bHasMoveOrder = true;
}

void AWARWW3Unit::Tick(float DeltaSeconds)
{
	Super::Tick(DeltaSeconds);
	if (bHasMoveOrder)
	{
		const FVector Delta = MoveTarget - GetActorLocation();
		const float DistSq = Delta.SizeSquared();
		const float Step = UnitDef.MaxSpeed * DeltaSeconds;
		if (DistSq <= Step * Step)
		{
			SetActorLocation(MoveTarget);
			bHasMoveOrder = false;
		}
		else
		{
			SetActorLocation(GetActorLocation() + Delta.GetSafeNormal() * Step);
		}
	}
}

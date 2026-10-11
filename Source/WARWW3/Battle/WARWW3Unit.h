// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Battle/BattleTypes.h"
#include "WARWW3Unit.generated.h"

/**
 * One battlefield unit (infantry squad, tank, artillery gun, helicopter...).
 * Movement, health, and combat statistics. Server-authoritative when in multiplayer.
 */
UCLASS()
class WARWW3_API AWARWW3Unit : public AActor
{
	GENERATED_BODY()

public:
	AWARWW3Unit();

	UFUNCTION(BlueprintPure, Category = "WARWW3|Unit")
	const FUnitDef& GetUnitDef() const { return UnitDef; }

	UFUNCTION(BlueprintCallable, Category = "WARWW3|Unit")
	void ApplyUnitDef(const FUnitDef& InDef);

	UFUNCTION(BlueprintPure, Category = "WARWW3|Unit")
	float GetHealth() const { return Health; }

	UFUNCTION(BlueprintPure, Category = "WARWW3|Unit")
	float GetHealthPct() const;

	/** Deduct health; returns true if this shot destroyed the unit. Server-authoritative. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Unit")
	bool ApplyDamage(float DamageAmount, AActor* DamageCauser);

	/** Move to a target position; converted to steering in Phase 3. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Unit")
	void OrderMoveTo(const FVector& TargetLocation);

	/** Broadcast when this unit dies. */
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnUnitDestroyed, AActor*, DamageCauser);

	UPROPERTY(BlueprintAssignable, Category = "WARWW3|Unit")
	FOnUnitDestroyed OnUnitDestroyed;

	virtual void Tick(float DeltaSeconds) override;

	virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

private:
	UPROPERTY(EditAnywhere, Category = "WARWW3|Unit")
	FUnitDef UnitDef;

	UPROPERTY(Replicated, VisibleInstanceOnly, Category = "WARWW3|Unit")
	float Health = 100.f;

	/** Move target in world space; invalid = hold position. */
	UPROPERTY(Replicated, VisibleInstanceOnly, Category = "WARWW3|Unit")
	FVector MoveTarget = FVector::ZeroVector;

	UPROPERTY(VisibleAnywhere, Category = "WARWW3|Unit")
	TObjectPtr<UStaticMeshComponent> MeshComponent;

	bool bHasMoveOrder = false;
};

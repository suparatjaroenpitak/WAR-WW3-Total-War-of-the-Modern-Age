// Copyright (c) WARWW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "BattlePlayerController.generated.h"

class AWARWW3Unit;

/**
 * Battle player controller: handles input routing and *requests*.
 * Movement/combat are only executed on the server after validation (ownership + rules).
 * In multiplayer, clients may only call the Server RPCs below.
 */
UCLASS()
class WARWW3_API ABattlePlayerController : public APlayerController
{
	GENERATED_BODY()

public:
	ABattlePlayerController();

	/** Client-side entry point: select units within a screen-space box. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Battle")
	void SelectUnitsInBox(const FVector2D& ScreenMin, const FVector2D& ScreenMax);

	/** Request to move the given units. Server validates ownership and legality. */
	UFUNCTION(BlueprintCallable, Category = "WARWW3|Battle", Server, Reliable, WithValidation)
	void ServerIssueMoveOrder(const TArray<AWARWW3Unit*>& Units, const FVector& TargetLocation);

	virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;
};

// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "CampaignTypes.generated.h"

/** Diplomatic stance between two countries. */
UENUM(BlueprintType)
enum class EDiplState : uint8
{
	War,
	Neutral,
	NonAggression,
	Alliance,
};

/** One province. Ownership, buildings, and recruitment live here. */
USTRUCT(BlueprintType)
struct FProvince
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FName ProvinceId;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FText ProvinceName;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FName OwnerCountryId;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	int32 Population = 1;

	/** Tax revenue per turn, in treasury units. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	float BaseIncome = 10.f;

	/** 0..100; below 30 triggers unrest events. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	float Stability = 50.f;

	/** Names of building blueprints already built here. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	TArray<FName> Buildings;
};

/** One playable country. */
USTRUCT(BlueprintType)
struct FCountry
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FName CountryId;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FText CountryName;

	/** UI color for map fill and unit accents. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FLinearColor CountryColor = FLinearColor::White;

	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	float Treasury = 100.f;

	/** Capital province id; losing it triggers defeat evaluation. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FName CapitalProvinceId;

	/** Which player or AI (empty name = unclaimed, AI default) controls this country. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	FName ControllerTag;
};

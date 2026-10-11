// Copyright (c) WAR WW3 project. All rights reserved.

#include "Campaign/CampaignController.h"
#include "Core/WARWW3Core.h"
#include "SaveSystem/WARWW3CampaignSaveGame.h"
#include "Misc/FileHelper.h"
#include "Misc/Paths.h"
#include "HAL/PlatformFileManager.h"
#include "Kismet/GameplayStatics.h"

namespace
{
	FString MakeSavePath(const FString& SlotName)
	{
		return FPaths::Combine(FPaths::ProjectSavedDir(), TEXT("SaveGames"),
			SlotName + TEXT(".sav"));
	}
}

void UCampaignController::StartNewCampaign(FName InPlayerCountryId, const TArray<FCountry>& InCountries, const TArray<FProvince>& InProvinces)
{
	PlayerCountryId = InPlayerCountryId;
	Countries = InCountries;
	Provinces = InProvinces;

	State = UTurnPipeline::FStateView();
	State.Treasury = 0.f;
	State.CurrentTurn = 0;

	// Compute starting income from owned provinces.
	State.GrossIncome = 0.f;
	for (const FProvince& P : Provinces)
	{
		if (P.OwnerCountryId == PlayerCountryId)
		{
			State.GrossIncome += P.BaseIncome;
			State.Treasury += 50.f; // starting funds per owned province
		}
	}
	State.UpkeepCost = 0.f;
	bCampaignActive = true;
	bTurnResolving = false;

	UE_LOG(LogWARWW3Campaign, Log, TEXT("New campaign: player=%s provinces=%d startTreasury=%s"),
		*PlayerCountryId.ToString(), Provinces.Num(), *LexToString(State.Treasury));
}

bool UCampaignController::EndTurn(TArray<FTurnStepReport>& OutReports)
{
	OutReports.Reset();

	if (!bCampaignActive)
	{
		UE_LOG(LogWARWW3Campaign, Warning, TEXT("EndTurn ignored: no active campaign"));
		return false;
	}
	if (bTurnResolving)
	{
		UE_LOG(LogWARWW3Campaign, Verbose, TEXT("EndTurn ignored: turn already resolving"));
		return false;
	}

	// Income may have changed due to construction/recruitment since last turn; recompute.
	float NewIncome = 0.f;
	for (const FProvince& P : Provinces)
	{
		if (P.OwnerCountryId == PlayerCountryId)
		{
			NewIncome += P.BaseIncome;
		}
	}
	State.GrossIncome = NewIncome;

	bTurnResolving = true;
	OutReports = UTurnPipeline::ExecuteTurn(State);
	bTurnResolving = false;
	return true;
}

bool UCampaignController::SaveCampaign(const FString& SlotName, FString& OutError)
{
	OutError.Reset();
	if (!bCampaignActive)
	{
		OutError = TEXT("No active campaign to save");
		return false;
	}

	UWARWW3CampaignSaveGame* SaveObj = Cast<UWARWW3CampaignSaveGame>(
		UGameplayStatics::CreateSaveGameObject(UWARWW3CampaignSaveGame::StaticClass()));
	if (!SaveObj)
	{
		OutError = TEXT("Failed to create save object");
		return false;
	}

	SaveObj->SchemaVersion = WARWW3_SAVE_SCHEMA_VERSION;
	SaveObj->CampaignTurn = State.CurrentTurn;
	SaveObj->PlayerCountryId = PlayerCountryId;
	SaveObj->Provinces = Provinces;
	SaveObj->Countries = Countries;
	SaveObj->PlayerTreasury = State.Treasury;
	SaveObj->SavedAtUtc = FDateTime::UtcNow();

	const FString FinalPath = MakeSavePath(SlotName);
	const FString TempPath = FinalPath + TEXT(".tmp");

	IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
	PlatformFile.CreateDirectoryTree(*FPaths::GetPath(TempPath));

	// Atomic write: serialize to temp, then rename over the final file.
	FBufferArchive Ar;
	Ar << SaveObj->SchemaVersion;
	Ar << SaveObj->CampaignTurn;
	Ar << SaveObj->PlayerCountryId;
	{
		// Manual serialization of structs via operator<< provided by TArray/FName wrappers.
		const int32 NumProvinces = SaveObj->Provinces.Num();
		Ar << NumProvinces;
		for (FProvince& P : SaveObj->Provinces)
		{
			Ar << P.ProvinceId;
			Ar << P.OwnerCountryId;
			Ar << P.Population;
			Ar << P.BaseIncome;
			Ar << P.Stability;
			Ar << P.Buildings;
		}
		const int32 NumCountries = SaveObj->Countries.Num();
		Ar << NumCountries;
		for (FCountry& C : SaveObj->Countries)
		{
			Ar << C.CountryId;
			Ar << C.CountryName;
			Ar << C.CountryColor;
			Ar << C.Treasury;
			Ar << C.CapitalProvinceId;
			Ar << C.ControllerTag;
		}
	}
	Ar << SaveObj->PlayerTreasury;
	Ar << SaveObj->SavedAtUtc;

	if (!FFileHelper::SaveArrayToFile(Ar, *TempPath))
	{
		OutError = TEXT("Failed to write temporary save file");
		return false;
	}

	if (!PlatformFile.MoveFile(*FinalPath, *TempPath))
	{
		OutError = TEXT("Failed to commit save (rename failed)");
		PlatformFile.DeleteFile(*TempPath);
		return false;
	}
	return true;
}

bool UCampaignController::LoadCampaign(const FString& SlotName, FString& OutError)
{
	OutError.Reset();
	if (SlotName.Contains(TEXT("..")) || SlotName.Contains(TEXT("/")) || SlotName.Contains(TEXT("\\")))
	{
		OutError = TEXT("Invalid slot name");
		return false;
	}

	const FString FinalPath = MakeSavePath(SlotName);
	IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
	if (!PlatformFile.FileExists(*FinalPath))
	{
		OutError = TEXT("Save file not found");
		return false;
	}

	TArray<uint8> RawBytes;
	if (!FFileHelper::LoadFileToArray(RawBytes, *FinalPath))
	{
		OutError = TEXT("Failed to read save file");
		return false;
	}

	FMemoryReader Reader(RawBytes);
	int32 SchemaVersion = 0;
	Reader << SchemaVersion;
	if (SchemaVersion != WARWW3_SAVE_SCHEMA_VERSION)
	{
		OutError = FString::Printf(TEXT("Incompatible save schema %d (expected %d)"), SchemaVersion, WARWW3_SAVE_SCHEMA_VERSION);
		return false;
	}

	int32 LoadedTurn = 0;
	Reader << LoadedTurn;
	FName LoadedPlayerCountry;
	Reader << LoadedPlayerCountry;

	int32 NumProvinces = 0;
	Reader << NumProvinces;
	TArray<FProvince> LoadedProvinces;
	for (int32 i = 0; i < NumProvinces; ++i)
	{
		FProvince P;
		Reader << P.ProvinceId;
		Reader << P.OwnerCountryId;
		Reader << P.Population;
		Reader << P.BaseIncome;
		Reader << P.Stability;
		Reader << P.Buildings;
		LoadedProvinces.Add(MoveTemp(P));
	}

	int32 NumCountries = 0;
	Reader << NumCountries;
	TArray<FCountry> LoadedCountries;
	for (int32 i = 0; i < NumCountries; ++i)
	{
		FCountry C;
		Reader << C.CountryId;
		Reader << C.CountryName;
		Reader << C.CountryColor;
		Reader << C.Treasury;
		Reader << C.CapitalProvinceId;
		Reader << C.ControllerTag;
		LoadedCountries.Add(MoveTemp(C));
	}

	float LoadedTreasury;
	FDateTime SavedAtUtc;
	Reader << LoadedTreasury;
	Reader << SavedAtUtc;

	if (Reader.IsError())
	{
		OutError = TEXT("Corrupt save: serialization error");
		return false;
	}

	// Commit the loaded state atomically-in-memory: only after full validation.
	PlayerCountryId = LoadedPlayerCountry;
	Provinces = MoveTemp(LoadedProvinces);
	Countries = MoveTemp(LoadedCountries);
	State = UTurnPipeline::FStateView();
	State.CurrentTurn = LoadedTurn;
	State.Treasury = LoadedTreasury;
	State.GrossIncome = 0.f;
	for (const FProvince& P : Provinces)
	{
		if (P.OwnerCountryId == PlayerCountryId)
		{
			State.GrossIncome += P.BaseIncome;
		}
	}
	State.UpkeepCost = 0.f;
	bCampaignActive = true;
	bTurnResolving = false;
	return true;
}

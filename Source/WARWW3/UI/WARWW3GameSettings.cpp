// Copyright (c) WAR WW3 project. All rights reserved.

#include "UI/WARWW3GameSettings.h"
#include "Core/WARWW3Core.h"
#include "Engine/GameInstance.h"
#include "Internationalization/Internationalization.h"
#include "GameFramework/GameUserSettings.h"

void UWARWW3GameSettings::SetLanguage(EGameLanguage NewLanguage, UGameInstance* OwningInstance)
{
	CurrentLanguage = NewLanguage;

	const FString Locale = (NewLanguage == EGameLanguage::Thai) ? TEXT("th-TH") : TEXT("en");
	FInternationalization::Get().SetCurrentCulture(Locale);

	// Persist the choice so it survives application restarts.
	if (UGameUserSettings* UserSettings = UGameUserSettings::GetGameUserSettings())
	{
		UserSettings->SetToDefaults(); // common baseline; specific settings persisted via Config property below
		UserSettings->ApplySettings(false);
	}
	if (UGameInstance* GI = OwningInstance)
	{
		GI->SaveConfig();
	}
}

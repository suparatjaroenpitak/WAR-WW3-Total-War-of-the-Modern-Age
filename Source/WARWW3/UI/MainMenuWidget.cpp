// Copyright (c) WAR WW3 project. All rights reserved.

#include "UI/MainMenuWidget.h"
#include "Core/WARWW3Core.h"
#include "Components/Button.h"
#include "Components/ComboBoxString.h"
#include "Kismet/GameplayStatics.h"
#include "WARWW3GameInstance.h"

#define LOCTEXT_NAMESPACE "MainMenu"

void UMainMenuWidget::NativeOnInitialized()
{
	Super::NativeOnInitialized();

	if (BtnStartCampaign)
	{
		BtnStartCampaign->OnClicked.AddDynamic(this, &UMainMenuWidget::OnStartCampaign);
	}
	if (BtnSettings)
	{
		BtnSettings->OnClicked.AddDynamic(this, &UMainMenuWidget::OnSettings);
	}
	if (BtnQuit)
	{
		BtnQuit->OnClicked.AddDynamic(this, &UMainMenuWidget::OnQuit);
	}
	if (CbLanguage)
	{
		CbLanguage->OnSelectionChanged.AddDynamic(this, &UMainMenuWidget::OnLanguageChanged);
		// Populate with localizable display names.
		CbLanguage->AddOption(FInternationalization::Get().GetDisplayString(TEXT("en"), TEXT("en")).ToString());
		CbLanguage->AddOption(FInternationalization::Get().GetDisplayString(TEXT("th"), TEXT("th")).ToString());
	}
}

void UMainMenuWidget::OnStartCampaign()
{
	UE_LOG(LogWARWW3Core, Log, TEXT("Main menu: Start Campaign clicked"));
	// Phase 2 will load the campaign map level here; menu stub for now.
}

void UMainMenuWidget::OnSettings()
{
	UE_LOG(LogWARWW3Core, Log, TEXT("Main menu: Settings clicked"));
}

void UMainMenuWidget::OnQuit()
{
	UKismetSystemLibrary::QuitGame(GetWorld(), UGameplayStatics::GetPlayerController(GetWorld(), 0), EQuitPreference::Quit, false);
}

void UMainMenuWidget::OnLanguageChanged(FString SelectedItem, ESelectMethod::Type SelectionMethodType)
{
	if (!GetGameInstance())
	{
		return;
	}
	UWARWW3GameInstance* GI = Cast<UWARWW3GameInstance>(GetGameInstance());
	if (!GI || !GI->GetGameSettings())
	{
		return;
	}
	// Match displayed option back to language code.
	const bool bThai = SelectedItem.Contains(TEXT("ไทย")) || SelectedItem.Contains(TEXT("Thai"));
	GI->GetGameSettings()->SetLanguage(bThai ? EGameLanguage::Thai : EGameLanguage::English, GI);
}

#undef LOCTEXT_NAMESPACE

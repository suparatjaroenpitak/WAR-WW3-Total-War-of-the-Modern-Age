// Copyright (c) WAR WW3 project. All rights reserved.
#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "WARWW3GameSettings.h"
#include "MainMenuWidget.generated.h"

class UButton;
class UComboBoxString;

/**
 * Main menu: start campaign, open settings, quit.
 * All text flows through localization so Thai and English both have real translations.
 */
UCLASS()
class WARWW3_API UMainMenuWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	virtual void NativeOnInitialized() override;

protected:
	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> BtnStartCampaign;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> BtnSettings;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> BtnQuit;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UComboBoxString> CbLanguage;

	UFUNCTION()
	void OnStartCampaign();

	UFUNCTION()
	void OnSettings();

	UFUNCTION()
	void OnQuit();

	UFUNCTION()
	void OnLanguageChanged(FString SelectedItem, ESelectMethod::Type SelectionMethodType);
};

// Copyright (c) WAR WW3 project. All rights reserved.
using UnrealBuildTool;

public class WARWW3 : ModuleRules
{
	public WARWW3(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[]
		{
			"Core",
			"CoreUObject",
			"Engine",
			"InputCore",
			"GameplayTags",
			"UMG",
			"Slate",
			"SlateCore",
			"StructUtils",
		});
		PrivateDependencyModuleNames.AddRange(new string[]
		{
			"GameplayTasks", // AI utility work
		});
	}
}

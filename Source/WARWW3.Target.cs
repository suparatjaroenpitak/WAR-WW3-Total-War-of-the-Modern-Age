// Copyright (c) WAR WW3 project. All rights reserved.
using UnrealBuildTool;
using System.Collections.Generic;

public class WARWW3Target : TargetRules
{
	public WARWW3Target(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Game;
		DefaultBuildSettings = BuildSettingsVersion.V7;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("WARWW3");
	}
}

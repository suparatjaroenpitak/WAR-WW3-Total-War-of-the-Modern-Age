// Copyright (c) WAR WW3 project. All rights reserved.
using UnrealBuildTool;
using System.Collections.Generic;

public class WARWW3EditorTarget : TargetRules
{
	public WARWW3EditorTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Editor;
		DefaultBuildSettings = BuildSettingsVersion.V7;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("WARWW3");
	}
}

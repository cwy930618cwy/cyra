// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class code : ModuleRules
{
	public code(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
	
		PublicDependencyModuleNames.AddRange(new string[] { 
			"Core", 
			"CoreUObject", 
			"Engine", 
			"InputCore",
			"GameplayAbilities",   // GAS 核心
			"GameplayTags",        // Tag 系统
			"GameplayTasks"        // 异步任务		
		});
	}
}

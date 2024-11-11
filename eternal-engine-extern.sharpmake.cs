using Sharpmake;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

[module: Sharpmake.Include(@"..\eternal-engine\eternal-engine-project.sharpmake.cs")]

namespace EternalEngine
{
	[Sharpmake.Generate]
	public class EternalEngineExternProject : EternalEngineBaseProject
	{
		static readonly Dictionary<string, string> FiltersRemapping = new Dictionary<string, string> {
			{ @"rapidjson\contrib\natvis", @"rapidjson\natvis" }
		};

		static readonly string[] GeneralFiltersRemapping = new string[] {
			"src",
			"source",
			"include",
		};

		static readonly string[] NonMicrosoftExclusions = new string[] {
			"DirectXTex",
			"DirectX-Headers",
			"NVIDIANsightAftermath",
			"renderdoc",
		};

		static readonly string[] MicrosoftExclusions = new string[] {
			"DirectXTexD3D11",
			"DDSTextureLoader9",
			"DDSTextureLoader11",
			"BCDirectCompute",
			"DirectXTexCompressGPU",
			"ScreenGrab",
			"Texassemble",
			@"Texconv\",
			"Texdiag",
			"WICTextureLoader",
			"DirectX-Headers",
			"NVIDIANsightAftermath",
			"renderdoc",
		};

		public EternalEngineExternProject()
			: base("extern")
		{
			SourceFilesExcludeRegex.AddRange(new string[] {
				@"(.*)imgui\\(.*)\\(.*)",
				@"(.*)rapidjson\\example\\(.*)",
				@"(.*)rapidjson\\test\\(.*)",
				@"(.*)rapidjson\\thirdparty\\(.*)",
				@"(.*)DirectX-Headers\\test\\(.*)",
				@"(.*)DirectXTex\\DDSView\\(.*)",
				@"(.*)optick\\samples\\(.*)",
				@"(.*)optick\\samples\\(.*)",
			});
		}

		public override void PreResolveSourceFiles()
		{
			base.PreResolveSourceFiles();

			for (int ConfigurationIndex = 0; ConfigurationIndex < Configurations.Count; ++ConfigurationIndex)
			{
				if (Configurations[ConfigurationIndex].Platform != Platform.win64 && Configurations[ConfigurationIndex].Platform != Platform.win32 && Configurations[ConfigurationIndex].Platform != Platform.scarlett)
				{
					foreach (string CurrentNonWindowsExclusion in NonMicrosoftExclusions)
					{
						Configurations[ConfigurationIndex].SourceFilesBuildExcludeRegex.Add(".*" + CurrentNonWindowsExclusion.ToLowerInvariant() + ".*");
					}
				}
				else
				{
					foreach (string CurrentScarlettExclusion in MicrosoftExclusions)
					{
						//Configurations[ConfigurationIndex].
						Configurations[ConfigurationIndex].SourceFilesBuildExcludeRegex.Add(".*" + CurrentScarlettExclusion.ToLowerInvariant() + @"\.*");
					}
				}
			}
		}

		public override void ConfigureAll(Configuration InConfiguration, Target InTarget)
		{
			base.ConfigureAll(InConfiguration, InTarget);

			// Include paths
			InConfiguration.IncludePaths.AddRange(new string[] {
				@"[conf.ProjectPath]\libtga\include",
				@"$(SolutionDir)eternal-engine-core\include",
			});

			if (ExtensionMethods.IsPC(InTarget.Platform))
			{
				InConfiguration.IncludePaths.AddRange(new string[] {
					@"[conf.ProjectPath]\DirectX-Headers\include",
					@"[conf.ProjectPath]\DirectXTex\DirectXTex",
					@"[conf.ProjectPath]\NVIDIANsightAftermath\include",
					@"[conf.ProjectPath]\renderdoc\include",
				});
			}

			// Include system paths
			InConfiguration.IncludeSystemPaths.AddRange(new string[] {
				@"$(SolutionDir)eternal-engine-extern\DirectX-Headers\include",
				EternalEngineSettings.VulkanPath + @"\Include",
			});
		}

		public override bool ResolveFilterPath(string InRelativePath, out string OutFilterPath)
		{
			if (FiltersRemapping.ContainsKey(InRelativePath))
			{
				OutFilterPath = FiltersRemapping[InRelativePath];
				return true;
			}

			for (int GeneralFiltersRemappingIndex = 0; GeneralFiltersRemappingIndex < GeneralFiltersRemapping.Length; ++GeneralFiltersRemappingIndex)
			{
				Match GeneralFilterMatch;
				GeneralFilterMatch = Regex.Match(InRelativePath, @"(.*)\\" + GeneralFiltersRemapping[GeneralFiltersRemappingIndex] + @"\\(.*)", RegexOptions.IgnoreCase);
				if (GeneralFilterMatch.Success)
				{
					OutFilterPath = GeneralFilterMatch.Groups[1].Value + @"\" + GeneralFilterMatch.Groups[2].Value;
					return true;
				}

				GeneralFilterMatch = Regex.Match(InRelativePath, @"(.*)\\" + GeneralFiltersRemapping[GeneralFiltersRemappingIndex], RegexOptions.IgnoreCase);
				if (GeneralFilterMatch.Success)
				{
					OutFilterPath = GeneralFilterMatch.Groups[1].Value;
					return true;
				}
			}

			return base.ResolveFilterPath(InRelativePath, out OutFilterPath);
		}
	}
}

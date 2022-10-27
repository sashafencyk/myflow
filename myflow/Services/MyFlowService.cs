using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace myflow.Services;

public class MyFlowService
{
	private readonly ILogger _logger;
	private readonly IConfiguration _configuration;
	private readonly GitService _gitService;
	private readonly VersionFileService _versionFileService;
	private readonly BranchResolverService _branchResolverService;
	private readonly DevOpsPipelineService _pipelineService;

	public MyFlowService(ILogger<MyFlowService> logger, IConfiguration configuration, GitService gitService,
		VersionFileService versionFileService, BranchResolverService branchResolverService, DevOpsPipelineService pipelineService)
	{
		_logger = logger;
		_configuration = configuration;
		_gitService = gitService;
		_versionFileService = versionFileService;
		_branchResolverService = branchResolverService;
		_pipelineService = pipelineService;
	}

	public async Task RunFlowAsync()
	{
		try
		{
			var branch = await _branchResolverService.GetBranchAsync();
			_logger.LogInformation("Branch model: {BranchModel}", branch);

			var version = await DetermineVersionAsync(branch);
			_logger.LogInformation("Version:{Version}", version);
			
			var buildNumber = DetermineBuildNumber(branch, version);
			_logger.LogInformation("BuildNumber:{BuildNumber}", buildNumber);
			
			var tag = branch.IsMaster() ? version : null;
			_logger.LogInformation("Tag:{Tag}", tag);

			var variables = DetermineVariables(branch, version);
			foreach (var (key, value) in variables)
			{
				_logger.LogInformation("{Key}:{Value}", key, value);
			}

			_pipelineService.SetVariables(variables);
			_pipelineService.UpdateBuildNumber(buildNumber);
			if (branch.IsMaster() && !branch.IsPr())
			{
				await _gitService.SetTagIfNotExistAsync(version);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while running the flow");
			_pipelineService.FailPipeline(ex.Message);
		}
	}

	private Dictionary<string, string> DetermineVariables(BranchModel branch, Version version)
	{
		return new Dictionary<string, string>
		{
			{ "FLOW_ORIGINAL_BUILDNUMBER", _configuration["BUILD_BUILDNUMBER"] },
			{ "FLOW_VERSION", version.ToString() }
		};
	}

	private string DetermineBuildNumber(BranchModel branch, Version version)
	{
		if (branch.IsMaster())
		{
			return version.ToString();
		}

		var buildId = _configuration["BUILD_BUILDID"];
		if (string.IsNullOrWhiteSpace(buildId))
		{
			throw new Exception("Environment variable BUILD_BUILDID is missing");
		}

		var versionSuffixBranch =
			branch.IsPr() ? "pullrequest" :
			branch.IsRelease() ? "release" :
			branch.IsHotfix() ? "hotfix" :
			Regex.Replace(branch.Branch, "[^a-zA-Z0-9 -]", "-").ToLower();

		var versionSuffix = $"-{versionSuffixBranch}-{buildId}";
		return $"{version}{versionSuffix}";
	}

	private async Task<Version> DetermineVersionAsync(BranchModel branch)
	{
		var fileVersion = await _versionFileService.GetVersionAsync();
		_logger.LogInformation("File version:{FileVersion}", fileVersion);

		var currentCommitTagVersion = await _gitService.GetCurrentCommitTagVersionAsync();
		_logger.LogInformation("CurrentCommitTagVersion: {CurrentCommitTagVersion}", currentCommitTagVersion);

		Version version;
		if (currentCommitTagVersion != null)
		{
			version = currentCommitTagVersion;
		}
		else if (branch.IsMaster() || branch.IsRelease() || branch.IsHotfix())
		{
			version = await _gitService.GetNextBuildVersionAsync(fileVersion);
		}
		else
		{
			version = fileVersion;
		}

		if ((branch.IsRelease() || branch.IsHotfix())
		    && (branch.BranchVersion() == null || branch.BranchVersion() != version))
		{
			throw new Exception($"Branch version is invalid. FileVersion: {fileVersion}; " +
			                    $"Expected: {version}; BranchVersion: {branch.BranchVersion()}");
		}

		return version;
	}
}
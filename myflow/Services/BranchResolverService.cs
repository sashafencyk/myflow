using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace myflow.Services;

public class BranchResolverService
{
	private readonly ILogger _logger;
	private readonly IConfiguration _configuration;
	private readonly GitService _gitService;

	public BranchResolverService(ILogger<BranchResolverService> logger, IConfiguration configuration, GitService gitService)
	{
		_logger = logger;
		_configuration = configuration;
		_gitService = gitService;
	}
	
	public async Task<BranchModel> GetBranchAsync()
	{
		var pullRequestSourceBranch = _configuration["SYSTEM_PULLREQUEST_SOURCEBRANCH"];
		_logger.LogInformation("SYSTEM_PULLREQUEST_SOURCEBRANCH: {SYSTEM_PULLREQUEST_SOURCEBRANCH}",
			pullRequestSourceBranch);

		var buildSourceBranch = _configuration["BUILD_SOURCEBRANCH"];
		_logger.LogInformation("BUILD_SOURCEBRANCH: {BUILD_SOURCEBRANCH}", buildSourceBranch);

		var activeGitBranch = await _gitService.GetActiveBranchAsync();
		_logger.LogInformation("Active branch: {ActiveBranch}", activeGitBranch);

		string? rawBranch;
		if (!string.IsNullOrWhiteSpace(pullRequestSourceBranch))
		{
			rawBranch = pullRequestSourceBranch;
		}
		else if (!string.IsNullOrWhiteSpace(buildSourceBranch))
		{
			rawBranch = buildSourceBranch;
		}
		else
		{
			rawBranch = activeGitBranch;
		}

		if (rawBranch == null)
		{
			throw new Exception("Branch can not be selected");
		}

		var match = Regex.Match(rawBranch, "^(refs)/(heads|pull)/(.*)$");
		if (!match.Success || match.Groups.Count != 4)
		{
			throw new Exception("Branch can not be resolved");
		}
		
		var branch = match.Groups.Values.ElementAtOrDefault(3)!.Value;
		_logger.LogInformation("Resolved branch:{Branch}", branch);

		return new BranchModel(pullRequestSourceBranch, buildSourceBranch, activeGitBranch, branch);
	}
}

public record BranchModel(string? PrSourceBranch, string? BuildSourceBranch, string ActiveGitBranch, string Branch)
{
	public bool IsDevelop()
	{
		return Branch == "develop";
	}

	public bool IsRelease()
	{
		return Regex.IsMatch(Branch, "^release/");
	}

	public bool IsHotfix()
	{
		return Regex.IsMatch(Branch, "^hotfix/");
	}
	
	public bool IsMaster()
	{
		return Branch == "master";
	}

	public bool IsPr()
	{
		return !string.IsNullOrWhiteSpace(PrSourceBranch);
	}

	public string BranchEnvironment()
	{
		return IsPr() ? "pr" :
			IsMaster() ? "prod" :
			IsHotfix() || IsRelease() ? "test" : "dev";
	}

	public Version? BranchVersion()
	{
		var matchValue = Regex.Match(Branch, "^(release|hotfix)/(.*)$").Groups[2].Value;
		Version.TryParse(matchValue, out var version);
		return version;
	}
	
}
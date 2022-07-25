using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace myflow.Services;

public class GitService
{
	private readonly ILogger _logger;
	
	public GitService(ILogger<GitService> logger)
	{
		_logger = logger;
	}
	
	public async Task<string> GetActiveBranchAsync()
	{
		var res = await ExecuteGitCommand("symbolic-ref HEAD");
		return res.Trim();
	}

	public async Task<Version?> GetCurrentCommitTagVersionAsync()
	{
		var tags = await GetAllTagVersionsAsync(currentCommit: true);
		var tag = tags.OrderByDescending(x => x).FirstOrDefault();
		return tag;
	}

	public async Task<Version> GetNextBuildVersionAsync(Version version)
	{
		var tags = await GetAllTagVersionsAsync($"{version.Major}.{version.Minor}.*");
		var nextBuildVersion = 0;
		if (tags.Any())
		{
			nextBuildVersion = tags.Max(x => x.Build) + 1;
		}
		return new Version(version.Major, version.Minor, nextBuildVersion);
	}

	private async Task<IEnumerable<Version>> GetAllTagVersionsAsync(string versionFilter = "*", bool currentCommit = false)
	{
		var specificCommitFilter = currentCommit ? " --points-at HEAD" : "";
		var commandRes = await ExecuteGitCommand($"tag -l \"v{versionFilter}\"{specificCommitFilter}");
		var versions = commandRes.Split(
				new[] { "\r\n", "\r", "\n" },
				StringSplitOptions.None
			)
			.Select(x => x.Trim())
			.Where(x => x.StartsWith("v"))
			.Select(x => !Version.TryParse(x.TrimStart('v'), out var version) ? null : version)
			.Where(x => x != null)
			.Select(x => x!)
			.ToList();
		return versions;
	}

	public async Task SetTagIfNotExistAsync(Version version)
	{
		//delete all local tags that do not exist in the remote.
		await ExecuteGitCommand("fetch --prune origin \"+refs/tags/*:refs/tags/*\"");

		// Check current commit for the version
		var currentCommitTagVersions = await GetAllTagVersionsAsync(version.ToString(), true);
		if (currentCommitTagVersions.Any())
		{
			_logger.LogInformation("Commit has the tag already");
			return;
		}

		// Check all commits for the version
		var allTagVersions = await GetAllTagVersionsAsync(version.ToString());
		if (allTagVersions.Any())
		{
			throw new Exception("Tag is used by another commit.");
		}

		var localTagResult = await ExecuteGitCommand($"tag v${version.ToString()}");
		_logger.LogInformation("Local tag result:{LocalTagResult}", localTagResult);
		var remoteTagResult = await ExecuteGitCommand($"push origin v${version}");
		_logger.LogInformation("Remote tag result:{RemoteTagResult}", remoteTagResult);
	}
	
	private Task<string> ExecuteGitCommand(string command)
	{
		var processStartInfo = new ProcessStartInfo("git", command)
		{
			RedirectStandardOutput = true
		};

		var statusProcess = Process.Start(processStartInfo);
		return statusProcess!.StandardOutput.ReadToEndAsync();
	}
}
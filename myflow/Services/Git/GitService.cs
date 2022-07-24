using System.Diagnostics;

namespace myflow.Services.Git;

public class GitService
{
	public async Task<string> GetActiveBranchAsync()
	{
		var res = await ExecuteGitCommand("symbolic-ref HEAD");
		return res.Trim();
	}

	public async Task<IEnumerable<Version>> GetAllTagVersions(string versionFilter = "*", bool specificCommit = false)
	{
		var specificCommitFilter = specificCommit ? " --points-at HEAD" : "";
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
using System.Diagnostics;

namespace myflow.Services.Git;

public class GitService
{
	public async Task<string> GetActiveBranchAsync()
	{
		var res = await ExecuteGitCommand("symbolic-ref HEAD");
		return res.Trim();
	}


	public async Task<IEnumerable<Version>> GetAllTagVersions(string versionFilter = "*", bool specificCommit = true)
	{
		var res = await ExecuteGitCommand("");


		// export const getTagVersions = async ({ exact, matchVersion } = {}) => {
		// 	return (await run(`git tag ${exact ? '--points-at HEAD' : ''} -l "v${matchVersion || '*'}"`, true))
		// 		.split(/\s+/)
		// 		.map((x) => Version.parse(x.slice(1)))
		// 		.filter((x) => x);
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
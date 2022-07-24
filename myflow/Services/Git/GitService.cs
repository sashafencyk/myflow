using System.Diagnostics;

namespace myflow.Services.Git;

public class GitService
{
	public async Task<string> GetActiveBranchAsync()
	{
		var res = await ExecuteGitCommand("symbolic-ref HEAD");
		return res.Trim();
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
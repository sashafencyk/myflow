using System.Diagnostics;

namespace myflow.Services.GitService;

public class GitService
{
	public async Task<IEnumerable<string>> GetVersions()
	{
		
	}

	private async Task<string> ExecuteGitCommand(string command)
	{
		var processStartInfo = new ProcessStartInfo("git", command)
		{
			RedirectStandardOutput = true
		};

		var statusProcess = Process.Start(processStartInfo);
		return await statusProcess!.StandardOutput.ReadToEndAsync();
	}
}
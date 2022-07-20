// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

Console.WriteLine("Hello, World!");

Console.WriteLine(await ExecuteGitCommand("tag -l"));

async Task<string> ExecuteGitCommand(string command)
{
	var processStartInfo = new ProcessStartInfo("git", command)
	{
		RedirectStandardOutput = true
	};

	var statusProcess = Process.Start(processStartInfo);
	return await statusProcess!.StandardOutput.ReadToEndAsync();
}
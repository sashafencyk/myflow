using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using myflow.Services.Git;
using myflow.Services.VersionFile;

namespace myflow.Services;

public class MyFlowService
{
	private readonly ILogger _logger;
	private readonly IConfiguration _configuration;
	private readonly GitService _gitService;
	private readonly VersionFileService _versionFileService;
	private readonly BranchResolverService _branchResolverService;

	public MyFlowService(ILogger<MyFlowService> logger, IConfiguration configuration, GitService gitService, VersionFileService versionFileService, BranchResolverService branchResolverService)
	{
		_logger = logger;
		_configuration = configuration;
		_gitService = gitService;
		_versionFileService = versionFileService;
		_branchResolverService = branchResolverService;
	}

	public async Task RunFlowAsync()
	{
		try
		{
			var fileVersion = await _versionFileService.GetVersionAsync();
			_logger.LogInformation("File version:{FileVersion}", fileVersion);
			var branch = await _branchResolverService.GetBranchAsync();
			_logger.LogInformation("Branch model: {BranchModel}", branch);
			
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while running the flow");
			throw;
		}
	}
}
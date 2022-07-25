using Microsoft.Extensions.Logging;

namespace myflow.Services;

public class DevOpsPipelineService
{
	private readonly ILogger _logger;

	public DevOpsPipelineService(ILogger<DevOpsPipelineService> logger)
	{
		_logger = logger;
	}

	public void FailPipeline(string message)
	{
		_logger.LogInformation("##vso[task.logissue type=error;]{Message}", message);
		_logger.LogInformation("##vso[task.complete result=Failed;]{Message}", message);
	}

	public void UpdateBuildNumber(string buildNumber)
	{
		_logger.LogInformation("Set BUILD_BUILDNUMBER={BuildNumber}", buildNumber);
		_logger.LogInformation("##vso[build.updatebuildnumber]{BuildNumber}", buildNumber);
	}

	public void SetVariables(IDictionary<string, string> variables)
	{
		if (!variables.Any())
			return;

		foreach (var (key, value) in variables)
		{
			_logger.LogInformation("Set {Key}={Value}", key, value);
			_logger.LogInformation("##vso[task.setvariable variable={Key};]{Value}", key, value);
		}
	}


// export const setVariable = (name, value = '') => {

// };

}
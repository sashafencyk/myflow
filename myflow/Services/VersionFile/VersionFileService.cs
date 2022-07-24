namespace myflow.Services.VersionFile;

public class VersionFileService
{
	public VersionFileService()
	{
	}

	public async Task<Version> GetVersionAsync()
	{
		var path = "version"; //todo: move to config
		if (!File.Exists(path))
		{
			throw new Exception("Version file doesnt exist");
		}
		
		var versionRaw = await File.ReadAllTextAsync(path);
		if (!Version.TryParse(versionRaw, out var version))
		{
			throw new Exception($"Version can not be resolved from path:{path}, content:{versionRaw}");
		}

		return version;
	}
}
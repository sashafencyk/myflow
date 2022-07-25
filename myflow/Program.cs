// See https://aka.ms/new-console-template for more information

using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using myflow.Services;

Console.WriteLine("Starting with args: " + string.Join(',', args));

Console.WriteLine("Starting with env variables:");
foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
	Console.WriteLine("{0}:{1}", de.Key, de.Value);

Environment.GetEnvironmentVariables();

var host = Host.CreateDefaultBuilder(args)
	.ConfigureServices((context, collection) =>
	{
		collection.AddSingleton(context.Configuration);
		collection.AddSingleton<MyFlowService>();
		collection.AddSingleton<GitService>();
		collection.AddSingleton<VersionFileService>();
		collection.AddSingleton<BranchResolverService>();
		collection.AddSingleton<DevOpsPipelineService>();
	})
	.Build();

var myFlowService = host.Services.GetRequiredService<MyFlowService>();
await myFlowService.RunFlowAsync();



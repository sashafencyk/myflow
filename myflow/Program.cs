// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using myflow.Services;
using myflow.Services.Git;
using myflow.Services.VersionFile;

var argsStr = JsonSerializer.Serialize(args);
Console.WriteLine("Starting with args: " + argsStr);

var host = Host.CreateDefaultBuilder(args)
	.ConfigureServices((context, collection) =>
	{
		collection.AddSingleton(context.Configuration);
		collection.AddSingleton<MyFlowService>();
		collection.AddSingleton<GitService>();
		collection.AddSingleton<VersionFileService>();
		collection.AddSingleton<BranchResolverService>();
	})
	.Build();

var myFlowService = host.Services.GetRequiredService<MyFlowService>();
await myFlowService.RunFlowAsync();



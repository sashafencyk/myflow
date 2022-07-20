// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

Console.WriteLine("Hello, World!");

var statusProcess = Process.Start("git", "status");
var test = await statusProcess.StandardOutput.ReadToEndAsync();
Console.WriteLine(test);
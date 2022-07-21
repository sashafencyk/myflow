// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

Console.WriteLine("Hello, World!");
Console.WriteLine(new Version(1, 0, 0));
var test = Version.Parse("1.140.11");
Console.WriteLine(test);

// Console.WriteLine(await ExecuteGitCommand("tag -l"));


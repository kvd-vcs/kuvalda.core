
var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// BUILD TASK
//////////////////////////////////////////////////////////////////////

Task("Clean").Does(() =>
{
	CleanDirectory("./artifacts");
});

Task("Build").Does(() =>
{
	
	var settings = new DotNetCoreBuildSettings
	{
		Framework = "netstandard2.0",
		Configuration = "Release",
		OutputDirectory = "./artifacts/Kuvalda.Core"
	};
	
	DotNetCoreBuild("./src/Kuvalda.Core", settings);
});

Task("PackNuget").Does(() =>
{
	CreateDirectory("./artifacts");
	
	NuGetPack("kuvalda.nuspec", new NuGetPackSettings()
	{
		Version = "0.2.2",
		BasePath = "./",
		OutputDirectory = "./artifacts/nuget/",
		NoPackageAnalysis = true
	});
});

Task("Test").Does(() =>
{
	var settings = new DotNetCoreTestSettings
	{
		Framework = "netcoreapp2.0",
		Configuration = "Release",
		OutputDirectory = "./artifacts/tests/"
	};

	DotNetCoreTest("./test/KuvaldaTests", settings);
});
 
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Travis")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("All")
	.IsDependentOn("Clean")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("PackNuget");
	
Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
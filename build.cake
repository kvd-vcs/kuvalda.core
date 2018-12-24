
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
	var settings_repo = new DotNetCoreBuildSettings
	{
		Framework = "netcoreapp2.1",
		Configuration = "Release",
		OutputDirectory = "./artifacts/Kuvalda.Repository/"
	};
	
	var settings_tree = new DotNetCoreBuildSettings
	{
		Framework = "netcoreapp2.1",
		Configuration = "Release",
		OutputDirectory = "./artifacts/Kuvalda.Tree"
	};
	
	DotNetCoreBuild("./src/Kuvalda.Repository", settings_repo);
	DotNetCoreBuild("./src/Kuvalda.Tree", settings_tree);
});

Task("PackNuget").Does(() =>
{
	CreateDirectory("./artifacts");
	
	NuGetPack("kuvalda.nuspec", new NuGetPackSettings()
	{
		Version = "0.1.0",
		BasePath = "./",
		OutputDirectory = "./artifacts/nuget/",
		NoPackageAnalysis = true
	});
});

Task("Test").Does(() =>
{
	var settings = new DotNetCoreTestSettings
	{
		Framework = "netcoreapp2.1",
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
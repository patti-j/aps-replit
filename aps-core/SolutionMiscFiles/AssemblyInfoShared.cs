using System.Reflection;

// The values in this file are shared across all Assemblies and so should 
// be omitted from the individual AssemblyInfo files of each project.

[assembly: AssemblyCopyright("Copyright © PlanetTogether 2026")]
[assembly: AssemblyCompany("PlanetTogether Inc")]
//The first two components should match the display version. The * means that the assembly will be unique for each build
//This is not used by PT code, but certain .net functions may reference this. For example the Application namespace.
//Changed the last two components from * to static to avoid a bug with Edit and Continue
[assembly: AssemblyVersion("12.3.1.28")]

//The package version that packages will use to synchronize references.
[assembly: PT.Common.Attributes.Assembly.PackageFrameworkVersion("12.3.1")]

//The version of the package framework that the packages are meant to reference.
//The system will use this version to validate the package can be loaded.
[assembly: PT.Common.Attributes.Assembly.TargetPackageFrameworkVersion("12.3.1")]
using System.Reflection;

// The values in this file are shared across all Assemblies and so should 
// be omitted from the individual AssemblyInfo files of each project.

[assembly: AssemblyCopyright("Copyright © PlanetTogether 2021")]
[assembly: AssemblyCompany("PlanetTogether Inc")]
//The first two components should match the display version. The * means that the assembly will be unique for each build
//This is not used by PT code, but certain .net functions may reference this. For example the Application namespace.
//Changed the last two components from * to static to avoid a bug with Edit and Continue
[assembly: AssemblyVersion("12.0.0.0")]
//Used to validate assemblies and display version number.
//Must be three or four components. (Due to ServerManager, these also must be integers)
//The first component is the major version and should only be changed when a significant major version is released.
//The second component is the compatibility number. It should be updated when serialization number changes, import database is changed
// or publish database has changed.
//The third component is the minor version number and should be updated when creating a new build a customer might use.
//When updating the major version number, reset the other two components to 0. When updating the compatibility number, reset the minor number to 0
[assembly: PT.Common.Attributes.Assembly.AssemblyDisplayVersion("12.0.0")]
//Used to validate build time for use with license key checks.

//[assembly: ComVisible(false)]

/*Serialization Change Log. This is used to keep track of whether the minor version number needs to be increased 


11.27: 676

*/
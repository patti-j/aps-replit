namespace PT.PackageDefinitions.PackageInterfaces;

public interface IPermissionModule
{
    List<IUserPermissionElement> GetUserPermissions()
    {
        return new List<IUserPermissionElement>();
    }

    List<IUserPermissionElement> GetPlantPermissions()
    {
        return new List<IUserPermissionElement>();
    }
}

public interface IPermissionValidationModule
{
    List<IPermissionsValidationElement> GetPermissionValidationElements();
}

public interface ILicenseValidationModule
{
    List<ILicenseValidationElement> GetLicenseValidationElements();
}
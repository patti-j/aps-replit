using PT.APSCommon;
using PT.Common.File;
using PT.Transmissions;
using System.Data;

namespace PT.PlanetTogetherAPI.Importing;

public interface IImporter
{
    /// <summary>
    /// Creates a connection of the correct type as determined by the ConnectionDef.
    /// </summary>
    IDbConnection GetConnection();

    IDbCommand GetCommand(IDbConnection connection, string sqlText);
    ApplicationExceptionList RunImport(bool testonly, Type objectType, BaseId instigator, BaseId a_targetScenarioId, ScenarioExceptionInfo a_sei, int a_targetConfigId);
    ApplicationExceptionList RunImport(bool testonly, BaseId instigator, BaseId a_targetScenarioId, ScenarioExceptionInfo a_sei, int a_targetConfigId);
    
    // Not currently in use - v11 holdovers?
    //PackageImportSettings GetPackageSettings();
    //ApplicationExceptionList RunPackageImport(bool a_testOnly, Type a_objectType, BaseId a_instigator, BaseId a_targetScenarioId);
}
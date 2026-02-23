using PT.APSCommon;
using PT.Transmissions;

namespace PT.PackageDefinitionsUI;

public interface IScenarioUndoInfo
{
    event Action<bool, string> UndoStart;
    /// <summary>
    /// Event raised when an undo process is complete. The bool flag indicates whether the undo completed successfully or not
    /// </summary>
    /// <remarks>True if it completed successfully, false if otherwise </remarks>
    event Action<bool> UndoComplete;
    event Action<BaseId, Transmission> UndoableActionProcessed;
}
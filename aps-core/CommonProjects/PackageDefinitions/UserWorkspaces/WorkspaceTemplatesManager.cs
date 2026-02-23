using System.IO;

using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.PackageDefinitions.UserWorkspaces;

public class WorkspaceTemplatesManager : IPTSerializable
{
    #region IPTSerializable
    private const int UNIQUE_ID = 1012;
    public int UniqueId => UNIQUE_ID;

    public WorkspaceTemplatesManager(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12036)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string workspaceName);
                a_reader.Read(out byte[] workspaceContainer);
                m_userWorkspaces.Add(workspaceName, workspaceContainer);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out byte[] workspaceContainer);
                m_defaultWorkspaceCollection.Add(workspaceContainer);
            }
        }
        else
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string workspaceName);
                a_reader.Read(out byte[] workspaceContainer);
                m_userWorkspaces.Add(workspaceName, workspaceContainer);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_userWorkspaces.Count);
        foreach (KeyValuePair<string, byte[]> w in m_userWorkspaces)
        {
            a_writer.Write(w.Key);
            a_writer.Write(w.Value);
        }

        a_writer.Write(m_defaultWorkspaceCollection.Count);
        foreach (byte[] workspace in m_defaultWorkspaceCollection)
        {
            a_writer.Write(workspace);
        }
    }
    #endregion

    public WorkspaceTemplatesManager() { }

    private readonly Dictionary<string, byte[]> m_userWorkspaces = new ();
    private List<byte[]> m_defaultWorkspaceCollection = new ();

    public void LoadInstanceWorkspaces(string a_defaultWorkspacesPath, ICommonLogger a_errorReporter)
    {
        m_defaultWorkspaceCollection = new List<byte[]>();
        string[] files = Directory.GetFiles(a_defaultWorkspacesPath, "*.ptwst", SearchOption.TopDirectoryOnly);
        foreach (string workspaceFile in files)
        {
            try
            {
                byte[] compressedContainerBytes = File.ReadAllBytes(workspaceFile);
                m_defaultWorkspaceCollection.Add(compressedContainerBytes);
            }
            catch (Exception e)
            {
                a_errorReporter.LogException(new PTHandleableException("Error loading instance workspace", e), null);
            }
        }
    }

    /// <summary>
    /// Represents the list of shared workspace containers.
    /// </summary>
    public Dictionary<string, byte[]> UserWorkspaces => m_userWorkspaces;

    public IEnumerable<byte[]> DefaultWorkspaceCollection => m_defaultWorkspaceCollection;

    public void AddSharedWorkspace(string a_name, byte[] a_container)
    {
        if (m_userWorkspaces.TryGetValue(a_name, out byte[] workspaceBytes))
        {
            throw new PTValidationException($"A workspace named '{a_name}' already exists. A different name must be selected.");
        }

        m_userWorkspaces.Add(a_name, a_container);
    }

    public void ReplaceSharedWorkspace(string a_name, byte[] a_container)
    {
        if (!m_userWorkspaces.TryGetValue(a_name, out byte[] workspaceBytes))
        {
            throw new PTValidationException($"A workspace named '{a_name}' could not be found. The workspace overwrite cannot be completed.");
        }

        m_userWorkspaces[a_name] = a_container;
    }

    public void DeleteSharedWorkspace(string a_name, byte[] a_container)
    {
        if (!m_userWorkspaces.TryGetValue(a_name, out byte[] workspaceBytes))
        {
            throw new PTValidationException($"A workspace named '{a_name}' could not be found. The workspace removal cannot be completed.");
        }

        m_userWorkspaces.Remove(a_name);
    }

    public Dictionary<string, byte[]> GetWorkspaceDictionary()
    {
        return m_userWorkspaces;
    }
}
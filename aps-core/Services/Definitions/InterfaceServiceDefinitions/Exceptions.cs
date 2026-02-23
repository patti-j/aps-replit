using System.Runtime.Serialization;

using PT.Common.Exceptions;
using PT.Common.Localization;

namespace PT.ImportDefintions;

public class ImporterException : ApplicationException, IPTSerializable
{
    public const int UNIQUE_ID = 361;

    #region IPTSerializable Members
    public ImporterException(IReader reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public virtual void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ImporterException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(Localizer.GetErrorString(a_message, a_stringParameters, a_appendHelpUrl)) { }

    public ImporterException() { }

    public ImporterException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

public class ImporterConfigException : ImporterException, ISerializable, IPTSerializable
{
    public new const int UNIQUE_ID = 362;

    #region IPTSerializable Members
    public ImporterConfigException(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out sectionName);
            reader.Read(out workingDirectory);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public string sectionName;
    public string workingDirectory;

    public ImporterConfigException(string sectionName, string workingDirectory, string message)
        : base(message)
    {
        this.sectionName = sectionName;
        this.workingDirectory = workingDirectory;
    }

    public ImporterConfigException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        sectionName = info.GetString("sectionName");
        workingDirectory = info.GetString("workingDirectory");
    }

    #region ISerializable Members
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("sectionName", sectionName, sectionName.GetType());
        info.AddValue("workingDirectory", workingDirectory, workingDirectory.GetType());
        base.GetObjectData(info, context);
    }
    #endregion
}

public class ImporterConnectionException : ImporterException, ISerializable, IPTSerializable
{
    public new const int UNIQUE_ID = 363;

    #region IPTSerializable Members
    public ImporterConnectionException(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out connectionString);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(connectionString);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public string connectionString;

    public ImporterConnectionException(string connectionString, string message)
        : base(message)
    {
        this.connectionString = connectionString;
    }

    public ImporterConnectionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        connectionString = info.GetString("connectionString");
    }

    #region ISerializable Members
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("connectionString", connectionString, connectionString.GetType());
        base.GetObjectData(info, context);
    }
    #endregion
}

public class ImporterCommandException : ImporterException, ISerializable, IPTSerializable
{
    public new const int UNIQUE_ID = 364;

    #region IPTSerializable Members
    public ImporterCommandException(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out commandText);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(commandText);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public string commandText;

    public ImporterCommandException(string commandText, string message)
        : base(message)
    {
        this.commandText = commandText;
    }

    public ImporterCommandException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        commandText = (string)info.GetValue("commandText", typeof(string));
    }

    #region ISerializable Members
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("commandText", commandText, commandText.GetType());
        base.GetObjectData(info, context);
    }
    #endregion
}

public class ImporterInvalidConnectionException : ImporterException, ISerializable, IPTSerializable
{
    public new const int UNIQUE_ID = 366;

    #region IPTSerializable Members
    public ImporterInvalidConnectionException(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out connectionName);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(connectionName);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public string connectionName;

    public ImporterInvalidConnectionException(string connectionName, string message)
        : base(message)
    {
        this.connectionName = connectionName;
    }

    #region ISerializable Members
    public ImporterInvalidConnectionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        connectionName = info.GetString("connectionName");
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("connectionName", connectionName, connectionName.GetType());
        base.GetObjectData(info, context);
    }
    #endregion
}

public class SettingsSaveException : ApplicationException, ISerializable, IPTSerializable
{
    public const int UNIQUE_ID = 367;

    #region IPTSerializable Members
    public SettingsSaveException(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out targetFile);
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif

        writer.Write(targetFile);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public string targetFile;

    public SettingsSaveException(string targetFile, string message)
        : base(message)
    {
        this.targetFile = targetFile;
    }

    public SettingsSaveException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        targetFile = info.GetString("targetFile");
    }

    #region ISerializable Members
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("targetFile", targetFile, targetFile.GetType());
        base.GetObjectData(info, context);
    }
    #endregion
}

public class SettingsSaverException : ApplicationException, IPTSerializable
{
    public const int UNIQUE_ID = 368;

    #region IPTSerializable Members
    public SettingsSaverException(IReader reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public SettingsSaverException(string message)
        : base(message) { }

    public SettingsSaverException(SerializationInfo info, StreamingContext context) { }
}

/// <summary>
/// Exception thrown if the interface is already locked when an access is attempted.
/// </summary>
public class InterfaceLockedException : PTHandleableException, ISerializable, IPTSerializable
{
    public const int UNIQUE_ID = 369;

    #region IPTSerializable Members
    public InterfaceLockedException(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out lockUser);

            reader.Read(out lockTime);
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif

        writer.Write(lockUser);

        writer.Write(lockTime);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public DateTime lockTime;
    public string lockUser; //Time the interface was locked

    public InterfaceLockedException(string userName, DateTime lockTime)
        : base(string.Format("User {0} has the Interface locked so it cannot be accessed at this time.  The Interface was locked at {1}.", userName, lockTime))
    {
        this.lockTime = lockTime;
        lockUser = userName;
    }

    #region ISerializable Members
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("lockTime", lockTime, lockTime.GetType());
        info.AddValue("lockUser", lockUser, lockUser.GetType());
        base.GetObjectData(info, context);
    }
    #endregion
}

/// <summary>
/// General exception thrown through the interface.
/// </summary>
public class ImportException : PTHandleableException, IPTSerializable, ISerializable
{
    public const int UNIQUE_ID = 370;

    #region IPTSerializable Members
    public ImportException(IReader reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public ImportException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(message, a_stringParameters, a_appendHelpUrl) { }

    public ImportException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
}

public class ImportApiException : PTHandleableException, IPTSerializable, ISerializable
{
    public const int UNIQUE_ID = 371;

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
    }

    public ImportApiException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(message, a_stringParameters, a_appendHelpUrl) { }

    public int UniqueId => UNIQUE_ID;
}
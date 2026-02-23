using System.Data;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// This is the base class of all manufacturing system transmissions.
/// </summary>
public class ERPTransmission : ScenarioBaseT, IValidate, IPTSerializable
{
    public const int UNIQUE_ID = 211;

    #region IPTSerializable Members
    public ERPTransmission(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 129)
        {
            a_reader.Read(out userConnectionNbr);
        }

        #region 21
        else if (a_reader.VersionNumber >= 21)
        {
            ulongUserConnectionNbrReader(a_reader);
        }
        #endregion

        #region Version 20
        else if (a_reader.VersionNumber >= 20)
        {
            ulongUserConnectionNbrReader(a_reader);

            a_reader.Read(out int directInstigatorTemp);
        }
        #endregion

        #region Version 1
        else if (a_reader.VersionNumber >= 1)
        {
            ulongUserConnectionNbrReader(a_reader);

            new BaseId(a_reader);
        }
        #endregion
    }

    /// <summary>
    /// For backwards compatibility with the ulong version of user connections.
    /// </summary>
    /// <param name="reader"></param>
    private void ulongUserConnectionNbrReader(IReader reader)
    {
        ulong tempUserConnectionNbr;
        reader.Read(out tempUserConnectionNbr);
        userConnectionNbr = (int)tempUserConnectionNbr;
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(userConnectionNbr);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private int userConnectionNbr;

    /// <summary>
    /// This is an optional value. You may use it to specify the
    /// Planet Together connection number of the user who caused
    /// this transmission to be sent. If you set it,
    /// the client of Planet Together may use it to display error
    /// messages on the Planet Together user's window.
    /// This value may be different from the ConnectionNbr because
    /// the ConnectionNbr refers to the actual connection on which
    /// the transmission was broadcast, for instance a connection
    /// number established from the Planet Together Interface
    /// service.
    /// </summary>

    public ERPTransmission()
    {
        Instigator = BaseId.ERP_ID;
        LogErrorToUsualLogFile = true;
    }

    void IValidate.Validate() { }

    public static void FillDataTable(DataTable a_table, IDbCommand a_cmd, string a_transmissionType)
    {
        System.Data.Common.DbDataAdapter adapter;
        a_cmd.CommandTimeout = 0; //wait till done

        if (a_cmd is Microsoft.Data.SqlClient.SqlCommand)
        {
            adapter = new Microsoft.Data.SqlClient.SqlDataAdapter((Microsoft.Data.SqlClient.SqlCommand)a_cmd);
        }
        else
        {
            throw new PTValidationException("2046", new object[] { a_cmd.GetType().ToString() });
        }

        if (a_table.DataSet != null) //Some transmissions just use one Table and have no DataSet.
        {
            a_table.DataSet.EnforceConstraints = false; //for better performance
        }

        a_table.BeginLoadData(); //turn off for better performance
        try
        {
            adapter.Fill(a_table);
        }
        catch (Exception e)
        {
            throw new PTException("4006", e, new object[] { a_table.TableName, a_cmd.CommandText, e.Message });
        }

        a_table.EndLoadData();

        //Make sure all of the columns that don't allow null are not null
        for (int i = 0; i < a_table.Rows.Count; i++)
        {
            DataRow row = a_table.Rows[i];
            for (int colI = 0; colI < a_table.Columns.Count; colI++)
            {
                DataColumn col = a_table.Columns[colI];
                if (!col.AllowDBNull && row[colI] is DBNull)
                {
                    throw new PTValidationException("2048", new object[] { col.ColumnName, a_table.TableName, a_transmissionType });
                }
            }
        }
    }
}
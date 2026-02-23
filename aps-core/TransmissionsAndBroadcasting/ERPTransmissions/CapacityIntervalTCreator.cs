using System.Data;

using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// Creates CapacityIntervalT transmissions using DataReaders as input.
/// </summary>
public class CapacityIntervalTCreator
{
    public class DbCapacityInterval : CapacityInterval, IPTSerializable
    {
        public new const int UNIQUE_ID = 449;

        #region PT Serialization
        public DbCapacityInterval(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 1) { }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public DbCapacityInterval(IDataReader reader)
        {
            PropertySetter.SetProperties(reader, this);

            Validate();
        }
    }

    public static CapacityIntervalT MakeTransmission(IDataReader reader, bool autoDelete, ref ApplicationExceptionList errors)
    {
        CapacityIntervalT t = new ();
        int rowIndex = -1;
        while (reader.Read())
        {
            rowIndex++;

            try
            {
                DbCapacityInterval dbCI = new (reader);
                //Don't have plant, dep, res ids yet so just send blank lists.
                CapacityIntervalT.CapacityIntervalDef capDef = new (dbCI.ExternalId, dbCI.Name, dbCI);
                t.Add(capDef);
            }
            catch (PTObjectBase.PTObjectBaseCreationException e)
            {
                errors.Add(new PTObjectBase.PTObjectCreationException(e.Message, typeof(CapacityInterval), e.propertyName, e.propertyIndex, e.expectedType, e.actualValue, e.actualType, rowIndex), null);
            }
            catch (ValidationException e)
            {
                errors.Add(new PTObjectBase.RowValidationException(e.Message, typeof(CapacityInterval), rowIndex), null);
            }
        }

        reader.Close();
        t.AutoDeleteMode = autoDelete;
        return t;
    }
}
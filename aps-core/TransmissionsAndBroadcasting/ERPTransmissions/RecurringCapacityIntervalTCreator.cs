using System.Data;

using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// Creates RecurringCapacityIntervalT transmissions using DataReaders as input.
/// </summary>
public class RecurringCapacityIntervalTCreator
{
    [Serializable]
    public class DbRecurringCapacityInterval : RecurringCapacityInterval, IPTSerializable
    {
        public new const int UNIQUE_ID = 450;

        #region PT Serialization
        public DbRecurringCapacityInterval(IReader reader)
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

        public DbRecurringCapacityInterval(IDataReader reader)
        {
            PropertySetter.SetProperties(reader, this);

            Validate();
        }
    }

    public static RecurringCapacityIntervalT MakeTransmission(IDataReader reader, bool autoDelete, ref ApplicationExceptionList errors)
    {
        RecurringCapacityIntervalT t = new ();
        int rowIndex = -1;
        while (reader.Read())
        {
            rowIndex++;

            try
            {
                DbRecurringCapacityInterval dbrci = new (reader);
                // SGS                 t.Add((RecurringCapacityIntervalT.RecurringCapacityIntervalDef)dbrci);
            }
            catch (PTObjectBase.PTObjectBaseCreationException e)
            {
                errors.Add(new PTObjectBase.PTObjectCreationException(e.Message, typeof(RecurringCapacityIntervalDef), e.propertyName, e.propertyIndex, e.expectedType, e.actualValue, e.actualType, rowIndex), null);
            }
            catch (ValidationException e)
            {
                errors.Add(new PTObjectBase.RowValidationException(e.Message, typeof(RecurringCapacityIntervalDef), rowIndex), null);
            }
        }

        reader.Close();
        t.AutoDeleteMode = autoDelete;
        return t;
    }
}
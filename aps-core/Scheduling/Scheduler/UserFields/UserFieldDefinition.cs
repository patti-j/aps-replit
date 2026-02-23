using PT.APSCommon;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using PT.ERPTransmissions;

using static PT.SchedulerDefinitions.UserField;

namespace PT.Scheduler
{
    public class UserFieldDefinition : BaseObject, ICloneable, IUserFieldDefinition
    {

        #region IPTSerializable Members
        public new const int UNIQUE_ID = 858;

        public UserFieldDefinition(IReader a_reader) : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12506)
            {
                m_bools = new(a_reader);
                a_reader.Read(out int udfDataType);
                m_udfDataType = (UDFTypes)udfDataType;
                a_reader.Read(out int udfObjectType);
                m_objectType = (EUDFObjectType)udfObjectType;

                a_reader.Read(out bool hasDefaultValue);
                if (hasDefaultValue)
                {
                    a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_defaultValue);
                }
            }
            else if (a_reader.VersionNumber >= 12503)
            {
                m_bools = new(a_reader);
                a_reader.Read(out int udfDataType);
                m_udfDataType = (UDFTypes)udfDataType;
                a_reader.Read(out int udfObjectType);
                m_objectType = (EUDFObjectType)udfObjectType;
                a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_defaultValue);
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            
            m_bools.Serialize(a_writer);
            a_writer.Write((int)m_udfDataType);
            a_writer.Write((int)m_objectType);

            bool hasDefaultValue = m_defaultValue != null;
            a_writer.Write(hasDefaultValue);
            if (hasDefaultValue)
            {
                a_writer.WriteBoxedPrimitiveAndCommonSystemStructs(m_defaultValue);
            }
        }

        #endregion

        #region Construction
        public UserFieldDefinition(BaseId a_id, string a_externalId) : base(a_id)
        {
            ExternalId = a_externalId;
            Name = a_externalId;
            DefaultValue = string.Empty;
        }

        public UserFieldDefinition(BaseId a_id, PTObjectBase a_ptObject) : base(a_id, a_ptObject)
        {
        }

        public UserFieldDefinition(UserFieldDefinition a_source, BaseId a_newId) : base(a_newId)
        {
            Name = a_source.Name;
            UDFDataType = a_source.UDFDataType;
            ObjectType = a_source.ObjectType;
            DefaultValue = a_source.DefaultValue;
            DisplayInUI = a_source.DisplayInUI;
            Publish = a_source.Publish;
            KeepValue = a_source.KeepValue;
        }

        #endregion

        #region Definitions

        UDFTypes m_udfDataType;
        public UDFTypes UDFDataType
        {
            get { return m_udfDataType; }
            set { m_udfDataType = value; }
        }

        public Type DataType
        {
            get { return GetTypeFromUDFType(UDFDataType); }
        }

        /// <summary>
        /// The object type that the UDF belongs to (I.e: Job, Operation, etc.)
        /// </summary>
        private EUDFObjectType m_objectType;
        public EUDFObjectType ObjectType
        {
            get { return m_objectType; }
            set { m_objectType = value; }
        }

        /// <summary>
        /// The default value of the UDF.
        /// </summary>
        private object m_defaultValue;
        public object DefaultValue
        {
            get { return m_defaultValue; }
            set { m_defaultValue = value; }
        }

        private const int c_displayInUIIdx = 0;
        private const int c_publishIdx = 1;
        private const int c_keepValueIdx = 2;
        private BoolVector32 m_bools = new BoolVector32();

        public bool DisplayInUI
        {
            get => m_bools[c_displayInUIIdx];
            set => m_bools[c_displayInUIIdx] = value;
        }

        public bool Publish
        {
            get => m_bools[c_publishIdx];
            set => m_bools[c_publishIdx] = value;
        }

        public bool KeepValue
        {
            get => m_bools[c_keepValueIdx];
            set => m_bools[c_keepValueIdx] = value;
        }
        #endregion

        #region Cloning
        public UserFieldDefinition Clone()
        {
            return (UserFieldDefinition)MemberwiseClone();
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Helpers
        UDFTypes GetTypeFromOldType(Type aOldType)
        {
            if (aOldType == typeof(string))
                return UDFTypes.String;
            else if (aOldType == typeof(int))
                return UDFTypes.Integer;
            else if (aOldType == typeof(long))
                return UDFTypes.Long;
            else if (aOldType == typeof(decimal))
                return UDFTypes.Decimal;
            else if (aOldType == typeof(DateTime))
                return UDFTypes.DateTime;
            else if (aOldType == typeof(bool))
                return UDFTypes.Boolean;
            else if (aOldType == typeof(double))
                return UDFTypes.Double;
            else if (aOldType == typeof(TimeSpan))
                return UDFTypes.TimeSpan;
            else
                throw new APSCommon.PTValidationException("2038", new object[] { aOldType });
        }

        public static Type GetTypeFromUDFType(UDFTypes a_udfType)
        {
            switch (a_udfType)
            {
                case UDFTypes.String:
                case UDFTypes.Hyperlink:
                    return typeof(string);
                case UDFTypes.Integer:
                    return typeof(int);
                case UDFTypes.Long:
                    return typeof(long);
                case UDFTypes.Decimal:
                    return typeof(decimal);
                case UDFTypes.DateTime:
                    return typeof(DateTime);
                case UDFTypes.Boolean:
                    return typeof(bool);
                case UDFTypes.Double:
                    return typeof(double);
                default:
                    return typeof(TimeSpan);
            }
        }

        public string GetColNameForUserField(UserField uf)
        {
            return Name + UserFieldSuffix;
        }

        public static string UserFieldSuffix = " UdfDef";

        /// <summary>
        /// Separates the Name, DataType, Value in a single User Field.
        /// </summary>
        public static string USER_FIELDS_SEP_CHAR
        {
            get { return "~"; }
        }

        /// <summary>
        /// Separates indvidual User Fields.
        /// </summary>
        public static string USER_FIELDS_INTERFIELD_SEP_CHAR
        {
            get { return ";"; }
        }

        /// <summary>
        /// Get the string that is used to pass the user fields in a transmission.
        /// </summary>
        public static string GetUserFieldImportString(string a_fieldName, UDFTypes a_dataType, object a_value, bool a_display = true)
        {
            return String.Format("{1}{0}{2}{0}{3}{0}{4}", USER_FIELDS_SEP_CHAR, a_fieldName, a_value == null ? "" : a_value.ToString(), Enum.GetName(typeof(UDFTypes), a_dataType), a_display ? "TRUE" : "FALSE");
        }

        /// <summary>
        /// Get the string that is used to pass the user fields in a transmission.
        /// </summary>
        public string GetUserFieldImportString()
        {
            return GetUserFieldImportString(Name, UDFDataType, null);
        }
        #endregion

        internal void Update(UserFieldDefinitionT.UserFieldDefinition a_sourceUserFieldDefinition, UserFieldDefinitionT a_userFieldDefinitionT)
        {
            base.Update(a_sourceUserFieldDefinition, a_userFieldDefinitionT);
            Name = a_sourceUserFieldDefinition.Name;
            UDFDataType = a_sourceUserFieldDefinition.UDFType;
            ObjectType = a_sourceUserFieldDefinition.ObjectType;
            DefaultValue = a_sourceUserFieldDefinition.DefaultValue;
            DisplayInUI = a_sourceUserFieldDefinition.DisplayInUI;
            Publish = a_sourceUserFieldDefinition.Publish;
        }

        //TODO: Implement this when implementing the publish data table
        /*
            public void PtDbPopulate(ref PtDbDataSet a_dataSet, PTDatabaseHelper a_dbHelper)
            {
                a_dataSet.Customers.AddCustomersRow(
                    a_dbHelper.AdjustPublishTime(PTDateTime.UtcNow).ToDateTime(),
                    Id.ToBaseType(),
                    AbcCode,
                    ColorUtils.ConvertColorToHexString(ColorCode),
                    Priority,
                    GroupCode,
                    Region,
                    Name,
                    CustomerType.ToString());
            }
         */

        public override string DefaultNamePrefix => "User Field Definition";
    }
}

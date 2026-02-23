using PT.Transmissions;
using System.Data;

using PT.APSCommon;
using PT.Transmissions.User;

namespace PT.ERPTransmissions
{
    public class UserFieldDefinitionT : ERPMaintenanceTransmission<UserFieldDefinitionT.UserFieldDefinition>
    {
        public override string Description => "User Field updated";
        public new const int UNIQUE_ID = 859;

        #region IPTSerializable Members
        public UserFieldDefinitionT(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12106)
            {
                a_reader.Read(out int count);
                for (int udfI = 0; udfI < count; udfI++)
                {
                    Add(new UserFieldDefinitionT.UserFieldDefinition(a_reader));
                }
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            a_writer.Write(Count);
            for (int i = 0; i < this.Count; i++)
            {
                this[i].Serialize(a_writer);
            }
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public UserFieldDefinitionT() { }

        public new UserFieldDefinition this[int i]
        {
            get
            {
                return Nodes[i];
            }
        }

        public void Fill(IDbCommand a_userFieldCmd)
        {
            PtImportDataSet ds = new();
            base.FillTable(ds.UserFieldDefinitions, a_userFieldCmd);

            Fill(ds);
        }

        private void Fill(PtImportDataSet a_ds)
        {
            foreach (PtImportDataSet.UserFieldDefinitionsRow userFieldRow in a_ds.UserFieldDefinitions)
            {
                Add(new UserFieldDefinition(userFieldRow));
            }
        }

        public class UserFieldDefinition : PTObjectBase, IPTSerializable
        {
            #region IPTSerializable Members
            public const int UNIQUE_ID = 860;


            public UserFieldDefinition(IReader a_reader) : base(a_reader)
            {
                if (a_reader.VersionNumber >= 12503)
                {
                    m_setBools = new BoolVector32(a_reader);
                    m_bools = new BoolVector32(a_reader);
                    a_reader.Read(out int udfType);
                    m_udfType = (SchedulerDefinitions.UserField.UDFTypes)udfType;
                    a_reader.Read(out int objectType);
                    m_objectType = (SchedulerDefinitions.UserField.EUDFObjectType)objectType;
                    a_reader.Read(out bool hasDefaultValue);
                    if (hasDefaultValue)
                    {
                        a_reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_defaultValue);
                    }
                }
            }

            public override void Serialize(IWriter a_writer)
            {
                base.Serialize(a_writer);
                m_setBools.Serialize(a_writer);
                m_bools.Serialize(a_writer);
                a_writer.Write((int)m_udfType);
                a_writer.Write((int)m_objectType);
                a_writer.Write(m_defaultValue != null);
                if (m_defaultValue != null)
                {
                    a_writer.WriteBoxedPrimitiveAndCommonSystemStructs(m_defaultValue);
                }
            }

            public int UniqueId => UNIQUE_ID;
            #endregion

            public UserFieldDefinition()
            {

            }

            public UserFieldDefinition(PtImportDataSet.UserFieldDefinitionsRow a_userFieldRow) : base(a_userFieldRow.ExternalId, a_userFieldRow.Name)
            {
                if (!a_userFieldRow.IsDescriptionNull())
                {
                    Description = a_userFieldRow.Description;
                }

                if (!a_userFieldRow.IsNotesNull())
                {
                    Notes = a_userFieldRow.Notes;
                }

                try
                {
                    UDFType = (SchedulerDefinitions.UserField.UDFTypes)Enum.Parse(typeof(SchedulerDefinitions.UserField.UDFTypes), a_userFieldRow.UDFType);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854", err, false, new object[]
                    {
                        a_userFieldRow.UDFType, "UserField", "UDFType",
                        string.Join(", ", Enum.GetNames(typeof(SchedulerDefinitions.UserField.UDFTypes)))
                    });
                }

                try
                {
                    ObjectType = (SchedulerDefinitions.UserField.EUDFObjectType)Enum.Parse(typeof(SchedulerDefinitions.UserField.EUDFObjectType), a_userFieldRow.ObjectType);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854", err, false, new object[]
                    {
                        a_userFieldRow.ObjectType, "UserField", "ObjectType",
                        string.Join(", ", Enum.GetNames(typeof(SchedulerDefinitions.UserField.EUDFObjectType)))
                    });
                }

                if (!a_userFieldRow.IsDefaultValueNull())
                {
                    DefaultValue = ParseDefaultValue(a_userFieldRow);
                }

                if (!a_userFieldRow.IsDisplayInUINull())
                {
                    DisplayInUI = a_userFieldRow.DisplayInUI;
                }
                else
                {
                    DisplayInUI = true;
                }

                if (!a_userFieldRow.IsPublishNull())
                {
                    Publish = a_userFieldRow.Publish;
                }
                else
                {
                    Publish = true;
                }
            }

            private object ParseDefaultValue(PtImportDataSet.UserFieldDefinitionsRow a_udfRow)
            {
                object dataValue = null;
                try
                {
                    switch (UDFType)
                    {
                        case SchedulerDefinitions.UserField.UDFTypes.String:
                        case SchedulerDefinitions.UserField.UDFTypes.Hyperlink:
                            dataValue = a_udfRow.DefaultValue;
                            break;
                        case SchedulerDefinitions.UserField.UDFTypes.Integer:
                            int.TryParse(a_udfRow.DefaultValue, out int intVal);
                            dataValue = intVal;
                            break;
                        case SchedulerDefinitions.UserField.UDFTypes.Long:
                            long.TryParse(a_udfRow.DefaultValue, out long longVal);
                            dataValue = longVal;
                            break;
                        case SchedulerDefinitions.UserField.UDFTypes.Double:
                            double.TryParse(a_udfRow.DefaultValue, out double dblValue);
                            dataValue = dblValue;
                            break;
                        case SchedulerDefinitions.UserField.UDFTypes.Decimal:
                            decimal.TryParse(a_udfRow.DefaultValue, out decimal decValue);
                            dataValue = decValue;
                            break;
                        case SchedulerDefinitions.UserField.UDFTypes.Boolean:
                            Boolean.TryParse(a_udfRow.DefaultValue, out Boolean boolValue);
                            dataValue = boolValue;
                            break;
                        case SchedulerDefinitions.UserField.UDFTypes.DateTime:
                            DateTime.TryParse(a_udfRow.DefaultValue, out DateTime dtValue);
                            dataValue = dtValue;
                            break;
                        case SchedulerDefinitions.UserField.UDFTypes.TimeSpan:
                            TimeSpan.TryParse(a_udfRow.DefaultValue, out TimeSpan tsValue);
                            dataValue = tsValue;
                            break;
                    }
                    return dataValue;
                }
                catch (Exception e)
                {
                    throw new APSCommon.PTValidationException("2778", new object[] { a_udfRow.ExternalId, e.Message });
                }
            }

            public UserFieldDefinition(string a_externalId, string a_name, string a_description, string a_notes, string a_userFields) : base(a_externalId, a_name, a_description, a_notes, a_userFields)
            {
            }

            #region Properties
            private BoolVector32 m_bools = new();
            private const int c_displayInUIIdx = 0;
            private const int c_publishIdx = 1;

            private BoolVector32 m_setBools = new();
            private const int c_nameIsSetIdx = 0;
            private const int c_descriptionIsSetIdx = 1;
            private const int c_notesIsSetIdx = 2;
            private const int c_udfTypeIsSetIdx = 3;
            private const int c_objectTypeIsSetIdx = 4;
            private const int c_defaultValueIsSetIdx = 5;
            private const int c_displayInUIIsSetIdx = 6;
            private const int c_publishIsSetIdx = 7;

            private string m_description;
            public string Description
            {
                get { return m_description; }
                set
                {
                    m_description = value;
                    m_setBools[c_descriptionIsSetIdx] = true;
                }
            }
            public bool DescriptionSet => m_setBools[c_descriptionIsSetIdx];

            private string m_notes;
            public string Notes
            {
                get { return m_notes; }
                set
                {
                    m_notes = value;
                    m_setBools[c_notesIsSetIdx] = true;
                }
            }
            public bool NotesSet => m_setBools[c_notesIsSetIdx];

            private SchedulerDefinitions.UserField.UDFTypes m_udfType = SchedulerDefinitions.UserField.UDFTypes.String;
            public SchedulerDefinitions.UserField.UDFTypes UDFType
            {
                get => m_udfType;
                set
                {
                    m_udfType = value;
                    m_setBools[c_udfTypeIsSetIdx] = true;
                }
            }
            public bool UDFTypeSet => m_setBools[c_udfTypeIsSetIdx];

            private SchedulerDefinitions.UserField.EUDFObjectType m_objectType;
            public SchedulerDefinitions.UserField.EUDFObjectType ObjectType
            {
                get => m_objectType;
                set
                {
                    m_objectType = value;
                    m_setBools[c_objectTypeIsSetIdx] = true;
                }
            }
            public bool ObjectTypeSet => m_setBools[c_objectTypeIsSetIdx];

            private object m_defaultValue;
            public object DefaultValue
            {
                get => m_defaultValue;
                set
                {
                    m_defaultValue = value;
                    m_setBools[c_defaultValueIsSetIdx] = true;
                }
            }
            public bool DefaultValueSet => m_setBools[c_defaultValueIsSetIdx];

            private bool m_displayInUI = true;
            public bool DisplayInUI
            {
                get => m_bools[c_displayInUIIdx];
                set
                {
                    m_bools[c_displayInUIIdx] = value;
                    m_setBools[c_displayInUIIdx] = true;
                }
            }
            public bool DisplayInUISet => m_setBools[c_displayInUIIsSetIdx];

            private bool m_publish = true;
            public bool Publish
            {
                get => m_bools[c_publishIdx];
                set
                {
                    m_bools[c_publishIdx] = value;
                    m_setBools[c_publishIsSetIdx] = true;
                }
            }
            public bool PublishSet => m_setBools[c_publishIsSetIdx];
            #endregion
        }
    }
}

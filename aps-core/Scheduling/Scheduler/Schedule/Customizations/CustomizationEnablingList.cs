using PT.APSCommon.Collections;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.Schedule.Customizations;

/// <summary>
/// Stores the Enabled/Disabled state of the Scheduler Customizations.
/// </summary>
public class CustomizationEnablingList : CustomSortedList<CustomizationEnablingList.CustomizationEnabling>, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 722;

    public new int UniqueId => UNIQUE_ID;

    public CustomizationEnablingList()
        : base(new CustomizationEnablingComparer()) { }

    public CustomizationEnablingList(IReader reader)
        : base(reader, new CustomizationEnablingComparer())
    {
        if (reader.VersionNumber >= 410)
        {
            // Nothing to do here. Base class reads list.
        }
        else
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                CustomizationEnabling nxtEnabling = new (reader);
                Add(nxtEnabling);
            }
        }
    }

    public new void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }
    #endregion

    protected override CustomizationEnabling CreateInstance(IReader a_reader)
    {
        return new CustomizationEnabling(a_reader);
    }

    public CustomizationEnablingList(CustomizationEnablingComparer a_comparer)
        : base(new CustomizationEnablingComparer()) { }

    internal void Update(AddInControlUpdateT a_T)
    {
        AddInControllerList addInControllerList = a_T.AddInControllerList;
        for (int i = 0; i < addInControllerList.Count; i++)
        {
            AddInController addInController = addInControllerList[i];
            CustomizationEnabling custEn = GetValue(addInController.Name);
            if (custEn != null)
            {
                custEn.Update(addInController);
            }
            else
            {
                Add(new CustomizationEnabling(addInController.Name, addInController.Enabled, addInController.AdminControlled));
            }
        }
    }

    public bool Contains(string a_CustomizationName)
    {
        return ContainsKey(a_CustomizationName);
    }

    /// <summary>
    /// returns null if does not exists.
    /// </summary>
    /// <param name="a_CustomizationName"></param>
    /// <returns></returns>
    public CustomizationEnabling GetCustomizationEnabling(string a_CustomizationName)
    {
        return GetValue(a_CustomizationName);
    }

    public class CustomizationEnablingComparer : IKeyObjectComparer<CustomizationEnabling>
    {
        public int Compare(CustomizationEnabling x, CustomizationEnabling y)
        {
            return CompareCustomizationEnabling(x, y);
        }

        internal static int CompareCustomizationEnabling(CustomizationEnabling a_cust, CustomizationEnabling a_anotherCust)
        {
            return Comparison.Compare(a_cust.CustomizationName, a_anotherCust.CustomizationName);
        }

        public object GetKey(CustomizationEnabling a_cust)
        {
            return a_cust.CustomizationName;
        }
    }

    public class CustomizationEnabling : IPTSerializable
    {
        public const int UNIQUE_ID = 723;

        #region IPTSerializable Members
        public int UniqueId => UNIQUE_ID;

        public CustomizationEnabling(IReader reader)
        {
            if (reader.VersionNumber >= 376)
            {
                reader.Read(out _customizationName);
                reader.Read(out _enabled);
                reader.Read(out _adminControlled);
            }
            else
            {
                reader.Read(out _customizationName);
                reader.Read(out _enabled);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(_customizationName);
            writer.Write(_enabled);
            writer.Write(_adminControlled);
        }
        #endregion

        public CustomizationEnabling(string a_customizationName, bool a_enable, bool a_adminControlled)
        {
            _customizationName = a_customizationName;
            _enabled = a_enable;
            _adminControlled = a_adminControlled;
        }

        internal CustomizationEnabling(string a_customizationName)
        {
            _customizationName = a_customizationName;
        }

        public void Update(AddInController aUpdateController)
        {
            Enabled = aUpdateController.Enabled;
            AdminControlled = aUpdateController.AdminControlled;
        }

        private string _customizationName;

        public string CustomizationName
        {
            get => _customizationName;
            internal set => _customizationName = value;
        }

        private bool _enabled;

        /// <summary>
        /// Whether to use the Customization during scheduling.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        private bool _adminControlled;

        /// <summary>
        /// Whether only Admin users can disable/enable.
        /// </summary>
        public bool AdminControlled
        {
            get => _adminControlled;
            set => _adminControlled = value;
        }
    }

    ///// <summary>
    ///// Update the list that keeps track of which customizations are enabled.
    ///// Any previous customizations that no longer exist are cleared.
    ///// </summary>
    //internal void UpdateCustomizationEnablingList(List<ICustomizationBase> a_customizations)
    //{
    //    Dictionary<string, CustomizationEnabling> oldList = _customizationEnablingList;

    //    Dictionary<string, CustomizationEnabling> newList = new Dictionary<string, CustomizationEnabling>();
    //    for (int i = 0; i < a_customizations.Count; i++)
    //    {
    //        ICustomizationBase customization=a_customizations[i];
    //        bool enabled = true; //default
    //        bool adminControlled = true; //default
    //        if (oldList.ContainsKey(customization.Name))
    //        {
    //            CustomizationEnabling enablingOfThisCustomizations = oldList[customization.Name];
    //            enabled = enablingOfThisCustomizations.Enabled;
    //            adminControlled = enablingOfThisCustomizations.AdminControlled;
    //        }
    //        if (newList.ContainsKey(customization.Name))
    //            throw new PT.APSCommon.PTValidationException(String.Format("Duplicate Scheduler Customization Name: '{0}'.  Each Scheduler Customization must have a unique Name.",customization.Name));
    //        newList.Add(customization.Name, new CustomizationEnabling(customization.Name, enabled, adminControlled));
    //    }

    //    _customizationEnablingList = newList;
    //}
}
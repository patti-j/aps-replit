using System.Reflection;

namespace PT.APSCommon
{
    public interface IAuditEntry
    {
        /// <summary>
        /// String field to Display the object being audited
        /// </summary>
        public string ObjectType { get; }
        /// <summary>
        /// The Id of the instance of the object being audited
        /// </summary>
        public BaseId Id { get; }
        /// <summary>
        /// The Parent Id of the instance of the object being audited, the may be null if the object is a top level object
        /// </summary>
        public BaseId ParentId { get; }
        /// <summary>
        /// Indicates that the instance of the object was deleted or removed
        /// </summary>
        public bool Deleted { get; }
        /// <summary>
        /// Indicates that a new instance of the object was created or added
        /// </summary>
        public bool Added { get; }
        /// <summary>
        /// Only set to true when there what changes found after the comparison was made.
        /// </summary>
        public bool Updated => Changes.Count > 0;
        /// <summary>
        /// Collection of change entries if an instance of the object was updated.
        /// </summary>
        public List<ChangeEntry> Changes { get; }
        void Compare();
        /// <summary>
        /// Sets the flags to indicate the object was either added or deleted
        /// </summary>
        /// <param name="a_added"></param>
        /// <param name="a_deleted"></param>
        void SetFlags(bool a_added, bool a_deleted);

    }

    public class ChangeEntry
    {
        public string Field { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }

    public interface IScheduleAuditEntry
    {
        public IAuditEntry GetAuditEntry();
    }
}

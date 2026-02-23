using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Interface that is implemented by all data types that can be suspect --
/// that is whose data integrity should be examined.
/// </summary>
public interface IDataInspector
{
    /// <summary>
    /// Unique, unchangeable, numeric identifier.
    /// </summary>
    [System.ComponentModel.ParenthesizePropertyName(true)]
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    BaseId Id { get; }

    /// <summary>
    /// Identifier for external system references.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    string ExternalId { get; }

    /// <summary>
    /// The result of examining the object.
    /// </summary>
    DataInspectorControls.diagnoses Diagnosis { get; }

    /// <summary>
    /// Indicates what is wrong with the object -- or what was wrong but has since been automatically adjusted.
    /// </summary>
    string Problems { get; }

    /// <summary>
    /// Checks the object for conformance to any DataInspectorControls pertaining to this type of object.
    /// Takes the appropriate action based on DataInspectorControls.Response.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    void Diagnose(DataInspectorControls controls);
}
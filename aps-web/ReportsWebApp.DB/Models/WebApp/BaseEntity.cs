using System.ComponentModel.DataAnnotations;

using ReportsWebApp.DB.Models.WebApp;

namespace ReportsWebApp.DB.Models;

public class BaseEntity : INamedEntity
{
    [Key]
    [Required]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public DateTime? CreationDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public BaseEntity()
    {
        CreationDate = DateTime.UtcNow;
    }
    public override bool Equals(object obj)
    {
        if (obj is BaseEntity typedObj)
        {
            return Id == typedObj.Id;
        }
        return base.Equals(obj);
    }
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    /// A short string designed for compact at-a-glance identification.
    /// </summary>
    /// <returns></returns>
    public virtual string QuickDisplayValue => Name;

    /// <summary>
    /// A potentially longer string for providing further information in e.g. a tooltip.
    /// </summary>
    /// <returns></returns>
    public virtual string DetailDisplayValue => string.Empty;

    /// <summary>
    /// Gets a concatenated string containing the fields a user would generally search for this item by.
    /// </summary>
    /// <returns></returns>
    public virtual string TextSearchDump => $"{QuickDisplayValue}|{DetailDisplayValue}";

    /// <summary>
    /// Human-friendly name for the entity type, to be displayed in management UIs
    /// </summary>
    public virtual string TypeDisplayName => this.GetType().Name;

    /// <summary>
    /// Pluralized version of <see cref="TypeDisplayName"/>
    /// </summary>
    public virtual string PluralName => TypeDisplayName + "s";
}


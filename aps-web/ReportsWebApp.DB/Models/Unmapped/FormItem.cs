namespace ReportsWebApp.DB.Models.Unmapped
{
    /// <summary>
    /// Represents a form item in the Planning Area Details form.
    /// </summary>
    public class FormItem
    {
        /// <summary>
        /// Gets the caption of the form item.
        /// </summary>
        public string Caption { get; }

        /// <summary>
        /// Gets or sets the value bound to the form item.
        /// </summary>
        public Func<string> GetValue { get; }
        public Action<string> SetValue { get; }

        /// <summary>
        /// Gets or sets the CSS class applied to the form item.
        /// </summary>
        public string CssClass { get { return HasError ? "invalid-field" : ""; } }
        public bool HasError { get; set; } 

        /// <summary>
        /// Gets or sets a value indicating whether the validation icon should be shown.
        /// </summary>
        public bool? ShowValidationIcon { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormItem"/> class.
        /// </summary>
        /// <param name="caption">The caption of the form item.</param>
        /// <param name="bindingValue">The value bound to the form item.</param>
        /// <param name="refValue">The reference to the DxTextBox component (optional).</param>
        /// <param name="cssClass">The CSS class applied to the form item (optional).</param>
        /// <param name="showValidationIcon">Indicates whether to show the validation icon (optional).</param>
        public FormItem(string caption, Func<string> getValue, Action<string> setValue, bool hasError = false, bool? showValidationIcon = null)
        {
            Caption = caption;
            GetValue = getValue;
            SetValue = setValue;
            HasError = hasError;
            ShowValidationIcon = showValidationIcon;
        }

        public string BindingValue
        {
            get => GetValue();
            set => SetValue(value);
        }
    }

}

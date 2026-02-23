namespace ReportsWebApp.DB.Models
{
    public class TemplateEvaluation
    {
        /// <summary>
        /// Indicates whether the template processing was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The result of the template processing if successful; otherwise, the original template.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Error or validation message providing details about issues during processing.
        /// </summary>
        public string Message { get; set; }
    }

}

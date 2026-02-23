namespace PT.ImportDefintions.RequestsAndResponses
{
    public class BrowseTableWithValidationRequest
    {
        public string CommandText { get; set; }
        public bool IncludeValidation { get; set; }
        public string TableName { get; set; }
        public NewImportSettings ImportSettings { get; set; }
    }
}

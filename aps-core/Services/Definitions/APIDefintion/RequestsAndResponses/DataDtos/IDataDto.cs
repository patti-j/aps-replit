namespace PT.APIDefinitions.RequestsAndResponses.DataDtos;

public interface IDataDto
{
    /// <summary>
    /// Holds the names of fields which can be optionally queried by the API caller to be returned in a GET request (in addition to fields that always return).
    /// </summary>
    // These will often be fields in the underlying entity class (or fields in nested objects), but aren't necessarily coupled.
    // *** Dev Note: implementations use nameof() to define this, which means updating DTO field names breaks existing APIs. This is intended and should not be done casually. ***
    public List<string> SelectableFields { get; }

    public string ExternalId { get; set; }
}
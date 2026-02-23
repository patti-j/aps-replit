using PT.ServerManagerSharedLib.Azure;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class StandardVersionResponse
    {
        public StandardVersionResponse() { }

        public StandardVersionResponse(IEnumerable<StandardVersionEntity> a_versions)
        {
            Versions = a_versions.Select(v =>
                new VersionDto(v)).ToList();
        }

        public List<VersionDto> Versions { get; set; }
    }

    public class VersionDto
    {
        public VersionDto() { }

        public VersionDto(StandardVersionEntity a_versionEntity)
        {
            VersionNumber = a_versionEntity.VersionNumber;
            VersionDate = a_versionEntity.VersionDate;
        }

        public string VersionNumber { get; set; }
        public DateTime VersionDate { get; set; }
    }
}

using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.Exceptions;

/// <summary>
/// Simple structure to hold both instance-specific and serverwide settings, so Dapper can pull both from a single query.
/// TODO: It would probably be beneficial in future to add ServerWideSettings to the APSInstanceEntity object...
/// TODO: but the way we store the instance right now prevents that (the serverwide settings would be seriailized within them)
/// </summary>
public class InstanceDbDto
{
    public string Name { get; set; }
    public string Version { get; set; }
    public APSInstanceEntity Instance { get; set; }

    public ServerWideInstanceSettings ServerWideSettings;
}
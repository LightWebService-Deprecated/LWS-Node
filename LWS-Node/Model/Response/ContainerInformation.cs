using Docker.DotNet.Models;

namespace LWS_Node.Model.Response;

public class ContainerInformation
{
    public string ContainerId { get; set; }
    public string ImageName { get; set; }
    public string ImageTag { get; set; }
    public CreateContainerParameters CreateContainerParameters { get; set; }
}
using System.Collections.Generic;

namespace LWS_Node.Model.Request;

public class AspInitializeRequest
{
    public string DockerImageName { get; set; }
    public string DockerImageTag { get; set; }
    public string AppSettingsAsJson { get; set; }
    public string AppSettingsStoreDirectory { get; set; }
    public string OpenedPort { get; set; } // i.e "1010:4000",
    public List<string> EnvironmentList { get; set; }
}
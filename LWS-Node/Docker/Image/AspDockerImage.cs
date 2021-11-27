using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using LWS_Node.Model.Request;
using LWS_Node.Model.Response;

namespace LWS_Node.Docker.Image;

public class AspDockerImage: DockerImageBase
{
    public AspDockerImage(DockerClient dockerClient, AspInitializeRequest request) : base(dockerClient)
    {
        ImageName = request.DockerImageName;
        ImageTag = request.DockerImageTag;
        
        var fromPort = request.OpenedPort.Split(':')[0];
        var destinationPort = request.OpenedPort.Split(':')[1];

        var appSettingsPath = CreateJsonFile(request.AppSettingsAsJson);

        ContainerParameters = new CreateContainerParameters
        {
            Image = FullImageName,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    [destinationPort] = new List<PortBinding> {new() {HostIP = "0.0.0.0", HostPort = fromPort}}
                },
                Binds = new List<string>
                {
                    $"{appSettingsPath}:{request.AppSettingsStoreDirectory}/appsettings.json"
                }
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                [fromPort] = new()
            },
            Env = request.EnvironmentList
        };
    }

    private string CreateJsonFile(string json)
    {
        var tmpPath = $"{Path.GetTempPath()}/KDR_TMP_{Guid.NewGuid().ToString()}.json";
        using var fileOpen = File.OpenWrite(tmpPath);
        using var streamWriter = new StreamWriter(fileOpen);
        
        streamWriter.Write(json);
        streamWriter.Flush();

        return tmpPath;
    }
    
    private async Task DownloadImage()
    {
        if (await CheckImageExists()) return;

        await DockerClient.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = ImageName,
            Tag = ImageTag
        }, new AuthConfig(), new Progress<JSONMessage>());
    }

    public override async Task<ContainerInformation> CreateContainerAsync()
    {
        await DownloadImage();
        ContainerId = (await DockerClient.Containers.CreateContainerAsync(ContainerParameters))
            .ID;

        return new ContainerInformation
        {
            ContainerId = ContainerId,
            ImageName = ImageName,
            ImageTag = ImageTag,
            CreateContainerParameters = ContainerParameters
        };
    }

    public override async Task RunContainerAsync()
    {
        await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters());
    }

    public override async Task RemoveContainerAsync()
    {
        await DockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters());
        await DockerClient.Containers.RemoveContainerAsync(ContainerId, new ContainerRemoveParameters());
    }
}
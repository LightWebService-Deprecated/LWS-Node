using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;

namespace LWS_Node.Docker.Image;

public class AspDockerImage: DockerImageBase
{
    private readonly string _destinationPath = $"{Path.GetTempPath()}/docker_asp_{Guid.NewGuid().ToString()}.tar";
    private readonly string _repositoryPath;
    
    public AspDockerImage(DockerClient dockerClient, string repositoryPath) : base(dockerClient)
    {
        _repositoryPath = repositoryPath;
        ImageName = "testaspcontainer";
        ImageTag = "latest";
        ContainerParameters = new CreateContainerParameters
        {
            Image = FullImageName
        };
    }

    public override async Task CreateContainerAsync()
    {
        CreateContextTar();
        await BuildAspImage();
        ContainerId = (await DockerClient.Containers.CreateContainerAsync(ContainerParameters)).ID;
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
    
    private void CreateContextTar()
    {
        using var tarArchive = TarArchive.Create();
        tarArchive.AddAllFromDirectory(_repositoryPath);
        tarArchive.SaveTo(_destinationPath, CompressionType.None);
    }

    private async Task BuildAspImage()
    {
        if (!(await CheckImageExists()))
        {
            await using var stream = new FileStream(_destinationPath, FileMode.Open);
            var imageParameter = new ImageBuildParameters
            {
                Tags = new List<string> { ContainerParameters.Image },
            };

            await DockerClient.Images.BuildImageFromDockerfileAsync(imageParameter, stream, new List<AuthConfig>(), new Dictionary<string, string>(), new Progress<JSONMessage>());
        }
    }
}
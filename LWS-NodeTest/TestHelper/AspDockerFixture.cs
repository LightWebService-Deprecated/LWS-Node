using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace LWS_NodeTest.TestHelper;

public class AspDockerFixture: IDisposable
{
    public readonly string FullImageName = $"{ImageName}:{ImageTag}";
    
    private const string ImageName = "testaspcontainer";
    private const string ImageTag = "latest";
    private const string TarDestinationBase = "docker_asp*"; 

    public readonly DockerClient DockerClient;

    public AspDockerFixture()
    {
        DockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();
        RemoveImage().Wait();
    }

    public void Dispose()
    {
        RemoveImage().Wait();
    }

    private async Task RemoveImage()
    {
        await RemoveContainers();
        var exists = (await DockerClient.Images.ListImagesAsync(new ImagesListParameters {All = true}))
            .Any(a => a.RepoTags.Contains(FullImageName));
        if (exists) await DockerClient.Images.DeleteImageAsync(FullImageName, new ImageDeleteParameters());
    }
    
    public async Task RemoveContainers()
    {
        var filteredList = await GetAspContainer();
        foreach (var eachImage in filteredList)
        {
            await DockerClient.Containers.StopContainerAsync(eachImage.ID, new ContainerStopParameters());
            await DockerClient.Containers.RemoveContainerAsync(eachImage.ID, new ContainerRemoveParameters());
        }
    }

    public async Task<IList<ContainerListResponse>> GetAspContainer()
    {
        var result = await DockerClient.Containers.ListContainersAsync(new ContainersListParameters {All = true});
        return result.Where(a => a.Image == FullImageName)
            .ToList();
    }

    public void RemoveTarFile()
    {
        var tempDirectory = Path.GetTempPath();
        foreach (var eachPath in Directory.GetFiles(tempDirectory, TarDestinationBase,
                     SearchOption.AllDirectories))
        {
            File.Delete(eachPath);
        }
    }
}
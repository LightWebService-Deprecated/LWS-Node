using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using LWS_Node.Docker;
using LWS_Node.Docker.Image;
using LWS_Node.Model.Request;
using Xunit;

namespace LWS_NodeTest.Docker;

public class AspDockerImageTest: IDisposable
{
    private readonly DockerClient _dockerClient;

    public AspDockerImageTest()
    {
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();
        DownloadUbuntuImage().Wait();
    }

    public void Dispose()
    {
        var tempPath = Path.GetTempPath();

        foreach (var pathString in Directory.EnumerateFiles(tempPath, "KDR_TMP_*.json"))
        {
            File.Delete(pathString);
        }
    }
    
    private async Task<bool> CheckImageExists()
    {
        var list = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters
        {
            All = true
        });
        return list.Any(a => a.RepoTags.Contains("ubuntu:latest"));
    }

    private async Task DownloadUbuntuImage()
    {
        if (await CheckImageExists()) return;

        await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = "ubuntu",
            Tag = "latest"
        }, new AuthConfig(), new Progress<JSONMessage>());
    }

    private async Task CleanupContainer(string containerId)
    {
        await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
    }

    [Fact(DisplayName = "AspDockerImage: CreateContainerAsync should create container with correct configuration well.")]
    public async void Is_AspDockerImage_Constructor_Creating_Necessary_File_Configuration_Well()
    {
        // Let
        var request = new AspInitializeRequest
        {
            AppSettingsAsJson = "",
            AppSettingsStoreDirectory = "/app",
            DockerImageName = "ubuntu",
            DockerImageTag = "latest",
            EnvironmentList = new List<string>(),
            OpenedPort = "4040:4040"
        };
        
        // Do
        var aspImage = new AspDockerImage(_dockerClient, request);
        var result = await aspImage.CreateContainerAsync();

        // Check
        Assert.NotNull(result);
        Assert.NotNull(result.ContainerId);
        Assert.Equal(request.DockerImageName, result.ImageName);
        Assert.Equal(request.DockerImageTag, result.ImageTag);
        
        // Cleanup
        await CleanupContainer(result.ContainerId);
    }

    [Fact(DisplayName = "AspDockerImage: RunContainerAsync should run container well with correct configuration")]
    public async void Is_AspDockerImage_Runs_Container_Well()
    {
        // Let
        var request = new AspInitializeRequest
        {
            AppSettingsAsJson = "",
            AppSettingsStoreDirectory = "/app",
            DockerImageName = "ubuntu",
            DockerImageTag = "latest",
            EnvironmentList = new List<string>(),
            OpenedPort = "4040:4040"
        };
        
        // Do
        var aspImage = new AspDockerImage(_dockerClient, request);
        var result = await aspImage.CreateContainerAsync();
        await aspImage.RunContainerAsync();

        // Check
        Assert.NotNull(result);
        Assert.NotNull(result.ContainerId);
        Assert.Equal(request.DockerImageName, result.ImageName);
        Assert.Equal(request.DockerImageTag, result.ImageTag);
        
        // Check running container
        var targetContainer = (await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters()))
            .SingleOrDefault(a => a.ID == result.ContainerId);
        Assert.NotNull(targetContainer);

        // Cleanup
        await CleanupContainer(result.ContainerId);
    }

    [Fact(DisplayName = "AspDockerImage: RemoveContainerAsync should remove container properly")]
    public async void Is_RemoveContainerAsync_Removes_Container_Properly()
    {
        // Let
        var request = new AspInitializeRequest
        {
            AppSettingsAsJson = "",
            AppSettingsStoreDirectory = "/app",
            DockerImageName = "ubuntu",
            DockerImageTag = "latest",
            EnvironmentList = new List<string>(),
            OpenedPort = "4040:4040"
        };
        
        // Do
        var aspImage = new AspDockerImage(_dockerClient, request);
        var result = await aspImage.CreateContainerAsync();
        await aspImage.RunContainerAsync();
        await aspImage.RemoveContainerAsync();

        // Check
        Assert.NotNull(result);
        Assert.NotNull(result.ContainerId);
        Assert.Equal(request.DockerImageName, result.ImageName);
        Assert.Equal(request.DockerImageTag, result.ImageTag);
        
        // Check running container
        var targetContainer = (await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters()))
            .SingleOrDefault(a => a.ID == result.ContainerId);
        Assert.Null(targetContainer);
    }
}
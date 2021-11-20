using System;
using System.IO;
using Docker.DotNet.Models;
using LWS_Node.Docker;
using LWS_Node.Docker.Image;
using LWS_NodeTest.TestHelper;
using Xunit;

namespace LWS_NodeTest.Docker.Image;

public class AspDockerImageTest: IDisposable, IClassFixture<AspDockerFixture>
{
    private readonly DockerImageBase _aspDockerImage;
    private readonly string _repositoryPath;

    private readonly AspDockerFixture _fixture;

    public AspDockerImageTest(AspDockerFixture fixture)
    {
        _fixture = fixture;

        using var fileStream = File.OpenRead("appsettings.Development.json");
        using var streamReader = new StreamReader(fileStream);
        var jsonString = streamReader.ReadToEnd();
        
        var initializer = new DotnetAspInitializer("https://github.com/LightWebService/LWS-Node", jsonString, "LWS-Node/LWS-Node.csproj");
        var result = initializer.InitAsp();
        
        _aspDockerImage = new AspDockerImage(_fixture.DockerClient, result.TargetObject);
        _repositoryPath = result.TargetObject!;

        _fixture = fixture;
    }
    
    public void Dispose()
    {
        Directory.Delete(_repositoryPath, true);
        _fixture.RemoveTarFile();
        _fixture.RemoveContainers().Wait();
    }

    [Fact(DisplayName = "CreateContainerAsync should build image and container well.")]
    public async void Is_CreateContainerAsync_Should_Build_Image_Well()
    {
        // Do
        await _aspDockerImage.CreateContainerAsync();
        
        // Check
        var result =
            await _fixture.DockerClient.Images.ListImagesAsync(new ImagesListParameters());
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, a => a.RepoTags.Contains(_fixture.FullImageName));
    }

    [Fact(DisplayName = "RunContainerAsync should make container status to 'running'")]
    public async void Is_RunContainerAsync_Set_Running()
    {
        // Let
        var currentContainer = (await _fixture.GetAspContainer()).Count;
        await _aspDockerImage.CreateContainerAsync();

        // Do
        await _aspDockerImage.RunContainerAsync();
        
        // Check
        var afterRunContainer = (await _fixture.GetAspContainer()).Count;
        Assert.Equal(currentContainer+1, afterRunContainer);
    }

    [Fact(DisplayName = "RemoveContainerAsync should remove created container well.")]
    public async void Is_RemoveContainerAsync_Removes_Container_Well()
    {
        // Let
        await _aspDockerImage.CreateContainerAsync();
        await _aspDockerImage.RunContainerAsync();
        await _aspDockerImage.RemoveContainerAsync();
        
        // Check
        var result = await _fixture.GetAspContainer();
        Assert.Empty(result);
    }
}
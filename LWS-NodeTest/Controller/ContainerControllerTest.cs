using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using LWS_Node.Configuration;
using LWS_Node.Model.Request;
using LWS_Node.Model.Response;
using LWS_NodeTest.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace LWS_NodeTest.Controller;

public class ContainerControllerTest: IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _testServerContainer;
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly DockerClient _dockerClient;

    public ContainerControllerTest(CustomWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        _httpClient = factory.CreateClient();
        _testServerContainer = factory.Services;

        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();
        _testOutputHelper = testOutputHelper;

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
        return list.Any(a => a.RepoTags.Contains("mongo:latest"));
    }

    private async Task DownloadUbuntuImage()
    {
        if (await CheckImageExists()) return;

        await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = "mongo",
            Tag = "latest"
        }, new AuthConfig(), new Progress<JSONMessage>());
    }
    
    private async Task CleanupContainer(string containerId)
    {
        await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
    }

    [Fact(DisplayName = "POST /api/v1/container/asp should create and run container well.")]
    public async void Is_CreateAspContainer_Works_Well()
    {
        // Let
        var nodeConfiguration = _testServerContainer.GetService<NodeConfiguration>();
        var testRequest = new AspInitializeRequest
        {
            AppSettingsAsJson = "",
            AppSettingsStoreDirectory = "/app",
            DockerImageName = "mongo",
            DockerImageTag = "latest",
            EnvironmentList = new List<string>(),
            OpenedPort = "2020:4040"
        };
        
        // Do
        _httpClient.DefaultRequestHeaders.Add("X-NODE-AUTH", new []{nodeConfiguration.NodeKey});
        var response = await _httpClient.PostAsJsonAsync("/api/v1/container/asp", testRequest);
        
        // Check
        Assert.NotNull(response);
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
        Assert.True(response.IsSuccessStatusCode);
        var responseObject = await response.Content.ReadFromJsonAsync<ContainerInformation>();
        Assert.NotNull(responseObject);

        await CleanupContainer(responseObject.ContainerId);
    }
}
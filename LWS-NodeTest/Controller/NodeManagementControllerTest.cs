using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using LWS_Node.Configuration;
using LWS_Node.Model.Response;
using LWS_NodeTest.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace LWS_NodeTest.Controller;

public class NodeManagementControllerTest: IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _testServerContainer;

    public NodeManagementControllerTest(CustomWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
        _testServerContainer = factory.Services;
    }

    [Fact(DisplayName = "GET /api/v1/node/management should return unauthorized result when no auth key provided")]
    public async void Is_GetNodeInformation_Returns_unauthorized_When_No_Auth_Key()
    {
        // Do
        var response = await _httpClient.GetAsync("/api/v1/node/management");
        
        // Check
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var errorModel = await response.Content.ReadFromJsonAsync<ErrorModel>();
        Assert.NotNull(errorModel);
        Assert.Equal(StatusCodes.Status401Unauthorized, errorModel.StatusCode);
    }

    [Fact(DisplayName =
        "GET /api/v1/node/management should return OK with node configuration data when associated key provided.")]
    public async void Is_GetNodeInformation_Returns_OK_When_Key_Correct()
    {
        // Let
        var nodeConfiguration = _testServerContainer.GetService<NodeConfiguration>()!;
        Assert.NotNull(nodeConfiguration);
        _httpClient.DefaultRequestHeaders.Add("X-NODE-AUTH", new []{nodeConfiguration.NodeKey});
        
        // Do
        var response = await _httpClient.GetAsync("/api/v1/node/management");
        
        // Check
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var configurationFromResult = await response.Content.ReadFromJsonAsync<NodeConfiguration>();
        Assert.Equal(nodeConfiguration.NodeKey, configurationFromResult.NodeKey);
    }

    [Fact(DisplayName =
        "GET /api/v1/node/management/alive should return unauthorized result when no auth key provided.")]
    public async void Is_NodeHeartbeat_Returns_Unauthorized_When_No_Auth_Key()
    {
        // Do
        var response = await _httpClient.GetAsync("/api/v1/node/management/alive");
        
        // Check
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var errorModel = await response.Content.ReadFromJsonAsync<ErrorModel>();
        Assert.NotNull(errorModel);
        Assert.Equal(StatusCodes.Status401Unauthorized, errorModel.StatusCode);
    }

    [Fact(DisplayName = "GET /api/v1/node/management/alive should return ok when everything is ok.")]
    public async void Is_NodeHeartbeat_Returns_Ok()
    {
        // Let
        var nodeConfiguration = _testServerContainer.GetService<NodeConfiguration>()!;
        Assert.NotNull(nodeConfiguration);
        _httpClient.DefaultRequestHeaders.Add("X-NODE-AUTH", new []{nodeConfiguration.NodeKey});
        
        // Do
        var response = await _httpClient.GetAsync("/api/v1/node/management/alive");
        
        // Check
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
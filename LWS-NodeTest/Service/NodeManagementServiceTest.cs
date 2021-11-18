using System;
using LWS_Authentication;
using LWS_Node.Configuration;
using LWS_Node.Service;
using LWS_NodeManager;
using LWS_Shared;
using Newtonsoft.Json;
using Xunit;

namespace LWS_NodeTest.Service;

public static class StringExtension
{
    public static T AsObject<T>(this string targetString)
    {
        return JsonConvert.DeserializeObject<T>(targetString) ?? throw new Exception("Cannot Deserialize object!");
    }
}

public class NodeManagementServiceTest
{
    private readonly NodeConfiguration _nodeConfiguration;
    private readonly NodeManagementService _nodeManagementService;

    public NodeManagementServiceTest()
    {
        _nodeConfiguration = new NodeConfiguration();
        _nodeManagementService = new NodeManagementService(_nodeConfiguration);
    }

    [Fact(DisplayName =
        "GetNodeInformation should return forbidden when input secret does not matches server's secret.")]
    public async void Is_GetNodeInformation_Returns_Forbidden()
    {
        // Let
        var registerRequest = new RegisterNodeRequest {Code = "test"};
        
        // Do
        var result = await _nodeManagementService.GetNodeInformation(registerRequest, null);
        
        // Check
        Assert.NotNull(result);
        Assert.Equal(ResultCode.Forbidden, result.ResultCode);
    }

    [Fact(DisplayName = "GetNodeInformation should return success when code is correct.")]
    public async void Is_GetNodeInformation_Returns_Success()
    {
        // Let
        var registerRequest = new RegisterNodeRequest {Code = _nodeConfiguration.NodeKey};
        _nodeConfiguration.NodeMaximumCpu = 4;
        _nodeConfiguration.NodeMaximumRam = 40;
        _nodeConfiguration.NodeNickName = "test";

        // Do
        var result = await _nodeManagementService.GetNodeInformation(registerRequest, null);
        
        // Check
        Assert.NotNull(result);
        Assert.Equal(ResultCode.Success, result.ResultCode);
        var configurationReceived = result.Content.AsObject<NodeConfiguration>();
        Assert.Equal(_nodeConfiguration.NodeMaximumCpu, configurationReceived.NodeMaximumCpu);
        Assert.Equal(_nodeConfiguration.NodeMaximumRam, configurationReceived.NodeMaximumRam);
        Assert.Equal(_nodeConfiguration.NodeNickName, configurationReceived.NodeNickName);
    }

    [Fact(DisplayName = "NodeHeartBeat should return successful result")]
    public async void Is_NodeHeartBeat_Works_Well()
    {
        var result = await _nodeManagementService.NodeHeartbeat(new HeartbeatRequest(), null);
        Assert.NotNull(result);
        Assert.Equal(ResultCode.Success, result.ResultCode);
    }
}
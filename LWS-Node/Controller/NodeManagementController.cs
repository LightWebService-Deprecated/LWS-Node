using LWS_Node.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Node.Controller;

[ApiController]
[Route("/api/v1/node/management")]
public class NodeManagementController: ControllerBase
{
    private readonly NodeConfiguration _nodeConfiguration;

    public NodeManagementController(NodeConfiguration nodeConfiguration)
    {
        _nodeConfiguration = nodeConfiguration;
    }

    [HttpGet]
    public IActionResult GetNodeInformation()
    {
        return Ok(_nodeConfiguration);
    }

    [HttpGet("alive")]
    public IActionResult NodeHeartbeat()
    {
        return Ok();
    }
}
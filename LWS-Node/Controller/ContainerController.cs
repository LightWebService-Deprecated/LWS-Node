using System;
using System.Threading.Tasks;
using Docker.DotNet;
using LWS_Node.Docker.Image;
using LWS_Node.Model.Request;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Node.Controller;

[Route("/api/v1/container")]
[ApiController]
public class ContainerController: ControllerBase
{
    [HttpPost("asp")]
    public async Task<IActionResult> CreateAspContainer(AspInitializeRequest request)
    {
        var dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();
        var aspContainerObject = new AspDockerImage(dockerClient, request);
        
        // Check
        var containerInformation = await aspContainerObject.CreateContainerAsync();
        await aspContainerObject.RunContainerAsync();
        
        // Return
        return Ok(containerInformation);
    }
}
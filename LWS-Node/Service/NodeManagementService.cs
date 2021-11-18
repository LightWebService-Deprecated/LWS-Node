using System.Threading.Tasks;
using Grpc.Core;
using LWS_Node.Configuration;
using LWS_NodeManager;
using LWS_Shared;
using Newtonsoft.Json;

namespace LWS_Node.Service
{
    public class NodeManagementService: NodeManagerService.NodeManagerServiceBase
    {
        private readonly NodeConfiguration _nodeConfiguration;

        public NodeManagementService(NodeConfiguration nodeConfiguration)
        {
            _nodeConfiguration = nodeConfiguration;
        }

        public override Task<Result> GetNodeInformation(RegisterNodeRequest request, ServerCallContext context)
        {
            if (request.Code != _nodeConfiguration.NodeKey)
            {
                return Task.FromResult(new Result()
                {
                    ResultCode = ResultCode.Forbidden,
                    Message = "The Node secret log is not correct! Please check again."
                });
            }

            return Task.FromResult(new Result
            {
                ResultCode = ResultCode.Success,
                Content = JsonConvert.SerializeObject(_nodeConfiguration)
            });
        }

        public override Task<Result> NodeHeartbeat(HeartbeatRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Result {ResultCode = ResultCode.Success});
        }
    }
}
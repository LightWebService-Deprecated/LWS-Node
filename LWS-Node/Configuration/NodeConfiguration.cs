using System;

namespace LWS_Node.Configuration
{
    public class NodeConfiguration
    {
        public string NodeKey { get; set; }
        public string NodeNickName { get; set; }
        public int NodeMaximumCpu { get; set; }
        public int NodeMaximumRam { get; set; }

        public NodeConfiguration()
        {
            NodeKey = Guid.NewGuid().ToString();
        }
    }
}
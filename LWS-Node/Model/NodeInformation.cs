using System.Diagnostics.CodeAnalysis;

namespace LWS_Node.Model
{
    [ExcludeFromCodeCoverage]
    public class NodeInformation
    {
        // Default Node Information
        public string Id { get; set; }
        public string NodeNickName { get; set; }
        public int NodeMaximumCpu { get; set; }
        public int NodeMaximumRam { get; set; }
        
        // Allocated Resource Information(Which is NOT identical to real system usage.)
        public int NodeAllocatedCpu { get; set; }
        public int NodeAllocatedRam { get; set; }
        
        // Usage calculated.(TODO: Auto-Calculate Usage)
        public double NodeCpuUsage { get; set; }
        public double NodeRamUsage { get; set; }
    }
}
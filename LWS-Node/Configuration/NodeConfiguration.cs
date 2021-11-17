using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace LWS_Node.Configuration
{
    public class NodeConfiguration
    {
        private const string TokenPath = "/tmp/lws_token";
        
        public string NodeKey { get; set; }
        public string NodeNickName { get; set; }
        public int NodeMaximumCpu { get; set; }
        public int NodeMaximumRam { get; set; }

        public NodeConfiguration()
        {
            if (File.Exists(TokenPath))
            {
                using var fileStream = File.OpenRead(TokenPath);
                using var fileReader = new StreamReader(fileStream);
                NodeKey = fileReader.ReadLine() 
                          ?? throw new NullReferenceException("Token file read from stream, but null returned");
            }
            else
            {
                NodeKey = GenerateRandomToken();
                
                using var fileStream = File.OpenWrite(TokenPath);
                using var fileWriter = new StreamWriter(fileStream);
                fileWriter.WriteLine(NodeKey);
                fileWriter.Flush();
            }
        }

        private string GenerateRandomToken(int length = 64)
        {
            var random = new Random();
            var charDictionary = "1234567890=#_+abcdefghijklmnopqrstuvwxyz";

            return new string(Enumerable.Repeat(charDictionary, length)
                .Select(a => a[random.Next(charDictionary.Length)]).ToArray());
        }
    }
}
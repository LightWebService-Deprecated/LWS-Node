using System;
using System.IO;
using LWS_Node.Configuration;
using Xunit;

namespace LWS_NodeTest.Configuration;

public class NodeConfigurationTest: IDisposable
{
    private const string TokenPath = "/tmp/lws_token";

    public NodeConfigurationTest()
    {
        RemoveTokenPathIfExists();
    }

    private string ReadTokenFile()
    {
        using var fileStream = File.OpenRead(TokenPath);
        using var fileReader = new StreamReader(fileStream);

        return fileReader.ReadLine() ?? throw new NullReferenceException("File read but null returned.");
    }

    private void WriteTokenFile(string tokenMessage)
    {
        using var fileStream = File.OpenWrite(TokenPath);
        using var fileWriter = new StreamWriter(fileStream);
        
        fileWriter.WriteLine(tokenMessage);
    }

    private void RemoveTokenPathIfExists()
    {
        if (File.Exists(TokenPath))
        {
            File.Delete(TokenPath);
        }
    }

    public void Dispose()
    {
        RemoveTokenPathIfExists();
    }

    [Fact(DisplayName = "NodeConfiguration will generate random token when target token file does not exists.")]
    public void Is_NodeConfiguration_Generates_Token_When_Target_File_Not_Exists()
    {
        // Do
        var configuration = new NodeConfiguration();
        
        // Check
        Assert.True(File.Exists(TokenPath));
        Assert.Equal(configuration.NodeKey, ReadTokenFile());
    }

    [Fact(DisplayName = "NodeConfiguration should use existing token if file exists")]
    public void Is_NodeConfiguration_Uses_Generated_Token()
    {
        var testToken = "test";
        WriteTokenFile(testToken);

        var configuration = new NodeConfiguration();
        
        Assert.True(File.Exists(TokenPath));
        Assert.Equal(configuration.NodeKey, testToken);
    }
}
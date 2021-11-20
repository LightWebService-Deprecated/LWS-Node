using System.IO;
using LWS_Node.Docker;
using Xunit;

namespace LWS_NodeTest.Docker;

public class DotnetAspInitializerTest
{
    [Fact(DisplayName = "InitAsp should return successful AspInitResult when everything works well.")]
    public void Is_InitAsp_Returns_Successful_AspInitResult()
    {
        var initializer = new DotnetAspInitializer("https://github.com/LightWebService/LWS-Node", "", "LWS-Node/LWS-Node.csproj");

        var result = initializer.InitAsp();
        Assert.True(result.IsSucceed);
        Assert.NotNull(result.TargetObject);
        Assert.True(Directory.Exists(result.TargetObject));
        
        // Remove
        Directory.Delete(result.TargetObject!, true);
    }

    [Fact(DisplayName = "InitAsp should return negative AspInitResult when app cannot clone repository.")]
    public void Is_InitAsp_Returns_Negative_AspInitResult_When_App_Cannot_Clone_Repository()
    {
        var initializer = new DotnetAspInitializer("", "", "LWS-Node/LWS-Node.csproj");

        var result = initializer.InitAsp();
        Assert.False(result.IsSucceed);
        Assert.Null(result.TargetObject);
        Assert.False(Directory.Exists(result.TargetObject));
    }
}
using YJCabin.Application.Common;

namespace YJCabin.Api.Tests;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Hello YJCabin", "hello-yjcabin")]
    [InlineData("C# / React", "c-react")]
    [InlineData("  Architecture  ", "architecture")]
    public void From_NormalizesTitle(string input, string expected)
    {
        Assert.Equal(expected, SlugHelper.From(input));
    }

    [Fact]
    public void From_Empty_Throws()
    {
        Assert.Throws<ValidationException>(() => SlugHelper.From("   "));
    }
}

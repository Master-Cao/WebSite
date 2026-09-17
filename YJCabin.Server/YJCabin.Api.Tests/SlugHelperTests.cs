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

    [Fact]
    public void FromTitle_UsesTitle()
    {
        Assert.Equal("hello-yjcabin", SlugHelper.FromTitle("Hello YJCabin"));
    }

    [Fact]
    public void FromTitle_Chinese_FallsBack()
    {
        var slug = SlugHelper.FromTitle("玻璃质感后台");
        Assert.StartsWith("article-", slug);
        Assert.Equal(16, slug.Length);
    }
}

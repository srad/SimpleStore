using Moq;
using SimpleStore.Helpers;

namespace SimpleStore.Tests.Helpers;

public class SlugTest
{
    [Fact]
    public void Generate_StorageSlugs()
    {
        // arrange
        var mockValidator = new Mock<StorageNameValidator>();
        var mockSlug = new Mock<StorageSlug>(mockValidator.Object);

        // act
        // assert
        Assert.NotNull(mockSlug.Object.Generate("a"));
        Assert.Throws<ArgumentException>(() => mockSlug.Object.Generate("-._"));
        Assert.Throws<ArgumentException>(() => mockSlug.Object.Generate(""));
        Assert.Throws<ArgumentException>(() => mockSlug.Object.Generate(" "));
        Assert.Throws<ArgumentException>(() => mockSlug.Object.Generate("                     "));
        Assert.Throws<ArgumentException>(() => mockSlug.Object.Generate(null));
        Assert.NotEqual("-some-fancy-shit", mockSlug.Object.Generate(" Some Fancy Shit          "));
        Assert.Throws<ArgumentException>(() => mockSlug.Object.Generate("_"));
        Assert.Throws<ArgumentException>(() => mockSlug.Object.Generate("."));
        Assert.Throws<ArgumentException>(() => mockSlug.Object.Generate("-"));
        Assert.Equal("some-fancy-shit", mockSlug.Object.Generate(" Some Fancy Shit          "));
        Assert.Equal("0123456789", mockSlug.Object.Generate(" 0123456789  "));
        Assert.Equal("something", mockSlug.Object.Generate("!\"§$%&/()= something "));
        Assert.Equal("some-thing-._-some-thing", mockSlug.Object.Generate(" some thing   #+'*-.,_:;°^!\"§$%&/()=?´`ß  some thing     "));
    }
}
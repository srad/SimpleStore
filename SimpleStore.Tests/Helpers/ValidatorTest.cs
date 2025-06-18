using Moq;
using SimpleStore.Helpers;

namespace SimpleStore.Tests.Helpers;

public class ValidatorTest
{
    [Fact]
    public void StorageNameValidator_Accept()
    {
        var mock = new Mock<StorageNameValidator>();
        
        Assert.True(mock.Object.IsValid("0"));
        Assert.True(mock.Object.IsValid("00"));
        Assert.True(mock.Object.IsValid("1"));
        Assert.True(mock.Object.IsValid("2"));
        Assert.True(mock.Object.IsValid("3"));
        Assert.True(mock.Object.IsValid("a"));
        Assert.True(mock.Object.IsValid("aa"));
        Assert.True(mock.Object.IsValid("a_normal-file.name.ext"));
        Assert.True(mock.Object.IsValid("abcdefghijklmnopqrstuvxyz0123456789"));
        Assert.True(mock.Object.IsValid("abcdefghij-klmnopqr-stuvxyz0123-456.789"));
        Assert.True(mock.Object.IsValid("a-z"));
        Assert.True(mock.Object.IsValid("0-1"));
        Assert.True(mock.Object.IsValid("0-1-a-z"));
        Assert.False(mock.Object.IsValid(" a"));
        Assert.False(mock.Object.IsValid(" a "));
        Assert.False(mock.Object.IsValid("."));
        Assert.False(mock.Object.IsValid("-"));
        Assert.False(mock.Object.IsValid("_"));
        Assert.False(mock.Object.IsValid(""));
        Assert.False(mock.Object.IsValid(" "));
        Assert.False(mock.Object.IsValid("           "));
        Assert.Throws<ArgumentNullException>(() => mock.Object.IsValid(null));
    }
}
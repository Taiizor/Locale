using Locale.Services;

namespace Locale.Tests.Services;

public class WatchServiceTests
{
    [Fact]
    public void WatchService_CanBeCreated()
    {
        using WatchService service = new();
        Assert.NotNull(service);
    }

    [Fact]
    public void WatchService_ThrowsForNonExistentDirectory()
    {
        using WatchService service = new();
        WatchOptions options = new()
        {
            BaseCulture = "en"
        };

        Assert.Throws<DirectoryNotFoundException>(() =>
            service.Start("/nonexistent/path/12345", options));
    }
}
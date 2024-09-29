using mvc_server.Helpers;
namespace Test.UtilityTest;

public class UtilityTest
{
    [Fact]
    public void Should_convert_bytes_to_human_string()
    {
        Assert.Equal("0 B", Utility.BytesToStringOptimized(0L));
        Assert.Equal("1 KiB", Utility.BytesToStringOptimized(1_024L));
        Assert.Equal("1 MiB", Utility.BytesToStringOptimized(1_024L * 1_024));
        Assert.Equal("1 GiB", Utility.BytesToStringOptimized(1_024L * 1_024 * 1_024));
        Assert.Equal("1 TiB", Utility.BytesToStringOptimized(1_024L * 1_024 * 1_024 * 1_024));
        Assert.Equal("1 PiB", Utility.BytesToStringOptimized(1_024L * 1_024 * 1_024 * 1_024 * 1_024));
        Assert.Equal("1 EiB", Utility.BytesToStringOptimized(1_024L * 1_024 * 1_024 * 1_024 * 1_024 * 1_024));

        Assert.Equal("5.42 GiB", Utility.BytesToStringOptimized(5_823_996_738L));
        Assert.Equal("8 EiB", Utility.BytesToStringOptimized(long.MaxValue));
        Assert.Equal("-8 EiB", Utility.BytesToStringOptimized(long.MinValue + 1));

    }
}
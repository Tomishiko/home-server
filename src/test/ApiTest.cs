using test.Heplers;

namespace test;

public class ApiTest
{
    [Fact]
    public void LargeFileUpload()
    {
        var helper = new StreamedFileHelper();
        var filePath = @"D:\VS_projs\HomeServer\src\mvc_server\wwwroot\files\movies\JoJo no Kimyou na Bouken Part 4 Diamond wa Kudakenai Episode 27.mp4";
        helper.SplitFile(filePath);
    }
}
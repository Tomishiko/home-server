using mvc_server.Models;
using mvc_server.Services;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
namespace test
{
    public class FileCompositorTest
    {
        [Fact]
        public void FileStreamDisposedAfterFilesFinalPartIsReached()
        {
            var loggerMoq = new Mock<ILogger<StreamedFileCompositor>>();

            var streamMoq = new Mock<SafeFileHandle>();
            bool handleClosed = false;
            //streamMoq.Setup(s => s.Close()).Callback(() => handleClosed = true);
            var comp = new StreamedFileCompositor(loggerMoq.Object);

            var fileExample = new StreamedFile
            {
                FileName = "file",
                FileSize = 1L,
                Id = "1",
                TotalFileParts = 20,
                Stream = streamMoq.Object,

            };
            comp.StreamedFiles.Add("1",fileExample);
            comp.StreamedFiles["1"].PartsWritten = 20;

            Assert.True(fileExample.Stream.IsClosed);
            Assert.Empty(comp.StreamedFiles);
        }
    }
}

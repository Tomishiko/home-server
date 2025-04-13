using web.Models;
using web.Services;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using web.Interfaces;
namespace test
{
    public class FileCompositorTest
    {
        [Fact]
        public void FileStreamDisposedAfterFilesFinalPartIsReached()
        {
            //Arrange
            var loggerMoq = new Mock<ILogger<StreamedFileCompositor>>();
            var streamMoq = new Mock<IFileHandleProvider>();
            bool handleClosed = false;
            streamMoq.Setup(s => s.Close()).Callback(() => handleClosed = true);
            var comp = new StreamedFileCompositor(loggerMoq.Object);
            var uid = Guid.NewGuid().ToString();
            var fileExample = new StreamedFile
            {
                FileName = "file",
                FileSize = 1L,
                Id = uid,
                TotalFileParts = 10,
                fileHandleProvider = streamMoq.Object,

            };
            comp.StreamedFiles.Add(uid, fileExample);
            fileExample.CloseEvent += comp.CloseEventHandler;

            //Handle should be active while final part is not reached
            for (int i = 0; i < 10; i++)
            {
                Assert.False(handleClosed);
                Assert.True(comp.StreamedFiles.ContainsKey(uid));
                comp.StreamedFiles[uid].PartsWritten++;
            }

            Assert.True(handleClosed);
            Assert.False(comp.StreamedFiles.ContainsKey(uid));
        }
    }
}

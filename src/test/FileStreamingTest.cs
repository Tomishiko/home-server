using System.Text;
using System.Text.Json;
namespace test;

public class FileStreamingTest
{
    [Fact]
    public void LargeFileUpload()
    {
        var helper = new StreamFileHelper();
        var filePath = @"D:\VS_projs\HomeServer\src\mvc_server\wwwroot\files\movies\JoJo no Kimyou na Bouken Part 4 Diamond wa Kudakenai Episode 27.mp4";
        helper.SplitFile(filePath);
    }

    //############# Helper class
    private class StreamFileHelper
    {
        private HttpClient client;

        public StreamFileHelper()
        {
            client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5019")
            };
            client.DefaultRequestHeaders.Add("User-agent", "StreamFileTest");
            client.DefaultRequestHeaders.Connection.Add("keep-alive");


        }

        public bool SplitFile(string filePath)
        {
            bool rslt = false;

            const int READBUFFER_SIZE = 1024 * 512;
            byte[] FSBuffer = new byte[READBUFFER_SIZE];

            // open the file to read it into chunks  
            var fileInfo = new FileInfo(filePath);

            using (FileStream FS = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // calculate the number of files that will be created  
                int TotalFileParts;
                int CurrentFilePart = 0;
                string uID;
                if (FS.Length < FSBuffer.Length)
                {
                    TotalFileParts = 1;
                }
                else
                {
                    float PreciseFileParts = ((float)FS.Length / (float)FSBuffer.Length);
                    TotalFileParts = (int)Math.Ceiling(PreciseFileParts);
                }
                //send file meta to the server
                var fileMeta = new Dictionary<string, string>
                {
                    { "fileName", fileInfo.Name },
                    {"fileSize",fileInfo.Length.ToString()},
                    { "totalParts",TotalFileParts.ToString() },
                    {"expectedPartSize", READBUFFER_SIZE.ToString()}
                };
                using (var formContent = new FormUrlEncodedContent(fileMeta))
                {
                    var requestUri = "api/streming/handshake";
                    var result = client.PostAsync(requestUri, formContent).Result;
                    if (!result.IsSuccessStatusCode)
                        return false;
                    else
                        uID = result.Content.ReadAsStringAsync().Result;
                }
                do
                {
                    int bytesRead = FS.Read(FSBuffer, 0, READBUFFER_SIZE);
                    bool result = false;
                    int attempts = 0;

                    while (!result || attempts <= 5)
                    {
                        result = sendFilePartHttp(FSBuffer, CurrentFilePart, TotalFileParts, FS.Name, uID);
                        attempts++;
                        if (attempts >= 5)
                        {

                            return false;
                        }


                    }

                    CurrentFilePart++;
                } while (FS.Position < FS.Length);

            }
            return true;
        }

        private bool sendFilePartHttp(byte[] data, int currentPart, int totalParts, string fname, string uID)
        {
            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(data);
                var meta = JsonSerializer.Serialize(
                    new Dictionary<string, object>(){
                        {"uid",uID},
                        {"currentPart",currentPart},
                        {"bytesRead",data.Length}
                    });
                var metaContent = new StringContent(meta, Encoding.UTF8, "application/json");
                content.Add(metaContent, "meta");
                content.Add(fileContent, "file", fname);

                var requestUri = "api/streming/uploadlarge";
                try
                {
                    var result = client.PostAsync(requestUri, content).Result;
                    int a = 0;
                    if (result.IsSuccessStatusCode)
                        return true;
                    else
                        return false;

                }
                catch (Exception ex)
                {
                    // log error  
                    return false;
                }
            }
            return true;
        }
    }

}
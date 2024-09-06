using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace test.Heplers;

public class StreamedFileHelper
{
    public HttpClient Client { get; set; }

    public StreamedFileHelper()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5019")
        };
        Client.DefaultRequestHeaders.Add("User-agent", "TestApp");
        Client.DefaultRequestHeaders.Connection.Add("keep-alive");


    }

    public bool SplitFile(string filePath, Func<byte[], int, int, string, bool> sendHttp)
    {
        bool rslt = false;

        const int READBUFFER_SIZE = 1024 * 1024;
        byte[] FSBuffer = new byte[READBUFFER_SIZE];
        // open the file to read it into chunks  
        using (FileStream FS = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            // calculate the number of files that will be created  
            int TotalFileParts;
            if (FS.Length < FSBuffer.Length)
            {
                TotalFileParts = 1;
            }
            else
            {
                float PreciseFileParts = ((float)FS.Length / (float)FSBuffer.Length);
                TotalFileParts = (int)Math.Ceiling(PreciseFileParts);
            }

            int CurrentFilePart = 0;

            // scan through the file, and each time we get enough data to fill a chunk, write out that file  
            do
            {
                int bytesRead = FS.Read(FSBuffer, 0, READBUFFER_SIZE);
                sendHttp(FSBuffer, CurrentFilePart, TotalFileParts, FS.Name);
                CurrentFilePart++;
            } while (FS.Position < FS.Length);

        }
        return rslt;
    }

    public bool sendHttp(byte[] data, int currentPart, int totalParts, string fname)
    {
        using (var content = new MultipartFormDataContent())
        {
            var fileContent = new ByteArrayContent(data);
            // fileContent.Headers.ContentDisposition = new
            //     ContentDispositionHeaderValue("file")
            // {
            //     FileName = Path.GetFileName("file")
            // };
            var parts = JsonSerializer.Serialize<(int, int)>((currentPart, totalParts), new JsonSerializerOptions { IncludeFields = true });
            var metaContent = new StringContent(parts, Encoding.UTF8, "application/json");
            content.Add(metaContent, "meta");
            content.Add(fileContent, "file", fname);

            var requestUri = "api/streming/uploadlarge";
            try
            {
                var result = Client.PostAsync(requestUri, content).Result;

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

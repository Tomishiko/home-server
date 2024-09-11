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
            BaseAddress = new Uri("http://homeserver.local")
        };
        Client.DefaultRequestHeaders.Add("User-agent", "TestApp");
        Client.DefaultRequestHeaders.Connection.Add("keep-alive");


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

            var fileMeta = new Dictionary<string, string>
            {
                { "fileName", fileInfo.Name },
                {"totalParts",TotalFileParts.ToString()},
                {"partSize", READBUFFER_SIZE.ToString()}
            };
            using (var formContent = new FormUrlEncodedContent(fileMeta))
            {
                var requestUri = "api/streming/handshake";
                var result = Client.PostAsync(requestUri, formContent).Result;
                if (!result.IsSuccessStatusCode)
                    return false;
                else
                    uID = result.Content.ReadAsStringAsync().Result;
            }

            // scan through the file, and each time we get enough data to fill a chunk, write out that file  
            do
            {
                int bytesRead = FS.Read(FSBuffer, 0, READBUFFER_SIZE);
                bool result;
                do
                {
                    result = sendFilePartHttp(FSBuffer, CurrentFilePart, TotalFileParts, FS.Name, uID);
                } while (!result);


                CurrentFilePart++;
            } while (FS.Position < FS.Length);

        }
        return rslt;
    }

    public bool sendFilePartHttp(byte[] data, int currentPart, int totalParts, string fname, string uID)
    {
        using (var content = new MultipartFormDataContent())
        {
            var fileContent = new ByteArrayContent(data);
            // fileContent.Headers.ContentDisposition = new
            //     ContentDispositionHeaderValue("file")
            // {
            //     FileName = Path.GetFileName("file")
            // };
            var parts = JsonSerializer.Serialize<(string, int)>((uID, currentPart), new JsonSerializerOptions { IncludeFields = true });
            var metaContent = new StringContent(parts, Encoding.UTF8, "application/json");
            content.Add(metaContent, "meta");
            content.Add(fileContent, "file", fname);

            var requestUri = "api/streming/uploadlarge";
            try
            {
                var result = Client.PostAsync(requestUri, content).Result;
                int a = 0;
                if (result.IsSuccessStatusCode)
                    return true;

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

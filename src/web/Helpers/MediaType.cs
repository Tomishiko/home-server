namespace web.Helpers;

public static class MediaTypeNames
{
    public static class Application
    {
        public const string Json = "application/json";
        public const string Xml = "application/xml";
        public const string OctetStream = "application/octet-stream";
    }

    public static class Text
    {
        public const string Plain = "text/plain";
        public const string Html = "text/html";
    }

    public static class Multipart
    {
        public const string FormData = "multipart/form-data";
    }
}

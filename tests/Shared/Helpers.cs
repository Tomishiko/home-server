using System.Security.Cryptography;
using System.Text;

namespace Shared.Helpers;

public static class Generators
{

    static public string RandomString32()
    {
        string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        byte[] inputBytes = Encoding.UTF8.GetBytes(timeStamp);
        byte[] hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}

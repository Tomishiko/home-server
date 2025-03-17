using System.Text;

namespace web.Services;
/// <summary>
/// Provides methods to get info of hosted files in specified directories.
/// </summary>
public interface ICoreFS
{
    public IEnumerable<FileInfo> GetIndexFiles { get; }
    public IEnumerable<FileInfo> GetMovies { get; }
    public FileInfo[] GetElements(string dirName);

}

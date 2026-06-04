namespace core.Services;

using System.Text;

/// <summary>
/// Provides methods to get info of hosted files in specified directories.
/// </summary>
[Obsolete]
public interface ICoreFS
{
    public IEnumerable<FileInfo> GetIndexFiles { get; }
    public IEnumerable<FileInfo> GetMovies { get; }
    public FileStream GetFileStream(string name);
    public FileInfo[] GetElements(string dirName);

}

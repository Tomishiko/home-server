namespace mvc_server.Services;

public interface ICoreFS
{
    public bool MovieUpdated { get; }
    public bool FileUpdated { get; }
    public FileInfo[] GetFiles { get; }
    public FileInfo[] GetMovies { get; }

}

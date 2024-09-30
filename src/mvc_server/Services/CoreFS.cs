namespace mvc_server.Services;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

public class CoreFS : ICoreFS
{
    private const string filesRelativePath = @"wwwroot/files";
    private const string moviesRelativePath = @"wwwroot/files/movies";
    private StringBuilder crawler;
    private readonly DirectoryInfo files = new DirectoryInfo(filesRelativePath);
    private readonly DirectoryInfo movies = new DirectoryInfo(moviesRelativePath);
    private DateTime lastFileUpdate;
    private DateTime lastMovieUpdate;
    private FileInfo[] FilesInfo;
    private FileInfo[] MoviesInfo;

    public CoreFS()
    {
        this.lastFileUpdate = files.LastWriteTime;
        this.lastMovieUpdate = movies.LastWriteTime;
        crawler = new StringBuilder(@"wwwroot/files");
        UpdateFiles();
        UpdateMovies();
    }
    private FileInfo[] GetElements(string dirPath)
    {
        string[] items = Directory.GetFiles(dirPath);
        FileInfo[] files = new FileInfo[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            files[i] = new FileInfo(items[i]);
        }
        return files;
    }
    private void UpdateFiles()
    {
        this.FilesInfo = GetElements("wwwroot/files");

    }
    private void UpdateMovies()
    {
        this.MoviesInfo = GetElements("wwwroot/files/movies");
    }
    public FileInfo[] GetFiles
    {
        get
        {
            files.Refresh();
            return files.GetFiles();
        }
    }
    public FileInfo[] GetMovies
    {
        get
        {
            //if (MovieUpdated)
            //{
            //UpdateMovies();
            //}
            movies.Refresh();

            return movies.GetFiles();
        }
    }
    public bool FileUpdated
    {
        get
        {
            files.Refresh();
            if (this.lastFileUpdate != files.LastWriteTime)
                return true;
            else
                return false;
        }
    }
    public bool MovieUpdated
    {

        get
        {
            movies.Refresh();
            if (this.lastMovieUpdate != movies.LastWriteTime)
                return true;
            else
                return false;
        }
    }

}

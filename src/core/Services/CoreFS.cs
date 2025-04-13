namespace core.Services;

using System.IO;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Represents the core file system service that manages hosted files.
/// Movies and index cached
/// </summary>
public class CoreFS : ICoreFS
{
    private const string filesRelativePath = @"wwwroot/files";
    private const string moviesRelativePath = @"wwwroot/files/movies";
    private readonly TimeSpan cacheTime = TimeSpan.FromMinutes(30);
    private readonly DirectoryInfo files = new DirectoryInfo(filesRelativePath);
    private readonly DirectoryInfo movies = new DirectoryInfo(moviesRelativePath);
    private readonly IMemoryCache _memCache;
    private DateTime lastFileUpdate;
    private DateTime lastMovieUpdate;
    private FileSystemWatcher _indexWatcher = new FileSystemWatcher(filesRelativePath);
    private FileSystemWatcher _movieWatcher = new FileSystemWatcher(moviesRelativePath);


    public CoreFS(IMemoryCache memCache)
    {
        this.lastFileUpdate = files.LastWriteTime;
        this.lastMovieUpdate = movies.LastWriteTime;
        this._memCache = memCache;
        SetWatchers();
    }

    private void SetWatchers()
    {
        _indexWatcher.EnableRaisingEvents = true;
        _indexWatcher.NotifyFilter = NotifyFilters.LastWrite;
        _indexWatcher.Changed += OnFilesChanged;
        _movieWatcher.NotifyFilter = NotifyFilters.Size;


        _movieWatcher.EnableRaisingEvents = true;
        _movieWatcher.NotifyFilter = NotifyFilters.LastWrite;
        _movieWatcher.NotifyFilter = NotifyFilters.Size;
        _movieWatcher.Changed += OnFilesChanged;

    }

    private void OnFilesChanged(object sender, FileSystemEventArgs e)
    {
        if (sender is FileSystemWatcher watcher)
        {
            var path = watcher.Path;
            if (_memCache.TryGetValue(path, out _))
                _memCache.Set(path, GetElements(path), cacheTime);
        }


    }

    public FileInfo[] GetElements(string dirPath)
    {
        string[] items = Directory.GetFileSystemEntries(dirPath);
        FileInfo[] files = new FileInfo[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            files[i] = new FileInfo(items[i]);
        }
        return files;
    }
    public IEnumerable<FileInfo> GetIndexFiles
    {
        get
        {
            if (!_memCache.TryGetValue<FileInfo[]>(filesRelativePath, out var temp))
            {
                temp = GetElements(filesRelativePath);
                _memCache.Set(filesRelativePath, temp, cacheTime);
            }
            return temp;
        }
    }
    public IEnumerable<FileInfo> GetMovies
    {
        get
        {

            if (!_memCache.TryGetValue<FileInfo[]>(moviesRelativePath, out var temp))
            {
                temp = GetElements(moviesRelativePath);
                _memCache.Set(moviesRelativePath, temp, cacheTime);
            }
            return temp;
        }
    }

}

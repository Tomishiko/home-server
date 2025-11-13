using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using core.Models;

namespace core.Services;


/// <summary>
/// Represents the core file system service that manages hosted files.
/// </summary>
public class CoreFS : ICoreFS
{
    private readonly string filesRelativePath;
    private readonly string moviesRelativePath;
    private readonly TimeSpan _cacheTime = TimeSpan.FromMinutes(30);
    private readonly DirectoryInfo _files;
    private readonly IMemoryCache _memCache;
    private DateTime _lastFileUpdate;
    private DateTime _lastMovieUpdate;
    private FileSystemWatcher _indexWatcher;
    private FileSystemWatcher _movieWatcher;


    public CoreFS(IMemoryCache memCache, IOptions<FileUploadOptions> uploadOptions)
    {
        var cfg = uploadOptions.Value;
        filesRelativePath = cfg.StoragePath;

        _files = new DirectoryInfo(filesRelativePath);
        _indexWatcher = new FileSystemWatcher(filesRelativePath);

        _lastFileUpdate = _files.LastWriteTime;
        _memCache = memCache;
        SetWatchers();
    }

    private void SetWatchers()
    {
        _indexWatcher.EnableRaisingEvents = true;
        _indexWatcher.NotifyFilter = NotifyFilters.LastWrite;
        _indexWatcher.Changed += OnFilesChanged;

    }

    private void OnFilesChanged(object sender, FileSystemEventArgs e)
    {
        if (sender is FileSystemWatcher watcher)
        {
            var path = watcher.Path;
            if (_memCache.TryGetValue(path, out _))
                _memCache.Set(path, GetElements(path), _cacheTime);
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
                _memCache.Set(filesRelativePath, temp, _cacheTime);
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
                _memCache.Set(moviesRelativePath, temp, _cacheTime);
            }
            return temp;
        }
    }

    public FileStream GetFileStream(string fileName)
    {
        return new FileStream($"{_files}/{fileName}", FileMode.Open);
    }

}

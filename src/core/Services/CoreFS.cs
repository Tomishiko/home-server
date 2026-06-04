using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using core.Models;

namespace core.Services;


/// <summary>
/// Represents the core file system service that manages hosted files.
/// </summary>
[Obsolete]
public class CoreFS : ICoreFS
{
    private readonly string _filesPath;
    private readonly string _moviesRelativePath;
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
        _filesPath = cfg.StoragePath;

        _files = new DirectoryInfo(_filesPath);
        _indexWatcher = new FileSystemWatcher(_filesPath);

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
            if (!_memCache.TryGetValue<FileInfo[]>(_filesPath, out var temp))
            {
                temp = GetElements(_filesPath);
                _memCache.Set(_filesPath, temp, _cacheTime);
            }
            return temp;
        }
    }

    public IEnumerable<FileInfo> GetMovies
    {
        get
        {

            if (!_memCache.TryGetValue<FileInfo[]>(_moviesRelativePath, out var temp))
            {
                temp = GetElements(_moviesRelativePath);
                _memCache.Set(_moviesRelativePath, temp, _cacheTime);
            }
            return temp;
        }
    }

    public FileStream GetFileStream(string fileName)
    {
        return new FileStream($"{_files}/{fileName}", FileMode.Open);
    }

}

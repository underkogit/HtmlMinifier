using HtmlMinifier.Interfaces;
using HtmlMinifier.Structures;
using Newtonsoft.Json;

namespace HtmlMinifier;

public class HtmlProcessor : IDisposable
{
    private string _pathJsonFile = "Options.json";
    private JsonOptions _jsonOptions = new JsonOptions();
    public string BaseDirectory { get; set; } = String.Empty;
    public bool IsLoaded { get; private set; }
    private readonly List<INUglifyProcess> _processes = [];
    private FileSystemWatcher _watcher;

    public bool Loaded(string pathJsonFile = "Options.json")
    {
        if (Path.GetFullPath(pathJsonFile) is { } path && File.Exists(path))
        {
            _pathJsonFile = path;

            if (File.ReadAllText(_pathJsonFile) is { } jsonData && !string.IsNullOrEmpty(jsonData))
            {
                JsonOptions? deserializeObject = JsonConvert.DeserializeObject<JsonOptions>(jsonData);
                if (deserializeObject != null && File.Exists(deserializeObject.PathHtmlFile) &&
                    File.Exists(deserializeObject.PathOutputHtmlFile))
                {
                    _jsonOptions = deserializeObject;
                    if (!string.IsNullOrEmpty(_jsonOptions.PathHtmlFile) &&
                        Path.GetFullPath(Path.GetDirectoryName(_jsonOptions.PathHtmlFile) ?? string.Empty) is
                            { } directory &&
                        Directory.Exists(directory))
                    {
                        BaseDirectory = directory;
                        return (IsLoaded = true);
                    }
                }
            }
        }
        else
        {
            File.WriteAllText(_pathJsonFile, JsonConvert.SerializeObject(new JsonOptions()
            {
                PathHtmlFile = "Options.html",
                PathOutputHtmlFile = "Options.html",
                Content = "Options.html",
            }, Formatting.Indented));
        }

        return (IsLoaded = false);
    }

    public void AddInUglify(INUglifyProcess process)
    {
        process.AddBaseDirectory(BaseDirectory);
        _processes.Add(process);
    }

    public void Build()
    {
        if (!IsLoaded)
            return;

        try
        {
            _jsonOptions.Content = File.ReadAllText(_jsonOptions.PathHtmlFile);
            foreach (var nUglifyProcess in _processes)
            {
                _jsonOptions.Content = nUglifyProcess.Call(_jsonOptions.Content);
            }

            File.WriteAllText(_jsonOptions.PathOutputHtmlFile, _jsonOptions.Content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
        }
    }

    public void StartTask()
    {
        if (!IsLoaded)
            return;
        var directoryPath = Path.GetDirectoryName(_jsonOptions.PathHtmlFile);
        if (!Directory.Exists(directoryPath))
            return;


        _watcher = _watcher ?? new FileSystemWatcher(directoryPath);

        _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
        _watcher.Filter = "*.*";
        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Deleted += OnChanged;
        _watcher.EnableRaisingEvents = true;
        Console.WriteLine("Нажмите 'q' для выхода.");
        while (Console.Read() != 'q') ;
    }

    void OnChanged(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"Файл: {e.FullPath} был {e.ChangeType}");
        Task.Delay(150);
        Build();
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
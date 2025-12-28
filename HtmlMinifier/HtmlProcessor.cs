using HtmlMinifier.Interfaces;
using HtmlMinifier.NUglifys;
using HtmlMinifier.Structures;
using Newtonsoft.Json;
using OllamaNUglifys;

namespace HtmlMinifier;

public class HtmlProcessor : IDisposable
{
    private string _pathJsonFile = "Options.json";
    public JsonOptions JsonOptions = new JsonOptions();
    public string BaseDirectory { get; set; } = String.Empty;
    public bool IsLoaded { get; private set; }
    private readonly List<INUglifyProcess> _processes = [];
    private FileSystemWatcher _watcher;

    public bool Mode { get; set; } = true;

    public bool Loaded(string pathJsonFile = "Options.json")
    {
        if (Path.GetFullPath(pathJsonFile) is { } path && File.Exists(path))
        {
            _pathJsonFile = path;

            if (File.ReadAllText(_pathJsonFile) is { } jsonData && !string.IsNullOrEmpty(jsonData))
            {
                JsonOptions? deserializeObject = JsonConvert.DeserializeObject<JsonOptions>(jsonData);
                if (deserializeObject != null && File.Exists(deserializeObject.PathHtmlFile))

                {
                    JsonOptions = deserializeObject;
                    if (!string.IsNullOrEmpty(JsonOptions.PathHtmlFile) &&
                        Path.GetFullPath(Path.GetDirectoryName(JsonOptions.PathHtmlFile) ?? string.Empty) is
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

    public async Task Build()
    {
        if (!IsLoaded)
            return;

        try
        {
            JsonOptions.Content = File.ReadAllText(JsonOptions.PathHtmlFile);
            var typeName = typeof(NUglifyConvertCppHeader);
            string contentBase = "";
            foreach (var nUglifyProcess in _processes)
            {
                var cTypeName = nUglifyProcess.GetType();
                if (typeName == cTypeName && Mode)
                    contentBase = JsonOptions.Content;

                JsonOptions.Content = await nUglifyProcess.Call(JsonOptions.Content);
            }

            var directory = Path.GetDirectoryName(JsonOptions.PathOutputHtmlFile);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(JsonOptions.PathOutputHtmlFile, contentBase);
            await File.WriteAllTextAsync(JsonOptions.PathOutputHeaderFile, JsonOptions.Content);
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
        var directoryPath = Path.GetDirectoryName(JsonOptions.PathHtmlFile);
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
        _ = Build();
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        foreach (var nUglifyProcess in _processes)
        {
            nUglifyProcess.Dispose();
        }
    }
}
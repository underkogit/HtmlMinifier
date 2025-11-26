using System.Runtime.Serialization.Json;
using HtmlMinifier;
using HtmlMinifier.NUglifys;

using (HtmlProcessor processor = new HtmlProcessor())
{
    if (processor.Loaded())
    {
        processor.Mode = true;
        Console.WriteLine("Loaded HtmlMinifier...");
        processor.AddInUglify(new NUglifyHtml());
        processor.AddInUglify(new NUglifyExtractorJavaScript());
        processor.AddInUglify(new NUglifyExtractorCssStyle());
        
        processor.AddInUglify(new NUglifyConvertCppHeader());
        await processor.Build();
        processor.StartTask();
    }
    else
    {
        Console.WriteLine("Error Loading HtmlMinifier...");
    }
}


// var pathHtmlFile = @"C:\Users\UnderKo\Documents\PlatformIO\Projects\ESP32WebPanel\src\WebUI\index.html";
// var pathOutputHtmlFile = @"C:\Users\UnderKo\Documents\PlatformIO\Projects\ESP32WebPanel\include\webui.h";
//
//
//
//
//
//
//
// if (!string.IsNullOrEmpty(pathHtmlFile))
// {
//     var directoryPath = Path.GetDirectoryName(pathHtmlFile);
//     Console.WriteLine($"Отслеживание изменений в директории: {directoryPath}");
//     Build();
//
//     using (var watcher = new FileSystemWatcher(directoryPath))
//     {
//         watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
//         watcher.Filter = "*.*";
//         watcher.Changed += OnChanged;
//         watcher.Created += OnChanged;
//         watcher.Deleted += OnChanged;
//         watcher.EnableRaisingEvents = true;
//         Console.WriteLine("Нажмите 'q' для выхода.");
//         while (Console.Read() != 'q') ;
//     }
// }
//
// void OnChanged(object sender, FileSystemEventArgs e)
// {
//     Console.WriteLine($"Файл: {e.FullPath} был {e.ChangeType}");
//     Build();
// }
//
// void Build()
// {
//     Thread.Sleep(200);
//     var processor = new HtmlScriptProcessor(pathHtmlFile, pathOutputHtmlFile);
//     processor.ProcessHtml();
// }
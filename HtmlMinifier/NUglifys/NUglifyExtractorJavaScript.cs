using System.Text.RegularExpressions;
using HtmlMinifier.Interfaces;
using NUglify;
using Ollama;
using OllamaNUglifys;

namespace HtmlMinifier.NUglifys;

public class NUglifyExtractorJavaScript : INUglifyProcess
{
    private string BaseDirectory { get; set; } = String.Empty;
    private string _cacheDirectory = "cache";

    public NUglifyExtractorJavaScript()
    {
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    public async Task<string> Call(string content)
    {
        if (!Directory.Exists(BaseDirectory))
            return await Task.FromResult<string>(null);
        var scripts = await ExtractJsScripts(content);

        var resultStr = ReplaceAllScriptsEmpty(content, scripts);
        return await Task.FromResult<string>(resultStr);
    }

    public void AddBaseDirectory(string directory)
    {
        BaseDirectory = directory;
    }


    private string ReplaceAllScriptsEmpty(string content, string[] scripts)
    {
        content = Regex.Replace(content, "((<script>)|(<script))[\\w\\W]+?<\\/script>", string.Empty);
        content += $" <script>{NUglify.Uglify.Js(string.Join(" ", scripts)).Code}</script>";

        return content;
    }


    private async Task<string[]> ExtractJsScripts(string content)
    {
        var scripts = Regex.Matches(content, "((<script>)|(<script))[\\w\\W]+?<\\/script>").Select(a => a.Value)
            .ToList();

        var scriptsCode = new List<string>();

        foreach (var script1 in scripts)
            if (Regex.IsMatch(script1, @"((<script>)|(<script))[\w\W]+?><\/script>"))
            {
                var nameJsFile = Regex.Match(script1, @"(src=?"".+?"")|(src=.+?\.js)").Value;

                var srcPath = Path.GetFullPath(
                    Path.Combine(BaseDirectory, nameJsFile
                        .Replace("src=\"", string.Empty).Replace("src=", string.Empty)
                        .Replace("\"", string.Empty))
                );

                if (File.Exists(srcPath))
                {
                    scriptsCode.Add(File.ReadAllText(srcPath));
                }
                else
                {
                    srcPath = Regex.Replace(script1, "(<script src=)|(defer></script>)|(></script>)", String.Empty);
                    if (srcPath.StartsWith("http"))
                    {
                        string nameFile = Regex.Replace(srcPath, "[\\W]+", string.Empty);
                        nameFile = Path.GetFullPath(
                            Path.Combine(_cacheDirectory, nameFile));
                        if (!File.Exists(nameFile))
                        {
                            var data = await JsDownload(srcPath);
                            if (!string.IsNullOrEmpty(data))
                                await File.WriteAllTextAsync(nameFile, data);

                            scriptsCode.Add(data);
                        }
                        else
                        {
                            var data = await File.ReadAllTextAsync(nameFile);
                            if (!string.IsNullOrEmpty(data))
                                scriptsCode.Add(data);
                        }
                    }


                    srcPath = Path.GetFullPath(
                        Path.Combine(BaseDirectory, srcPath));
                    if (File.Exists(srcPath))
                        scriptsCode.Add(File.ReadAllText(srcPath));
                }
            }
            else
            {
                scriptsCode.Add(script1.Replace("<script>", string.Empty).Replace("</script>", string.Empty));
            }
        
        // await File.WriteAllTextAsync(Path.GetFullPath(
        //     Path.Combine(_cacheDirectory, "alljs.js")), string.Join("\n///================\n" , scriptsCode.ToArray()));
        
        return await Task.FromResult(scriptsCode.ToArray());
    }

    async Task<string> JsDownload(string host)
    {
        try
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, host);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            return string.Empty;
        }
    }

    public void Dispose()
    {
    }
}
using System.Text.RegularExpressions;
using HtmlMinifier.Interfaces;
using NUglify;
using Ollama;
using OllamaNUglifys;

namespace HtmlMinifier.NUglifys;

public class NUglifyExtractorJavaScript : INUglifyProcess
{
    private string BaseDirectory { get; set; } = String.Empty;


    public async Task<string> Call(string content)
    {
        if (!Directory.Exists(BaseDirectory))
            return await Task.FromResult<string>(null);
        var scripts = ExtractJsScripts(content);

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


    private string[] ExtractJsScripts(string content)
    {
        var scripts = Regex.Matches(content, "((<script>)|(<script))[\\w\\W]+?<\\/script>").Select(a => a.Value)
            .ToList();

        var scriptsCode = new List<string>();

        foreach (var script1 in scripts)
            if (Regex.IsMatch(script1, @"((<script>)|(<script))[\w\W]+?><\/script>"))
            {


                var nameJsFile = Regex.Match(script1, @"(src=?"".+?"")|(src=.+?\.js)").Value;
                
                var srcPath = Path.GetFullPath(
                    Path.Combine(BaseDirectory,nameJsFile
                        .Replace("src=\"", string.Empty).Replace("src=", string.Empty)
                            .Replace("\"", string.Empty))
                );
                
                if (File.Exists(srcPath))
                {
                    scriptsCode.Add(File.ReadAllText(srcPath));
                }
                else
                {
                    srcPath = script1.Replace("<script src=", string.Empty).Replace("defer></script>", string.Empty);
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


        return scriptsCode.ToArray();
    }

    public void Dispose()
    {
    }
}
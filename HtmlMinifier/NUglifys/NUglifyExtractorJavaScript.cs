using System.Text.RegularExpressions;
using HtmlMinifier.Interfaces;
using NUglify;

namespace HtmlMinifier.NUglifys;



public class NUglifyExtractorJavaScript : INUglifyProcess
{
    private string BaseDirectory { get; set; } = String.Empty;

    public string Call(string content)
    {
        if (!Directory.Exists(BaseDirectory))
            return content;
        var scripts = ExtractJsScripts(content);
        return ReplaceAllScriptsEmpty(content, scripts) ;
    }

    public void AddBaseDirectory(string directory)
    {
        BaseDirectory = directory;
    }


    private string ReplaceAllScriptsEmpty(string content, string[] scripts)
    {
        content = Regex.Replace(content, "((<script>)|(<script))[\\w\\W]+?<\\/script>", string.Empty);
        content += $" <script>{string.Join(" ", scripts)}</script>";

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
                var srcPath = Path.GetFullPath(
                    Path.Combine(BaseDirectory,
                        Regex.Match(script1, @"src=?"".+?""").Value.Replace("src=\"", string.Empty)
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

        return scriptsCode.AsParallel().Select(script => NUglify.Uglify.Js(script).Code).ToArray();
    }
}
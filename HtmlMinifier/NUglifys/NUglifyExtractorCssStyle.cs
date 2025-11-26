using System.Text.RegularExpressions;
using HtmlMinifier.Interfaces;
using NUglify;

namespace HtmlMinifier.NUglifys;

public class NUglifyExtractorCssStyle : INUglifyProcess
{
    private string BaseDirectory { get; set; } = String.Empty;


    public Task<string> Call(string content)
    {
        if (!Directory.Exists(BaseDirectory))
            return Task.FromResult<string>(content);
        return Task.FromResult<string>(ReplaceAllCssEmpty(content, ExtractCssStyles(content)));
    }

    private string ReplaceAllCssEmpty(string content, string[] csses)
    {
        var css = RemoveDuplicates(csses);

        content = Regex.Replace(content, "(((<link>)|(<link)).+?>)|(((<style>)|(<style))[\\w\\W]+?<\\/style>)",
            string.Empty);
        content += $"<style>{css}</style>";
        return content;
    }

    private string RemoveDuplicates(string[] csses)
    {
        // [\w:.-]+?{.+?}

        var css = Uglify.Css(string.Join(" ", csses)).Code;
        var elements = Regex.Matches(css, "[\\w:.-]+?{.+?}").AsParallel()
            .Select(m => m.Value)
            .ToList().GroupBy(x => x)
            //.Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .Distinct();

        return string.Join(" ", elements);
    }

    public void AddBaseDirectory(string directory)
    {
        BaseDirectory = directory;
    }

    private string[] ExtractCssStyles(string content)
    {
        var cssStyles = Regex
            .Matches(content, "((<style>)|(<style))[\\w\\W]+?<\\/style>").AsParallel()
            .Select(a => Regex.Replace(a.Value, "(<style>)|(</style>)", string.Empty))
            .ToList();

        foreach (var cssFile in Regex
                     .Matches(content, "((<link>)|(<link))[\\w\\W]+?(<\\/link>|>)")
                     .Select(a => a.Value)
                     .ToList())
        {
            if (Regex.Match(cssFile, "href=.+?.css").Value is { } path && !string.IsNullOrEmpty(path))
            {
                path = Path.GetFullPath(Path.Combine(BaseDirectory,
                    path.Replace("\"", string.Empty).Replace("href=", string.Empty)));
                if (File.Exists(path) && File.ReadAllText(path) is { } fileContent &&
                    !string.IsNullOrEmpty(fileContent))
                {
                    cssStyles.Add(fileContent);
                }
            }
        }

        return cssStyles.AsParallel().Select(script => NUglify.Uglify.Css(script).Code).ToArray();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
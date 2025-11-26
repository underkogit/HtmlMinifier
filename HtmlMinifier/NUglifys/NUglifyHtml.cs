using HtmlMinifier.Interfaces;

namespace HtmlMinifier.NUglifys;

public class NUglifyHtml : INUglifyProcess
{
    public Task<string> Call(string content)
    {
        return Task.FromResult<string>(NUglify.Uglify.Html(content.Trim()).Code);
    }

    public void AddBaseDirectory(string directory)
    {
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
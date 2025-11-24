using HtmlMinifier.Interfaces;

namespace HtmlMinifier.NUglifys;

public class NUglifyHtml : INUglifyProcess
{
    public string Call(string content)
    {
        return NUglify.Uglify.Html(content.Trim()).Code;
    }
    
    public void AddBaseDirectory(string directory)
    {
         
    }
}
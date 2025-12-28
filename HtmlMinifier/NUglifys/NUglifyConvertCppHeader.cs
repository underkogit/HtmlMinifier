using HtmlMinifier.Interfaces;

namespace HtmlMinifier.NUglifys;

public class NUglifyConvertCppHeader : INUglifyProcess
{
    private string _nameParametr = String.Empty;

    public Task<string> Call(string content)
    {
        return Task.FromResult<string>(@"
 
 
const char* " + _nameParametr + @" = R""rawliteral(
{{WEBUI}}
)rawliteral"";
 
".Replace("{{WEBUI}}", content));
    }

    public NUglifyConvertCppHeader AddParametrName(string nameParametr)
    {
        _nameParametr = nameParametr;
        return this; 
    }

    public void AddBaseDirectory(string directory)
    {
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
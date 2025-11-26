using HtmlMinifier.Interfaces;

namespace HtmlMinifier.NUglifys;

public class NUglifyConvertCppHeader : INUglifyProcess
{
    public Task<string> Call(string content)
    {
        return Task.FromResult<string>(@"
#ifndef WEBUI_H
#define WEBUI_H
const char* htmlContent = R""rawliteral(
{{WEBUI}}
)rawliteral"";
#endif
".Replace("{{WEBUI}}", content));
    }


    public void AddBaseDirectory(string directory)
    {
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
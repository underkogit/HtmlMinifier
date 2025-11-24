using HtmlMinifier.Interfaces;

namespace HtmlMinifier.NUglifys;

public class NUglifyConvertCppHeader : INUglifyProcess
{
    public string Call(string content)
    {
        return @"
#ifndef WEBUI_H
#define WEBUI_H
const char* htmlContent = R""rawliteral(
{{WEBUI}}
)rawliteral"";
#endif
".Replace("{{WEBUI}}", content);
    }


    public void AddBaseDirectory(string directory)
    {
    }
}
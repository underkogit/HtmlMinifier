namespace HtmlMinifier.Interfaces;

public interface INUglifyProcess : IDisposable
{
    public Task<string> Call(string content);

    public void AddBaseDirectory(string directory);
    
    
}
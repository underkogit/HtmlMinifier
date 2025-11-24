namespace HtmlMinifier.Interfaces;

public interface INUglifyProcess
{
    public string Call( string content );

    public void AddBaseDirectory( string directory );

}
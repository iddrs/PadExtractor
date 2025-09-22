using System.Xml;

namespace PadExtractor.Layout;

/**
 * Representa um arquivo de layout.
 */
public class Layout
{
    private readonly string layoutpath;
    private readonly XmlDocument xml;


    public Layout(string layoutpath)
    {
        this.layoutpath = layoutpath ?? throw new ArgumentNullException(nameof(layoutpath));
        if (!File.Exists(layoutpath))
        {
            throw new FileNotFoundException(layoutpath);
        }
        xml = new XmlDocument();
        xml.Load(layoutpath);
    }

    public XmlNodeList GetCollumns()
    {
        return xml.DocumentElement.SelectNodes("//col");
    }
}
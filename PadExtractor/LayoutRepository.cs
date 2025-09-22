namespace PadExtractor.Layout;

/**
 * Representa o conjunto de layouts.
 */
public class LayoutRepository
{
    /**
     * Diretório onde estão os arquivos de layout.
     */
    private readonly string layoutDir;

    public LayoutRepository(string layoutDir)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layoutDir, nameof(layoutDir));
        this.layoutDir = layoutDir;
        if(!Directory.Exists(layoutDir))
        { 
            throw new DirectoryNotFoundException(layoutDir);
        }
    }

    /**
     * Layout para um Source.
     */
    public Layout GetLayoutFor(Source.Source source)
    {
        string layoutpath = Path.Combine(this.layoutDir, $"{source.GetBaseName()}.xml");
        if (!File.Exists(layoutpath))
        {
            throw new FileNotFoundException(layoutpath);
        }
        return new Layout(layoutpath);
    }
}
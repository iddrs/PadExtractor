using System.IO;

namespace PadExtractor.Source;

/**
 * Repositório de arquivos *.txt do PAD.
 *
 */
public class SourceRepository
{
    public readonly string[] sourceDirs;

    public SourceRepository(string[] sourceDirs)
    {
        this.sourceDirs = sourceDirs ?? throw new ArgumentNullException(nameof(sourceDirs));
        foreach (string dir in sourceDirs)
        {
            if (!Directory.Exists(dir))
            {
                throw new DirectoryNotFoundException(dir);
            }
        }
    }

    /**
     * Retorna uma instância de PadExtractor.Source.Source para `filepath`.
     */
    public Source GetSourceFor(string filepath)
    {
        return new Source(filepath);
    }

}

namespace PadExtractor;

/**
 * Interface para o monitor de progresso.
 */
public interface IProgressMonitor
{
    public void UpdateProgress(int current, string? message = null);
}

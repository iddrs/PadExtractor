using System;
using System.Collections.Generic;
using System.Text;

namespace PadExtractor
{
    public interface IProgressMonitor
    {
        public void UpdateProgress(int current, string? message = null);
    }
}

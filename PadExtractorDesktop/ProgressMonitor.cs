using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using PadExtractor;
using System.Runtime.CompilerServices;

namespace PadExtractorDesktop
{
    public class ProgressMonitor : INotifyPropertyChanged, PadExtractor.IProgressMonitor
    {
        private int progressValue;
        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                if(progressValue != value)
                {
                    progressValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? progressMessage;

        public string? ProgressMessage
        {
            get { return progressMessage; }
            set
            {
                if(progressMessage != value)
                        {
                    progressMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateProgress(int current, string? message = null)
        {
            ProgressValue = current;
            ProgressMessage = message;
        }
    }
}

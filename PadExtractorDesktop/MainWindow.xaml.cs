using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using PadExtractor.Layout;
using PadExtractor.Process;
using PadExtractor.Source;
using PadExtractor.Writer;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PadExtractorDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string _connStr = "";
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ProgressMonitor();

            var config = new ConfigurationBuilder()
                .AddIniFile("lastdir.ini", optional: false, reloadOnChange: true)
                .Build();
            ExecutivoSourceDirectoryEntry.Text = config["ExecutivoSourceDirectoryPath"];
            LegislativoSourceDirectoryEntry.Text = config["LegislativoSourceDirectoryPath"];
        }

        private void OpenExecutivoDirectoryDialog(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Selecione a pasta dos arquivos do Executivo";
            bool? result = dialog.ShowDialog();
            if(result == true)
            {
                ExecutivoSourceDirectoryEntry.Text = dialog.FolderName;
            }
        }
        
        private void OpenLegislativoDirectoryDialog(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Selecione a pasta dos arquivos do Legislativo";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                LegislativoSourceDirectoryEntry.Text = dialog.FolderName;
            }
        }

        private void SaveLastDirectories()
        {
            using (StreamWriter writer = new StreamWriter("lastdir.ini"))
            {
                writer.WriteLine($"ExecutivoSourceDirectoryPath={ExecutivoSourceDirectoryEntry.Text}");
                writer.WriteLine($"LegislativoSourceDirectoryPath={LegislativoSourceDirectoryEntry.Text}");
                writer.Flush();
                writer.Close();
            }
        }

        private async void StartProcess(object sender, RoutedEventArgs e)
        {
            if(!Directory.Exists(ExecutivoSourceDirectoryEntry.Text) || !Directory.Exists(LegislativoSourceDirectoryEntry.Text))
            {
                MessageBox.Show("Um dos diretórios de arquivos *.txt do PAD é inválido ou não existe.");
                return;
            }

            if (ExecutivoSourceDirectoryEntry.Text == LegislativoSourceDirectoryEntry.Text)
            {
                MessageBox.Show("Os dois diretórios não podem ser iguais!");
                return;
            }

            SaveLastDirectories();

            if (this.DataContext is ProgressMonitor monitor)
            {
                //var config = new ConfigurationBuilder()
                //.AddJsonFile("appconfig.json", optional: false, reloadOnChange: true)
                //.Build();
                //string[] sourceDirs = [ExecutivoSourceDirectoryEntry.Text, LegislativoSourceDirectoryEntry.Text];
                //SourceRepository sourceRepository = new PadExtractor.Source.SourceRepository(sourceDirs);
                //LayoutRepository layoutRepository = new PadExtractor.Layout.LayoutRepository(config["Path:LayoutDir"]);
                //WriterRepository writerRepository = new WriterRepository(config["Db:ConnectionStr"]);
                var config = new ConfigurationBuilder()
                .AddIniFile("appconfig.ini", optional: false, reloadOnChange: true)
                .Build();
                _connStr = $"Host={config["DbHost"]};Username={config["DbUser"]};Password={config["DbPassword"]};Database={config["DbName"]}";
                string[] sourceDirs = [ExecutivoSourceDirectoryEntry.Text, LegislativoSourceDirectoryEntry.Text];
                SourceRepository sourceRepository = new PadExtractor.Source.SourceRepository(sourceDirs);
                LayoutRepository layoutRepository = new PadExtractor.Layout.LayoutRepository(config["LayoutDir"]);
                WriterRepository writerRepository = new WriterRepository(_connStr);

                Processor processor = new Processor(sourceRepository, layoutRepository, writerRepository, monitor);
                this.Cursor = Cursors.Wait;
                StartButton.IsEnabled = false;
                try
                {
                    await Task.Run(processor.Process);

                    this.Progressbar.IsIndeterminate = true;
                    monitor.ProgressMessage = "Processando os restos a pagar agora. Isso pode demorar um pouco...";
                    await Task.Run(() =>
                    {
                        RestosPagarProcessor rp = new RestosPagarProcessor(_connStr);
                        rp.MontaRestosPagar(writerRepository.remessa);
                    });
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                    StartButton.IsEnabled = true;
                    this.Progressbar.IsIndeterminate = false;
                    monitor.ProgressMessage = "Arquivos *.txt do PAD convertidos e salvos no banco de dados!";
                    MessageBox.Show("Processamento concluído!");
                }

            }
        }

        private void btnOpenConfig_Click(object sender, RoutedEventArgs e)
        {
            var wndConfig = new ConfigWindow();
            wndConfig.Owner = this;
            wndConfig.ShowDialog();
        }
    }
}
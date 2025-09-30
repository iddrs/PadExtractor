using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PadExtractorDesktop
{
    /// <summary>
    /// Lógica interna para ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            var config = new ConfigurationBuilder()
                .AddIniFile("appconfig.ini", optional: false, reloadOnChange: true)
                .Build();
            entryLayoutDirConfig.Text = config["LayoutDir"];
            entryHostConfig.Text = config["DbHost"];
            entryDbUserConfig.Text = config["DbUser"];
            entryDbPasswordConfig.Password = config["DbPassword"];
            entryDbNameconfig.Text = config["DbName"];
        }

        private void btnSelectLayoutDirConfig_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Selecione a pasta dos arquivos de layout";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                entryLayoutDirConfig.Text = dialog.FolderName;
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter writer = new StreamWriter("appconfig.ini"))
            {
                writer.WriteLine($"LayoutDir={entryLayoutDirConfig.Text}");
                writer.WriteLine($"DbHost={entryHostConfig.Text}");
                writer.WriteLine($"DbUser={entryDbUserConfig.Text}");
                writer.WriteLine($"DbPassword={entryDbPasswordConfig.Password}");
                writer.WriteLine($"DbName={entryDbNameconfig.Text}");
                writer.Flush();
                writer.Close();
            }
            MessageBox.Show("Configurações salvas!");
        }
    }
}

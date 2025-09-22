using System.Security.Cryptography;

namespace PadExtractor.Source
{
    /**
     * Representa um arquivo *.txt do PAD.
     */
    public class Source
    {
        /**
         * Caminho para o arquivo de dados do PAD.
         */
        public readonly string filepath;

        public readonly long fileSize;

        private readonly StreamReader reader;

        public Source(string filepath)
        {
            this.filepath = filepath ?? throw new ArgumentNullException(nameof(filepath));
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException(filepath);
            }

            FileInfo finfo = new FileInfo(filepath);
            this.fileSize = finfo.Length;

            this.reader = new StreamReader(filepath);
        }

        /**
         * Nome do arquivo sem a extenção.
         */
        public string GetBaseName()
        {
            return Path.GetFileNameWithoutExtension(this.filepath);
        }

        public string ReadNextLine()
        {
            return reader.ReadLine();
        }

        public void Dispose()
        {
            reader?.Close();
            reader?.Dispose();
        }
    }
}
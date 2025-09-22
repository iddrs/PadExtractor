using Npgsql;
using PadExtractor.Source;

namespace PadExtractor.Writer
{
    /**
     * Representa o banco de dados que receberá os dados convertidos.
     */
    public class WriterRepository
    {
        private readonly NpgsqlConnectionStringBuilder connStr;
        private NpgsqlConnection conn;
        private NpgsqlTransaction transaction;
        private List<string> tables = new List<string>();
        public string remessa;

        public WriterRepository(string connStr)
        {
            this.connStr = new NpgsqlConnectionStringBuilder(connStr);
            this.Connect();
        }

        private void Connect()
        {
            this.conn = new NpgsqlConnection(this.connStr.ToString());
            this.conn.Open();
        }

        public void BeginTransaction()
        {
            this.transaction = this.conn.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                this.transaction.Commit();

                this.BeginTransaction();
                foreach (string tableName in this.tables)
                {
                    DeleteOldData(tableName);
                }
                this.transaction.Commit();

                this.BeginTransaction();
                foreach (string tableName in this.tables)
                {
                    UpdateRemessa(tableName);
                }
                this.transaction.Commit();
            }
            catch (Exception)
            {
                this.transaction.Rollback();
                throw;
            }
        }

        public Writer GetWriterFor(Source.Source source)
        {
            ArgumentNullException.ThrowIfNull(source);
            string baseName = source.GetBaseName();
            this.tables.Add(baseName);
            return new Writer(this.conn, this.transaction, baseName);
        }

        public void DeleteOldData(string tableName)
        {
            string sql = $"DELETE FROM pad.{tableName} WHERE remessa = {this.remessa};";
            //File.AppendAllText(@"C:\Users\Everton\Downloads\delete-test.sql", sql + "\n\r");
            NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn, this.transaction);
            cmd.CommandTimeout = 3600;
            cmd.ExecuteNonQuery();
        }
        public void UpdateRemessa(string tableName)
        {
            string sql = $"UPDATE pad.{tableName} SET remessa = {this.remessa} WHERE remessa = 0;";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn, this.transaction);
            cmd.CommandTimeout = 3600;
            cmd.ExecuteNonQuery();
        }

        public NpgsqlConnection GetConnection()
        {
            return this.conn;
        }

    }
}
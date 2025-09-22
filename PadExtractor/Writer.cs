using Npgsql;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PadExtractor.Writer
{
    /**
     * Representa uma tabela do banco de dados.
     */
    public class Writer
    {
        private readonly NpgsqlConnection conn;
        private readonly NpgsqlTransaction transaction;
        private readonly string tableName;

        public Writer(NpgsqlConnection conn, NpgsqlTransaction transaction, string tableName)
        {
            this.conn = conn;
            this.transaction = transaction;
            this.tableName = tableName;
        }

        public void Save(Dictionary<string,object> data)
        {
            string sql = this.InsertBuilder(data);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, this.conn, this.transaction);
            cmd.CommandTimeout = 3600;
            cmd.ExecuteNonQuery();
        }

        private string InsertBuilder(Dictionary<string, object> data)
        {
            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();
            foreach(var pair in data)
            {
                columns.Append($"{pair.Key}, ");
                if(pair.Value == null)
                {
                    values.Append("NULL, ");
                }
                else
                {
                    switch(pair.Value)
                    {
                        case string str:
                            values.Append($"'{str.Replace("'", "''")}', ");
                            break;
                        case int or long or short or byte:
                            values.Append($"{pair.Value}, ");
                            break;
                        case double or float or decimal:
                            values.Append($"{pair.Value}, ");
                            break;
                        default:
                            values.Append($"'{pair.Value.ToString().Replace("'", "''")}', ");
                            break;
                    }
                }
            }
            if(columns.Length > 0 )
            {
                columns.Length -= 2;
                values.Length -= 2;
            }

            return $"INSERT INTO pad.{this.tableName} ({columns}) VALUES ({values});";
        }
    }
}
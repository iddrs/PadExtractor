using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PadExtractor.Process;

public class RestosPagarProcessor
{
    private readonly NpgsqlConnection connection;
    private string remessa = "";
    private NpgsqlTransaction transaction;

    public RestosPagarProcessor(string connStr)
    {
        this.connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(connStr).ToString());
    }

    public void MontaRestosPagar(string remessa)
    {
        this.connection.Open();

        try
        {
            this.transaction = this.connection.BeginTransaction();
            DeleteOldData(remessa);

            int year = int.Parse(remessa.Substring(0, 4));
            int month = int.Parse(remessa.Substring(4, 2));
            int anoAnterior = year - 1;
            string anoAtual = remessa.Substring(0, 4);
            string dataInicial = $"{anoAtual}-01-01";
            string dataFinal = $"{anoAtual}-{remessa.Substring(4, 2)}-{DateTime.DaysInMonth(year, month)}";
            string dataInicialAnoAnterior = $"{anoAnterior}-01-01";
            string dataFinalAnoAnterior = $"{anoAnterior}-12-31";

            string sqlTemplate = File.ReadAllText(@"rp.sql");
            string sql = sqlTemplate.Replace(@"%remessa%", remessa)
                .Replace(@"%anoAtual%", anoAtual)
                .Replace(@"%dataInicial%", dataInicial)
                .Replace(@"%dataFinal%", dataFinal)
                .Replace(@"%dataInicialAnoAnterior%", dataInicialAnoAnterior)
                .Replace(@"%dataFinalAnoAnterior%", dataFinalAnoAnterior);
            //File.WriteAllText(@"C:\Users\Everton\Downloads\rp-test.sql", sql);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, this.connection, this.transaction);
            cmd.CommandTimeout = 3600;
            cmd.ExecuteNonQuery();
            this.transaction.Commit();
        }
        catch (Exception)
        {
            this.transaction.Rollback();
            throw;
        }

    }

    public void DeleteOldData(string remessa)
    {
        string sql = $"DELETE FROM pad.restos_pagar WHERE remessa = {remessa};";
        NpgsqlCommand cmd = new NpgsqlCommand(sql, this.connection, this.transaction);
        cmd.ExecuteNonQuery();
    }
}



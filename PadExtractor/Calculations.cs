
using PadExtractor.Transformer;

namespace PadExtractor.Calculation;
public class Calculations
{
    public static string DetectEntidade(Dictionary<string, object> row)
    {
        // Pega a entidade pelo campo orgao
        if (row.ContainsKey("orgao"))
        {
            switch (Convert.ToInt32(row["orgao"]))
            {
                case 1:
                    return "cm";
                case 12:
                case 50:
                    return "fpsm";
                default:
                    return "pm";
            }
        }

        // Pega a entidade pelo CNPJ
        if (row.ContainsKey("cnpj"))
        {
            if (row["cnpj"].ToString() == "12292535000162")
            {
                return "cm";
            }
        }

        // Pega a entidade pelo campo entidade_empenho
        if (row.ContainsKey("entidade_empenho"))
        {
            switch (Convert.ToInt32(row["entidade_empenho"]))
            {
                case 0:
                    return "pm";
                case 1:
                    return "fpsm";
                case 2:
                    return "cm";
            }
        }

        // Pega a entidade pelo campo fonte_recurso_suplementacao
        if (row.ContainsKey("fonte_recurso_suplementacao"))
        {
            switch (Convert.ToInt32(row["fonte_recurso_suplementacao"]))
            {
                case 800:
                case 801:
                case 802:
                case 803:
                case 804:
                    return "fpsm";
            }
        }

        if (row.ContainsKey("fonte_recurso_reducao"))
        {
            switch (Convert.ToInt32(row["fonte_recurso_reducao"]))
            {
                case 800:
                case 801:
                case 802:
                case 803:
                case 804:
                    return "fpsm";
                default:
                    return "pm";
            }
        }

        return null;
    }

    public static string Remessa(Dictionary<string, object> row)
    {
        return "0";
    }

    public static string NaturezaReceita(Dictionary<string, object> row)
    {
        string codigoReceita = row["codigo_receita"].ToString();
        switch (codigoReceita[0])
        {
            case '9':
                return (codigoReceita.Substring(1) + "0");
            case '7':
                return Transformer.Transformers.NroFmt("1" + codigoReceita.Substring(1));
            case '8':
                return Transformer.Transformers.NroFmt("2" + codigoReceita.Substring(1));
            default:
                return Transformer.Transformers.NroFmt(codigoReceita);
        }
    }

    public static string CategoriaReceita(Dictionary<string, object> row)
    {
        if (Convert.ToInt32(row["caracteristica_peculiar_receita"]) > 0)
        {
            return "dedutora";
        }

        string codigoReceita = row["codigo_receita"].ToString();
        switch (codigoReceita[0])
        {
            case '7':
            case '8':
                return "intra";
            default:
                return "normal";
        }
    }

    public static string TipoReceita(Dictionary<string, object> row)
    {
        return row["natureza_receita"].ToString().Substring(7, 1);
    }

    public static string AArrecadarAtualizado(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["previsao_atualizada"]) - Convert.ToDouble(row["receita_realizada"]), 2).ToString("F2");
    }

    public static string AArrecadarOrcado(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["receita_orcada"]) - Convert.ToDouble(row["receita_realizada"]), 2).ToString("F2");
    }

    public static string Realizada1Bim(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["realizada_jan"]) + Convert.ToDouble(row["realizada_fev"]), 2).ToString("F2");
    }

    public static string Realizada2Bim(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["realizada_mar"]) + Convert.ToDouble(row["realizada_abr"]), 2).ToString("F2");
    }

    public static string Realizada3Bim(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["realizada_mai"]) + Convert.ToDouble(row["realizada_jun"]), 2).ToString("F2");
    }

    public static string Realizada4Bim(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["realizada_jul"]) + Convert.ToDouble(row["realizada_ago"]), 2).ToString("F2");
    }

    public static string Realizada5Bim(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["realizada_set"]) + Convert.ToDouble(row["realizada_out"]), 2).ToString("F2");
    }

    public static string Realizada6Bim(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["realizada_nov"]) + Convert.ToDouble(row["realizada_dez"]), 2).ToString("F2");
    }

    public static string MetaJan(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_1bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaFev(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_1bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaMar(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_2bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaAbr(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_2bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaMai(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_3bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaJun(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_3bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaJul(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_4bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaAgo(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_4bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaSet(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_5bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaOut(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_5bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaNov(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_6bim"]) / 2, 2).ToString("F2");
    }

    public static string MetaDez(Dictionary<string, object> row)
    {
        return Math.Round(Convert.ToDouble(row["meta_6bim"]) / 2, 2).ToString("F2");
    }

    public static string DotacaoAtualizada(Dictionary<string, object> row)
    {
        return Math.Round(
            Convert.ToDouble(row["dotacao_inicial"]) +
            Convert.ToDouble(row["atualizacao_monetaria"]) +
            Convert.ToDouble(row["credito_suplementar"]) +
            Convert.ToDouble(row["credito_especial"]) +
            Convert.ToDouble(row["credito_extraordinario"]) -
            Convert.ToDouble(row["reducao_dotacao"]) +
            Convert.ToDouble(row["transferencia"]) +
            Convert.ToDouble(row["transposicao"]) +
            Convert.ToDouble(row["remanejamento"]), 2).ToString("F2");
    }

    public static string DotacaoDisponivel(Dictionary<string, object> row)
    {
        return Math.Round(
            Convert.ToDouble(row["dotacao_atualizada"]) -
            Convert.ToDouble(row["valor_limitado"]) +
            Convert.ToDouble(row["valor_recomposto"]), 2).ToString("F2");
    }

    public static string SaldoAEmpenhar(Dictionary<string, object> row)
    {
        return Math.Round(
            Convert.ToDouble(row["dotacao_atualizada"]) -
            Convert.ToDouble(row["valor_empenhado"]), 2).ToString("F2");
    }

    public static string SaldoDisponivel(Dictionary<string, object> row)
    {
        return Math.Round(
            Convert.ToDouble(row["dotacao_disponivel"]) -
            Convert.ToDouble(row["valor_empenhado"]), 2).ToString("F2");
    }

    public static string EmpenhadoALiquidar(Dictionary<string, object> row)
    {
        return Math.Round(
            Convert.ToDouble(row["valor_empenhado"]) -
            Convert.ToDouble(row["valor_liquidado"]), 2).ToString("F2");
    }

    public static string LiquidadoAPagar(Dictionary<string, object> row)
    {
        return Math.Round(
            Convert.ToDouble(row["valor_liquidado"]) -
            Convert.ToDouble(row["valor_pago"]), 2).ToString("F2");
    }

    public static string EmpenhadoAPagar(Dictionary<string, object> row)
    {
        return Math.Round(
            Convert.ToDouble(row["valor_empenhado"]) -
            Convert.ToDouble(row["valor_pago"]), 2).ToString("F2");
    }

    public static string SaldoInicial(Dictionary<string, object> row)
    {
        string contaContabil = row["conta_contabil"].ToString();
        switch (contaContabil[0])
        {
            case '1':
            case '3':
            case '5':
            case '7':
                return Math.Round(
                    Convert.ToDouble(row["saldo_inicial_devedor"]) -
                    Convert.ToDouble(row["saldo_inicial_credor"]), 2).ToString("F2");
            case '2':
            case '4':
            case '6':
            case '8':
                return Math.Round(
                    Convert.ToDouble(row["saldo_inicial_credor"]) -
                    Convert.ToDouble(row["saldo_inicial_devedor"]), 2).ToString("F2");
            default:
                return "0";
        }
    }

    public static string SaldoAtual(Dictionary<string, object> row)
    {
        string contaContabil = row["conta_contabil"].ToString();
        switch (contaContabil[0])
        {
            case '1':
            case '3':
            case '5':
            case '7':
                return Math.Round(
                    Convert.ToDouble(row["saldo_atual_devedor"]) -
                    Convert.ToDouble(row["saldo_atual_credor"]), 2).ToString("F2");
            case '2':
            case '4':
            case '6':
            case '8':
                return Math.Round(
                    Convert.ToDouble(row["saldo_atual_credor"]) -
                    Convert.ToDouble(row["saldo_atual_devedor"]), 2).ToString("F2");
            default:
                return "0";
        }
    }
}
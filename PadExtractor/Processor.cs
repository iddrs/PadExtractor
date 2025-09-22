using PadExtractor.Layout;
using PadExtractor.Source;
using PadExtractor.Writer;
using System.Data;
using System.Xml;

namespace PadExtractor.Process;

/**
 * Controlador geral do processamento.
 */
public class Processor(SourceRepository sourceRepository, LayoutRepository layoutRepository, WriterRepository writerRepository, IProgressMonitor progressMonitor)
{
    private readonly SourceRepository sourceRepository = sourceRepository ?? throw new ArgumentNullException(nameof(sourceRepository));

    private readonly WriterRepository writerRepository = writerRepository ?? throw new ArgumentNullException(nameof(writerRepository));

    private readonly LayoutRepository layoutRepository = layoutRepository ?? throw new ArgumentNullException(nameof(layoutRepository));

    private readonly IProgressMonitor progressMonitor = progressMonitor ?? throw new ArgumentNullException(nameof(progressMonitor));

    private string[] sourceFiles = [];

    public long totalSourceSize = 0;

    public long totalProcessedSize = 0;

    public void Process()
    {
        this.progressMonitor.UpdateProgress(0, "Processamento iniciado...");
        this.loadSourceFiles();
        this.calcTotalSourceSize();
        this.writerRepository.BeginTransaction();
        this.ProcessSources();
        this.writerRepository.Commit();

        this.progressMonitor.UpdateProgress(100, "Processamento terminado.");
    }

    private int calcPercentProcessed()
    {
        decimal processed = (decimal)this.totalProcessedSize;
        decimal total = (decimal)this.totalSourceSize;
        decimal percent = processed / total * 100;
        return (int)percent;
    }

    private void loadSourceFiles()
    {
        this.sourceFiles = this.sourceRepository.sourceDirs
                .Where(Directory.Exists)
                .SelectMany(dir => Directory.GetFiles(dir, "*.txt"))
                .ToArray();
    }

    private void calcTotalSourceSize()
    {
        foreach (string file in this.sourceFiles)
        {
            FileInfo finfo = new FileInfo(file);
            this.totalSourceSize += finfo.Length;
        }
    }

    private void ProcessSources()
    {
        foreach (string file in this.sourceFiles)
        {
            Source.Source source = this.sourceRepository.GetSourceFor(file);
            try
            {
                ParseSource(source);
            }
            catch (FileNotFoundException ex)
            {
                this.totalProcessedSize += source.fileSize;
            }
            this.progressMonitor.UpdateProgress(calcPercentProcessed(), $"{file} ({this.totalProcessedSize} / {this.totalSourceSize})");
        }
        this.totalProcessedSize = this.totalSourceSize;// Garante que o percentual de conclusão seja 100%
        this.progressMonitor.UpdateProgress(calcPercentProcessed(), "Processamento dos *.txt terminado. Salvando tudo no banco de dados...");
    }

    private void ParseSource(Source.Source source)
    {
        Layout.Layout layout = this.layoutRepository.GetLayoutFor(source);
        Writer.Writer writer = this.writerRepository.GetWriterFor(source);

        bool isFirstLine = true;
        Dictionary<string, string> header = new Dictionary<string, string>();
        while (true)
        {
            this.progressMonitor.UpdateProgress(calcPercentProcessed(), $"{source.filepath} ({this.totalProcessedSize} / {this.totalSourceSize})");
            string line = source.ReadNextLine();
            totalProcessedSize += line.Length;
            if (line.StartsWith("FINALIZADOR"))
            {
                return;
            }

            if (isFirstLine)
            {
                ParseSourceHeader(line, header);
                this.writerRepository.remessa = header["remessa"];
                isFirstLine = false;
            } else {
                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("cnpj", header["cnpj"]);
                data.Add("data_inicial", header["data_inicial"]);
                data.Add("data_final", header["data_final"]);
                data.Add("data_geracao", header["data_geracao"]);
                //data.Add("remessa", header["remessa"]);
                data.Add("remessa", "0");
                writer.Save(this.ParceSourceData(line, data, layout));
            }
        }
    }

    private void ParseSourceHeader(string data, Dictionary<string, string> header)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(data);
        header.Add("cnpj", data.Substring(0, 14));
        header.Add("data_inicial", $"{data.Substring(18, 4)}-{data.Substring(16, 2)}-{data.Substring(14, 2)}");
        header.Add("data_final", $"{data.Substring(26, 4)}-{data.Substring(24, 2)}-{data.Substring(22, 2)}");
        header.Add("data_geracao", $"{data.Substring(34, 4)}-{data.Substring(32, 2)}-{data.Substring(30, 2)}");
        header.Add("remessa", $"{data.Substring(26, 4)}{data.Substring(24, 2)}");
    }

    private Dictionary<string, object> ParceSourceData(string line, Dictionary<string, object> data, Layout.Layout layout)
    {
        XmlNodeList cols = layout.GetCollumns();

        foreach (XmlNode col in cols)
        {
            if (col.Attributes["id"].Value == "remessa")
            { continue; }
            else if (col.Attributes["origin"].Value == "source")
            {
                int start = Int32.Parse(col.Attributes["start"].Value) - 1;
                int len = Int32.Parse(col.Attributes["len"].Value);
                string val = line.Substring(start, len);
                if (col.Attributes["transformer"].Value != "")
                {
                    val = ApplyTransformer(col.Attributes["transformer"].Value, val);
                }
                data.Add(col.Attributes["id"].Value, val);
            }
            else if (col.Attributes["origin"].Value == "header")
            {
                continue;
            }
            else if (col.Attributes["origin"].Value == "calc")
            {
                string val = DoCalculations(col.Attributes["fn"].Value, data);
                data.Add(col.Attributes["id"].Value, val);
            }
            else
            {
                throw new InvalidConstraintException(col.Attributes["origin"].Value);
            }
        }
        return data;
    }

    private static string DoCalculations(string calcName, Dictionary<string, object> row)
    {
        if(calcName == "detect_entidade")
        {
            return Calculation.Calculations.DetectEntidade(row);
        }
        else if(calcName == "natureza_receita")
        {
            return Calculation.Calculations.NaturezaReceita(row);
        }
        else if(calcName == "remessa")
        {
            return Calculation.Calculations.Remessa(row);
        }
        else if(calcName == "categoria_receita")
        {
            return Calculation.Calculations.CategoriaReceita(row);
        }
        else if(calcName == "tipo_receita")
        {
            return Calculation.Calculations.TipoReceita(row);
        }
        else if(calcName == "a_arrecadar_atualizado")
        {
            return Calculation.Calculations.AArrecadarAtualizado(row);
        }
        else if(calcName == "a_arrecadar_orcado")
        {
            return Calculation.Calculations.AArrecadarOrcado(row);
        }
        else if(calcName == "realizada_1bim")
        {
            return Calculation.Calculations.Realizada1Bim(row);
        }
        else if(calcName == "realizada_2bim")
        {
            return Calculation.Calculations.Realizada2Bim(row);
        }
        else if(calcName == "realizada_3bim")
        {
            return Calculation.Calculations.Realizada3Bim(row);
        }
        else if(calcName == "realizada_4bim")
        {
            return Calculation.Calculations.Realizada4Bim(row);
        }
        else if(calcName == "realizada_5bim")
        {
            return Calculation.Calculations.Realizada5Bim(row);
        }
        else if(calcName == "realizada_6bim")
        {
            return Calculation.Calculations.Realizada6Bim(row);
        }
        else if(calcName == "meta_jan")
        {
            return Calculation.Calculations.MetaJan(row);
        }
        else if(calcName == "meta_fev")
        {
            return Calculation.Calculations.MetaFev(row);
        }
        else if(calcName == "meta_mar")
        {
            return Calculation.Calculations.MetaMar(row);
        }
        else if(calcName == "meta_abr")
        {
            return Calculation.Calculations.MetaAbr(row);
        }
        else if(calcName == "meta_mai")
        {
            return Calculation.Calculations.MetaMai(row);
        }
        else if(calcName == "meta_jun")
        {
            return Calculation.Calculations.MetaJun(row);
        }
        else if(calcName == "meta_jul")
        {
            return Calculation.Calculations.MetaJul(row);
        }
        else if(calcName == "meta_ago")
        {
            return Calculation.Calculations.MetaAgo(row);
        }
        else if(calcName == "meta_set")
        {
            return Calculation.Calculations.MetaSet(row);
        }
        else if(calcName == "meta_out")
        {
            return Calculation.Calculations.MetaOut(row);
        }
        else if(calcName == "meta_nov")
        {
            return Calculation.Calculations.MetaNov(row);
        }
        else if(calcName == "meta_dez")
        {
            return Calculation.Calculations.MetaDez(row);
        }
        else if(calcName == "dotacao_atualizada")
        {
            return Calculation.Calculations.DotacaoAtualizada(row);
        }
        else if(calcName == "dotacao_disponivel")
        {
            return Calculation.Calculations.DotacaoDisponivel(row);
        }
        else if(calcName == "saldo_a_empenhar")
        {
            return Calculation.Calculations.SaldoAEmpenhar(row);
        }
        else if(calcName == "saldo_disponivel")
        {
            return Calculation.Calculations.SaldoDisponivel(row);
        }
        else if(calcName == "empenhado_a_liquidar")
        {
            return Calculation.Calculations.EmpenhadoALiquidar(row);
        }
        else if(calcName == "liquidado_a_pagar")
        {
            return Calculation.Calculations.LiquidadoAPagar(row);
        }
        else if(calcName == "empenhado_a_pagar")
        {
            return Calculation.Calculations.EmpenhadoAPagar(row);
        }
        else if(calcName == "saldo_inicial")
        {
            return Calculation.Calculations.SaldoInicial(row);
        }
        else if(calcName == "saldo_atual")
        {
            return Calculation.Calculations.SaldoAtual(row);
        }
        else
        { throw new InvalidConstraintException(calcName); }
    }

    private static string ApplyTransformer(string transformerName, string val)
    {
        if(transformerName == "date_fmt")
        {
            return Transformer.Transformers.DateFmt(val);
        }
        else if(transformerName == "currency_fmt")
        {
            return Transformer.Transformers.CurrencyFmt(val);
        }
        else if(transformerName == "currency_post_signal_fmt")
        {
            return Transformer.Transformers.CurrencyPostSignalFmt(val);
        }
        else if(transformerName == "ndo_fmt")
        {
            return Transformer.Transformers.NdoFmt(val);
        }
        else if(transformerName == "elemento_fmt")
        {
            return Transformer.Transformers.ElementoFmt(val);
        }
        else if(transformerName == "nro_fmt")
        {
            return Transformer.Transformers.NroFmt(val);
        }
        else if(transformerName == "cc_fmt")
        {
            return Transformer.Transformers.CcFmt(val);
        }
        else if(transformerName == "trim")
        {
            return Transformer.Transformers.Trim(val);
        }
        else if(transformerName == "strtoupper")
        {
            return Transformer.Transformers.Strtoupper(val);
        }
        else
        {  throw new InvalidConstraintException(transformerName); }
    }
}
namespace PadExtractor.Transformer;

public class Transformers
{
    public static string ElementoFmt(string val)
    {
        val = ZerosFromLeftToRight(val);
        val = val.Substring(0, Math.Min(6, val.Length));
        return val;
    }

    public static string ZerosFromLeftToRight(string val)
    {
        if (string.IsNullOrEmpty(val) || val[0] != '0')
            return val;

        int originalLength = val.Length;
        int firstNonZeroPosition = 0;
        for (int i = 0; i < val.Length; i++)
        {
            if (val[i] != '0')
                break;
            firstNonZeroPosition++;
        }

        string nonZeros = val.Substring(firstNonZeroPosition);
        return nonZeros.PadRight(originalLength, '0');
    }

    public static string DateFmt(string val)
    {
        if (val == "00000000")
            return null;

        if (DateTime.TryParseExact(val, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out DateTime result))
        {
            return result.ToString("yyyy-MM-dd");
        }

        return null;
    }

    public static string CurrencyFmt(string val)
    {
        if (long.TryParse(val, out long intValue))
        {
            return $"{Math.Round(intValue / 100.0, 2)}";
        }
        return "0";
    }

    public static string CurrencyPostSignalFmt(string val)
    {
        if (string.IsNullOrEmpty(val) || val.Length < 14)
            return "0";

        string valor = val.Substring(0, 13);
        string sinal = val.Substring(val.Length - 1, 1);
        string combined = sinal + valor;

        if (long.TryParse(combined, out long intValue))
        {
            return $"{Math.Round(intValue / 100.0, 2)}";
        }

        return "0";
    }

    public static string NdoFmt(string val)
    {
        if (string.IsNullOrEmpty(val))
            return string.Empty;

        string processedVal = ZerosFromLeftToRight(val);

        return processedVal.Length > 15 ? processedVal.Substring(0, 15) : processedVal;
    }

    public static string NroFmt(string val)
    {
        if (string.IsNullOrEmpty(val))
            return string.Empty;

        string processedVal = ZerosFromLeftToRight(val);

        return processedVal.Length > 15 ? processedVal.Substring(0, 15) : processedVal;
    }

    public static string CcFmt(string val)
    {
        if (string.IsNullOrEmpty(val))
            return string.Empty;

        string processedVal = ZerosFromLeftToRight(val);

        return processedVal.Length > 15 ? processedVal.Substring(0, 15) : processedVal;
    }

    public static string Trim(string val)
    {
        return val.Trim();
    }
    public static string Strtoupper(string val)
    {
        return val.ToUpper();
    }
}

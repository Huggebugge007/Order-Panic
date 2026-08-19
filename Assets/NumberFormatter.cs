using UnityEngine;

public static class NumberFormatter
{
    private static readonly string[] suffixes =
    {
        "",     // 1
        "K",    // Thousand
        "M",    // Million
        "B",    // Billion
        "T",    // Trillion
        "Qa",   // Quadrillion
        "Qi",   // Quintillion
        "Sx",   // Sextillion
        "Sp",   // Septillion
        "Oc",   // Octillion
        "No",   // Nonillion
        "Dc"    // Decillion
    };

    public static string FormatNumber(double number)
    {
        if (number < 1000)
        {
            return Mathf.RoundToInt((float)number).ToString();
        }

        int suffixIndex = 0;

        while (number >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000;
            suffixIndex++;
        }

        return number.ToString("0.#") + suffixes[suffixIndex];
    }
}
namespace shared
{
    public static class Utils
    {
        /// <summary>
        /// Преобразует строку, содержащую число, в double. 
        /// [-]d[(d)][.][dd]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToDoubleSumma(string value)
        {
            double summa = 0.0;
            if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out double val))
            {
                int v = (int)((val * 100) + (val < 0 ? -0.5 : 0.5));
                summa = v;
                summa = summa / 100;
            }

            return summa;
        }

        /*
        public static decimal ToDecimalSumma(string value)
        {
            decimal summa = 0;
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out decimal val))
            {
                int v = (int)((val * 100) + (val < 0 ? -0.5 : 0.5));
                summa = v;
                summa = summa / 100;
            }

            return summa;
        }
        CURRENCY to_currency(unsigned int sum)
        {
            CURRENCY c;
            c.int64 = sum * 100;
            return c;
        }
        */
    }
}

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
    }
}

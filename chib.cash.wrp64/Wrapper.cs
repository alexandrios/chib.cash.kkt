using IniParser;
using System;
using shared;
using System.Reflection;
using System.Collections.Generic;

namespace chib.cash.wrp64
{
    public class Wrapper
    {
        Log log;
        String cashDriver;
        //const String INI_FILE = "chib.cash.wrp64.ini";

        List<String> textLines = new List<String>();
        String srvText;
        String section;
        String total;
        String typeCheque;
        String carryIn;

        Dictionary<String, String> inputParams = new Dictionary<String, String>();

        String positions;
        Boolean isPositions = false;
        List<String> posText = new List<String>();
        List<String> posPrice = new List<String>();
        List<String> posTax = new List<String>();

        public Wrapper() {}

        public void X_Report()
        {
            ReadIniFile();
            CashObject cashObject;
            try
            {
                cashObject = new CashObject(cashDriver);
                ExecuteMethod(cashObject, "XReport", null);
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "[" + this.cashDriver + "]: " + ex.Message);
            }
            finally
            {
                cashObject = null;
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
            }
        }

        public void Z_Report()
        {
            ReadIniFile();
            CashObject cashObject;
            try
            {
                cashObject = new CashObject(cashDriver);
                ExecuteMethod(cashObject, "ZReport", null);
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "[" + this.cashDriver + "]: " + ex.Message);
            }
            finally
            {
                cashObject = null;
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
            }
        }

        public void MoneyIn(String value)
        {
            ReadIniFile();
            CashObject cashObject;
            try
            {
                cashObject = new CashObject(cashDriver);
                ExecuteMethod(cashObject, "Income", value);
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "[" + this.cashDriver + "]: " + ex.Message);
            }
            finally
            {
                cashObject = null;
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
            }
        }

        public void MoneyOut(String value)
        {
            ReadIniFile();
            CashObject cashObject;
            try
            {
                cashObject = new CashObject(cashDriver);
                ExecuteMethod(cashObject, "Outcome", value);
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "[" + this.cashDriver + "]: " + ex.Message);
            }
            finally
            {
                cashObject = null;
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
            }
        }

        public void PrintDocument(String chequeTemplate, Dictionary<String, String> parameters)
        {
            this.inputParams = parameters;

            ReadIniFile();

            ParseChequeTemplate(chequeTemplate);

            CashExecute(this.cashDriver);
        }

        /// <summary>
        /// Чтение ini-файла.
        /// </summary>
        private void ReadIniFile()
        {
            FileIniDataParser parser = new FileIniDataParser();
            String iniFile = Assembly.GetExecutingAssembly().GetName().Name + ".ini";
            IniParser.Model.IniData configuration = parser.ReadFile(iniFile /*INI_FILE*/);

            String logPath = configuration.GetKey("LogPath");
            log = new Log(logPath);
            log.WriteToLog("");
            log.WriteToLog("");
            log.WriteToLog("### Чтение параметров из " + iniFile /*INI_FILE*/ + " ###");
            log.WriteToLog("LogPath=" + logPath);

            this.cashDriver = configuration.GetKey("CashDriver");
            if (String.IsNullOrEmpty(cashDriver))
                log.WriteToLog(Log.Level.Error, "Не указан cashDriver");
            else
                log.WriteToLog("cashDriver=" + cashDriver);
        }
        
        
        /// <summary>
        /// Разбор текстового шаблона чека.
        /// </summary>
        /// <param name="chequeTemplate"></param>
        private void ParseChequeTemplate(String chequeTemplate)
        {
            String[] lines = chequeTemplate.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (String line in lines)
            {
                if (line.Substring(0, 5) == "~hdr:")
                {
                    // Строки нефискальной печати (могут содержать в начале chr(4), в этом случае они не печатаются)
                    textLines.Add(line.Substring(5));
                }
                else if (line.Substring(0, 5) == "~doc ")
                {
                    // не используется?
                }
                else if (line.Substring(0, 5) == "~dep ")
                {
                    // Наименование перевода (используется, если не указаны позиции). Если не указано, то используется умолчание из драйверов. Обычно: "Приём перевода"
                    srvText = line.Substring(5);
                }
                else if (line.Substring(0, 5) == "~srv ")
                {
                    // Номер секции, Сумма оплаты по чеку
                    String[] tmp = line.Substring(5).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    section = tmp[0];
                    total = tmp[1];
                }
                else if (line.Substring(0, 5) == "~pay ")
                {
                    // Тип платежа (1-продажа, 0-возврат), Сумма принятая (Сумма принятая - Сумма оплаты по чеку = Сдача)
                    String[] tmp = line.Substring(5).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    typeCheque = tmp[0];
                    carryIn = tmp[1];
                }
                else if (line.Substring(0, 5) == "~pos:")
                {
                    // Позиции в чеке: "~pos: Оплата за воду=0.04|Плата за газ=0.10"
                    if (String.IsNullOrEmpty(positions))
                        positions = line.Substring(5).Trim();
                    else
                        positions += '|' + line.Substring(5).Trim();
                }
            }

            log.WriteToLog("### Чтение параметров из шаблона чека ###");
            log.WriteToLog("srvText=" + srvText);
            log.WriteToLog("section=" + section);
            log.WriteToLog("total=" + total);
            log.WriteToLog("typeCheque=" + typeCheque);
            log.WriteToLog("carryIn=" + carryIn);

            // Разбор позиций
            if (!String.IsNullOrEmpty(positions))
            {
                String[] _tmp = positions.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String line in _tmp)
                {
                    String[] _pos = line.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
                    if (_pos.Length != 3)
                    {
                        log.WriteToLog(Log.Level.Error, "Позиция " + line + " содержит недопустимое количество столбцов: " + _pos.Length.ToString());
                        break;
                    }
                    else
                    {
                        posText.Add(_pos[0]);
                        posPrice.Add(_pos[1]);
                        posTax.Add(_pos[2]);
                    }
                }

                if (posText.Count > 0 && posPrice.Count > 0 && posTax.Count > 0 && posText.Count == posPrice.Count && posText.Count == posTax.Count)
                    isPositions = true;
            }

            // Логирование позиций
            if (isPositions)
            {
                log.WriteToLog("Позиции:");
                for (int i = 0; i < posText.Count; i++)
                {
                    log.WriteToLog($"{posText[i], -25} {posPrice[i], 10} {posTax[i]}");
                }
            }
            else
            {
                log.WriteToLog("Позиций нет");
            }

            log.WriteToLog("Строки нефискальной печати:");
            foreach (String line in textLines)
            {
                log.WriteToLog(line + NonPrintableString(NonPrintableText(line)));
            }
        }


        /// <summary>
        /// Основной цикл работы с ККТ.
        /// </summary>
        /// <param name="cashDriver"></param>
        private void CashExecute(String cashDriver)
        {
            CashObject cashObject;
            long result = -1;

            try
            {
                cashObject = new CashObject(cashDriver);
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, ex.Message + " " + cashDriver);
                return;
            }

            result = ExecuteMethod(cashObject, "BeginDocument", inputParams);

            //result = ExecuteMethod(cashObject, "PrintKKTInfoToLog", null);
            //result = ExecuteMethod(cashObject, "SetInputParams", inputParams);
            //result = ExecuteMethod(cashObject, "IsSessionOver", null);
            //result = ExecuteMethod(cashObject, "IsSessionOpen", null);

            result = ExecuteMethod(cashObject, "NewCheque", GetTypeCheque(typeCheque));

            result = ExecuteMethod(cashObject, "MultiPrint", GetPrintableTextLines(textLines));

            
            // Регистрация продажи / возврата продажи
            Dictionary<String, String> regSaleParams = new Dictionary<String, String>();
            if (isPositions)
            {
                for (int i = 0; i < posText.Count - 1; i++)
                {
                    regSaleParams[Attributes.TEMPL_SRVTEXT] = posText[i];
                    regSaleParams[Attributes.TEMPL_PRICE] = posPrice[i];
                    regSaleParams[Attributes.TEMPL_QUANTITY] = "1";
                    regSaleParams[Attributes.TEMPL_SECTION] = "0";
                    regSaleParams[Attributes.TEMPL_TAX] = posTax[i];

                    result += ExecuteMethod(cashObject, "RegisterSale", regSaleParams);
                }
            }
            else
            {
                regSaleParams[Attributes.TEMPL_SRVTEXT] = srvText;
                regSaleParams[Attributes.TEMPL_PRICE] = total;
                regSaleParams[Attributes.TEMPL_QUANTITY] = "1";
                regSaleParams[Attributes.TEMPL_SECTION] = "0";
                regSaleParams[Attributes.TEMPL_TAX] = "0";

                result = ExecuteMethod(cashObject, "RegisterSale", regSaleParams);
            }

            result = ExecuteMethod(cashObject, "CloseCheque", String.IsNullOrWhiteSpace(carryIn) ? "0" : carryIn);

            result = ExecuteMethod(cashObject, "EndDocument", null);

        }


        /// <summary>
        /// Выполнение метода объекта cashObject.
        /// </summary>
        /// <param name="cashObject"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private long ExecuteMethod(CashObject cashObject, String methodName, object args)
        {
            long result = -1;
            try
            {
                result = cashObject.ExecuteMethod(methodName, args);
                log.WriteToLog("Метод " + methodName + " вернул значение: " + result.ToString());
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "Метод " + methodName + ": " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Возвращает список строк для нефискальной печати, которые не содержат спец. символ.
        /// (Спец. символ char(4) - добавляется в начало строки в шаблоне для тех строк, которые 
		/// не должны быть напечатаны, в случае, если в шаблоне имеется корректная строка позиций ~pos).
        /// </summary>
        /// <param name="textLines"></param>
        /// <returns></returns>
        private List<String> GetPrintableTextLines(List<String> textLines)
        {
            List<String> printableTextLines = new List<string>();

            if (!isPositions)
            {
                printableTextLines.AddRange(textLines);
            }
            else
            {
                foreach (String line in textLines)
                {
                    if (!NonPrintableText(line))
                        printableTextLines.Add(line);
                }
            }

            return printableTextLines;
        }

        private bool NonPrintableText(String text)
        {
            return (int)text[0] == 4;
        }

        private String NonPrintableString(bool b)
        {
            return b ? " [Строка не для печати]" : "";
        }

        private ChequeType GetTypeCheque(String typeCheque)
        {
            return  typeCheque != "1" ? ChequeType.SELL_RETURN : ChequeType.SELL;
        }
    }
}

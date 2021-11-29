using IniParser;
using System;
using shared;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace chib.cash.wrp64
{
    public class Wrapper
    {
        Log log;
        String cashDriver;

        List<String> textLines = new List<String>();
        String srvText;
        String section;
        String total;
        String typeCheque;
        String carryIn;

        String positions;
        Boolean isPositions = false;
        List<String> posText = new List<String>();
        List<String> posPrice = new List<String>();
        List<String> posTax = new List<String>();

        public Wrapper()
        {
        }

        public void Start(String chequeTemplate)
        {
            ReadIniFile();
            ParseChequeTemplate(chequeTemplate);

            CashExecute(this.cashDriver);
        }

        private void ParseChequeTemplate(String chequeTemplate)
        {
            String[] lines = chequeTemplate.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (String line in lines)
            {
                //log.WriteToLog(line);
                if (line.Substring(0, 5) == "~hdr:")
                {
                    //log.WriteToLog(line);
                    textLines.Add(line.Substring(5));
                }
                else if (line.Substring(0, 5) == "~doc ")
                {
                    // не используется
                }
                else if (line.Substring(0, 5) == "~dep ")
                {
                    srvText = line.Substring(5);
                }
                else if (line.Substring(0, 5) == "~srv ")
                {
                    String[] tmp = line.Substring(5).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    section = tmp[0];
                    total = tmp[1];
                }
                else if (line.Substring(0, 5) == "~pay ")
                {
                    String[] tmp = line.Substring(5).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    typeCheque = tmp[0];
                    carryIn = tmp[1];
                }
                else if (line.Substring(0, 5) == "~pos:")
                {
                    if (String.IsNullOrEmpty(positions))
                        positions = line.Substring(5).Trim();
                    else
                        positions += '|' + line.Substring(5).Trim();
                }
            }

            //foreach (String line in textLines)
            //{
            //    log.WriteToLog(line);
            //}

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

        }


        private void ReadIniFile()
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniParser.Model.IniData configuration = parser.ReadFile("chib.cash.wrp64.ini");

            String logDirectory = configuration.GetKey("LogDirectory");
            Console.WriteLine("logDirectory = " + logDirectory);

            log = new Log(logDirectory);
            log.WriteToLog("logDirectory = " + logDirectory);

            this.cashDriver = configuration.GetKey("CashDriver");
            Console.WriteLine("cashDriver = " + cashDriver);
            if (String.IsNullOrEmpty(cashDriver))
                log.WriteToLog(Log.Level.Error, "Не указан cashDriver");
            else
                log.WriteToLog("cashDriver = " + cashDriver);
        }


        private void CashExecute(String cashDriver)
        {
            String driverPath = Directory.GetCurrentDirectory() + @"\" + cashDriver + ".dll";

            try
            {
                Assembly assembly = Assembly.LoadFile(driverPath);
                //foreach (var t in assembly.GetTypes())
                //{
                //    Console.WriteLine(t.FullName + " " + t.BaseType);
                //}

                Type cashType = assembly.GetType(cashDriver + ".Cash", true, true);
                //ICash cash = (ICash)Activator.CreateInstance(cashType);
                object cash = Activator.CreateInstance(cashType);




                MethodInfo executeMethod = cashType.GetMethod("Printer");
                //object[] parametersArray = new object[] {"aaaaaaaaaaaa", "bbbbbbbbbbbbb"};
                object[] parametersArray = new object[] { textLines };
                //object[] parametersArray = textLines.ToArray();

                executeMethod.Invoke(cash, parametersArray);

                //MethodInfo executeMethod = cashType.GetMethod("IsSessionOpen");
                //object[] parametersArray = new object[] {};
                //executeMethod.Invoke(cash, parametersArray);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + driverPath);
                log.WriteToLog(Log.Level.Error, ex.Message + " " + driverPath);
            }
        }
    }
}

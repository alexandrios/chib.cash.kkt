using System;
using System.Collections.Generic;
using shared;
using Atol.Drivers10.Fptr;
using System.Windows.Forms;

namespace chib.cash.atol64
{
    public class Cash : ICash
    {
        Config config;
        KKT kkt;
        Log log;
        InputParams ip;

        /*
        public Cash()
        {
            config = new Config("chib.cash.atol64.ini");
            log = config.GetLog();
            log.WriteToLog("Cash()");

            ip = new InputParams(log);

            kkt = new KKT(log);
            kkt.Initialize();
            if (!kkt.IsOpened())
            {
                kkt.Open(config.PortName, config.IpAddress, config.TCPPort);
            }
        }
        */

        private static Cash instance;

        protected Cash()
        {
            String iniFile = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".ini";
            config = new Config(iniFile /*"chib.cash.atol64.ini"*/);
            log = config.GetLog();
            log.WriteToLog("Cash()");

            ip = new InputParams(log);

            kkt = new KKT(log);
            kkt.Initialize();
        }

        public static Cash Instance()
        {
            if (instance == null)
                instance = new Cash();
            return instance;
        }

        ~Cash()
        {
            log.WriteToLog("~Cash()");
            kkt.Close();
            kkt.Deinitialize();
            End();
        }

        private List<String> SplitLinesByMaxSize(List<String> textLines)
        {
            List<String> result = new List<string>();
            String t = "";

            foreach (String line in textLines)
            {
                t = line;
                while (t.Length > config.MaxCashStringLength)
                {
                    result.Add(t.Substring(0, config.MaxCashStringLength));
                    t = t.Substring(config.MaxCashStringLength).TrimStart();
                }
                result.Add(t);
            }

            return result;
        }

        private void Open()
        {
            if (!kkt.IsOpened())
            {
                kkt.Open(config.PortName, config.IpAddress, config.TCPPort);
            }
        }

        private void Close()
        {
            kkt.Close();
        }

        public long BeginDocument(Dictionary<String, String> inputParams)
        {
            long result = -1;

            Open();
            SetInputParams(inputParams);
            PrintKKTInfoToLog();
            
            IsSessionOver();
            int shiftState = (int)IsSessionOpen();

            switch (shiftState)
            {
                case Constants.LIBFPTR_SS_OPENED:
                    log.WriteToLog("Смена открыта");
                    result = 0;
                    break;
                case Constants.LIBFPTR_SS_CLOSED:
                    log.WriteToLog("Смена закрыта");
                    break;
                case Constants.LIBFPTR_SS_EXPIRED:
                    log.WriteToLog("Смена истекла");
                    break;
            }

            return result;
        }

        public long EndDocument()
        {
            Close();
            return 0;
        }

        public long SetInputParams(Dictionary<String, String> inputParams)
        {
            ip.SetInputParams(inputParams);
            return inputParams.Count;
        }

        public long PrintKKTInfoToLog()
        {
            if (config.RegInfoLog == "Y" || config.RegInfoLog == "D")
            {
                // Запрос общей информации и статуса ККТ
                kkt.RequestInfoKKT();

                if (config.RegInfoLog == "D")
                {
                    // Запрос реквизитов регистрации ККТ
                    kkt.RequestRegInfo();
                }
            }
            return 0;
        }

        public long IsSessionOver()
        {
            // Проверить состояние смены
            uint shiftState = kkt.GetShiftState();
            if (shiftState == Constants.LIBFPTR_SS_EXPIRED)
            {
                // LIBFPTR_SS_EXPIRED (2) - смена истекла (продолжительность смены больше 24 часов)
                bool toTryZ = false;
                log.WriteToLog("Необходимо выполнить Z-отчет (закрыть смену)");
                if (config.AutoShift == "Y")
                {
                    log.WriteToLog("[AutoShift = YES] - попытка выполнить Z-отчет (закрыть смену)");
                    toTryZ = true;
                    kkt.ZReport();
                }
                else
                {
                    if (MessageBox.Show("Выполнить Z-отчет (закрыть смену)?", "Смена превысила 24 часа", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        log.WriteToLog("[AutoShift = N] - Request - Да - попытка выполнить Z-отчет (закрыть смену)");
                        toTryZ = true;
                        kkt.ZReport();
                    }
                }

                if (toTryZ)
                {
                    // Проверить состояние смены
                    shiftState = kkt.GetShiftState();
                    if (shiftState == Constants.LIBFPTR_SS_CLOSED)
                    {
                        log.WriteToLog("=> Смена закрыта");
                    }
                    else if (shiftState == Constants.LIBFPTR_SS_OPENED)
                    {
                        log.WriteToLog("=> Смена всё ещё открыта");
                    }
                }
            }

            return shiftState;
        }

        public long IsSessionOpen()
        {
            // Проверить состояние смены
            uint shiftState = kkt.GetShiftState();
            if (shiftState == Constants.LIBFPTR_SS_CLOSED)
            {
                // LIBFPTR_SS_CLOSED(0) - смена закрыта
                bool toOpenShift = false;
                if (config.AutoShift == "Y")
                {
                    log.WriteToLog("[AutoShift = Y] - попытка открыть смену");
                    toOpenShift = true;
                }
                else
                {
                    if (MessageBox.Show("Открыть смену?", "Смена закрыта",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        log.WriteToLog("[AutoShift = N] - Request - Да - попытка окрыть смену");
                        toOpenShift = true;
                    }
                    else
                    {
                        log.WriteToLog("[AutoShift = N] - Request - Нет - отказ открыть смену");
                    }
                }

                if (toOpenShift)
                {
                    // Регистрация кассира
                    kkt.CashierRegistration(ip.CashierName, ip.CashierInn);

                    // Открыть смену
                    kkt.OpenShift();

                    // Проверить состояние смены
                    shiftState = kkt.GetShiftState();
                }
            }

            return shiftState;
        }

        public long NewCheque(ChequeType chequeType)
        {
            long result = 0;

            if (ip.IsPreCheque)
            {
                log.WriteToLog("NewCheque: пречек");
                result = kkt.BeginNonFiscalDoc();
                return result;
            }

            log.WriteToLog("NewCheque");

            // Регистрация кассира
            kkt.CashierRegistration(ip.CashierName, ip.CashierInn);

            // Установка значения тегов
            uint tag1057 = (uint)config.Tag1057;
            if (tag1057 > 0)
            {
                log.WriteToLog("Признак агента [1057] взят из ini-файла: " + tag1057.ToString());
            }
            else
            {
                tag1057 = kkt.GetRegInfoParamUInt(1057);
                log.WriteToLog("Признак агента [1057] взят из регистрационных данных: " + tag1057.ToString());
            }
            log.WriteToLog("Установка значения параметра: Признак агента [1057]");
            kkt.SetParam(1057, tag1057);

            if (tag1057 > 0)
            {
                if (!String.IsNullOrEmpty(ip.OperatorName))
                {
                    log.WriteToLog("Установка значения параметра: Наименование оператора перевода [1026]");
                    kkt.SetParam(1026, ip.OperatorName);
                }

                if (!String.IsNullOrEmpty(ip.OperatorInn))
                {
                    log.WriteToLog("Установка значения параметра: ИНН оператора перевода [1016]");
                    kkt.SetParam(1016, ip.OperatorInn);
                }

                if (!String.IsNullOrEmpty(ip.OperatorAddress))
                {
                    log.WriteToLog("Установка значения параметра: Адрес оператора перевода [1005]");
                    kkt.SetParam(1005, ip.OperatorAddress);
                }

                if (!String.IsNullOrEmpty(ip.OperatorPhone))
                {
                    log.WriteToLog("Установка значения параметра: Телефон оператора перевода [1075]");
                    kkt.SetParam(1075, ip.OperatorPhone);
                }

                if (!String.IsNullOrEmpty(ip.AgentPhone))
                {
                    log.WriteToLog("Установка значения параметра: Телефон платежного агента [1073]");
                    kkt.SetParam(1073, ip.AgentPhone);
                }

                /*
                // 1074 -  у нас нет оператора по приёму платежей (а значит и его телефона) [05.10.2021]
                if (!String.IsNullOrEmpty(ip.PayAgentPhone))
                {
                    log.WriteToLog("Установка значения параметра: Телефон платежного агента [1074]");
                    kkt.SetParam(1074, ip.PayAgentPhone);
                }
                */

                if (!String.IsNullOrEmpty(ip.OperatorOper))
                {
                    log.WriteToLog("Установка значения параметра: Операция платежного агента [1044]");
                    kkt.SetParam(1044, ip.OperatorOper);
                }

                if (!String.IsNullOrEmpty(ip.PuPhone))
                {
                    log.WriteToLog("Установка значения параметра: Телефон поставщика [1171]");
                    kkt.SetParam(1171, ip.PuPhone);  // max 19 symbols
                }
            }

            // Установка типа чека
            uint typeCheque = 0;
            if (chequeType == ChequeType.SELL)
                typeCheque = 1;
            else if (chequeType == ChequeType.SELL_RETURN)
                typeCheque = 2;
            kkt.SetParam(Constants.LIBFPTR_PARAM_RECEIPT_TYPE, typeCheque);

            if (ip.IsECheque)
            {
                // Установка признака "электронный чек" (чтобы чек не печатался)
                log.WriteToLog("Установка значения параметра: Электронный чек [65572]");
                kkt.SetParam(Constants.LIBFPTR_PARAM_RECEIPT_ELECTRONICALLY, true);

                // для электронного чека обязательна передача почты или абонентского номера
                if (!String.IsNullOrEmpty(ip.ClientPhone))
                {
                    log.WriteToLog("Установка значения параметра: Телефон клиента [1008]");
                    kkt.SetParam(1008, ip.ClientPhone);
                }
                else if (!String.IsNullOrEmpty(ip.ClientEmail))
                {
                    log.WriteToLog("Установка значения параметра: Email клиента [1008]");
                    kkt.SetParam(1008, ip.ClientEmail);
                }
            }

            // Открытие чека
            result = kkt.OpenCheque();

            return result;
        }

        public long OpenSession()
        {
            return kkt.OpenShift();
        }

        public long Print(String text)
        {
            return kkt.Print(text);
        }

        public long MultiPrint(List<String> textLines)
        {
            // Разбить строки по максимальной ширине строки (config.MaxCashStringLength)
            log.WriteToLog("MultiPrint: получено строк: " + textLines.Count.ToString());
            List<String> splittedLines = SplitLinesByMaxSize(textLines);
            log.WriteToLog("Ширина строки(config.MaxCashStringLength): " + config.MaxCashStringLength.ToString() + 
                "; Итого строк: " + splittedLines.Count.ToString());

            long cnt = 0;
            foreach (String line in splittedLines)
            {
                kkt.Print(line);
                cnt++;
            }
            return cnt;
        }

        public long RegisterSale(Dictionary<String, String> regSaleParams)
        {
            long result = 0;

            String srvText = "Приём перевода";
            double sum = 0;
            double quantity = 1;
            uint section = 0;
            uint tax = 6;

            if (ip.IsPreCheque)
            {
                log.WriteToLog("RegisterSale: пречек");
                return result;
            }

            log.WriteToLog("RegisterSale: прием параметров:");
            foreach (KeyValuePair<String, String> pair in regSaleParams)
            {
                log.WriteToLog("Получен параметр: " + pair.Key + "=" + pair.Value);
                switch (pair.Key)
                {
                    case Attributes.TEMPL_SRVTEXT:
                        {
                            // Наименование позиции (услуги). Приоритет такой: 1. Параметр функции 2. Внешний параметр (из АРМ) 3. По-умолчанию
                            String value = pair.Value;
                            if (!String.IsNullOrEmpty(value))
                            {
                                srvText = value;
                            }
                            else if (!String.IsNullOrEmpty(ip.ServiceName))
                            {
                                srvText = ip.ServiceName;
                            }
                            log.WriteToLog("srvText=" + srvText);
                            break;
                        }
                    case Attributes.TEMPL_PRICE:
                        {
                            // Цена позиции
                            sum = Utils.ToDoubleSumma(pair.Value);
                            /*
                            if (uint.TryParse(pair.Value, out uint val))
                            {
                                uint v = (uint)((val * 100) + 0.5);
                                sum = v;
                            }
                            sum = sum / 100;
                            */
                            log.WriteToLog("sum=" + sum.ToString());
                            break;
                        }
                    case Attributes.TEMPL_QUANTITY:
                        {
                            // Количество позиции
                            if (uint.TryParse(pair.Value, out uint val))
                            {
                                quantity = val;
                            }
                            log.WriteToLog("quantity=" + quantity.ToString());
                            break;
                        }
                    case Attributes.TEMPL_SECTION:
                        {
                            // Номер секции (отдела)
                            String value = pair.Value;
                            if (uint.TryParse(value, out uint val))
                                section = val;
                            log.WriteToLog("section=" + section.ToString());
                            break;
                        }
                    case Attributes.TEMPL_TAX:
                        {
                            // Код НДС
                            // 1 LIBFPTR_TAX_VAT18  - НДС 18% (НДС 20%)
                            // 2 LIBFPTR_TAX_VAT10  - НДС 10%
                            // 3 LIBFPTR_TAX_VAT118 - НДС расчитанный 18/118 (НДС расчитанный 20/120)
                            // 4 LIBFPTR_TAX_VAT110 - НДС расчитанный 10/110
                            // 5 LIBFPTR_TAX_VAT0   - НДС 0%
                            // 6 LIBFPTR_TAX_NO     - НЕ ОБЛАГАЕТСЯ
                            String taxTakenFrom = "";
                            uint taxTypeNumber_ = 0;
                            String value = pair.Value;
                            if (uint.TryParse(value, out uint val))
                            {
                                if (val >= 1102 && val <= 1107)
                                {
                                    if (val == 1102) taxTypeNumber_ = 7;      // 20%
                                    else if (val == 1103) taxTypeNumber_ = 2; // 10%
                                    else if (val == 1106) taxTypeNumber_ = 8; // 20/120
                                    else if (val == 1107) taxTypeNumber_ = 4; // 10/110
                                    else if (val == 1104) taxTypeNumber_ = 5; // 0%
                                    else if (val == 1105) taxTypeNumber_ = 6; // НЕ ОБЛАГАЕТСЯ
                                    taxTakenFrom = " [Налог взят из позиции (предмета расчета)]";
                                }
                            }
                            if (taxTypeNumber_ == 0)
                            {
                                // Если бы код налога передавали из АРМ:
                                //if (ip.TaxNumber > 0)
                                //{
                                //    taxTypeNumber_ = ip.TaxNumber;
                                //    taxTakenFrom = "[Налог взят из внешнего параметра (АРМ)] ";
                                //}
                                //else
                                //{
                                    taxTypeNumber_ = config.TaxTypeNumber;
                                    taxTakenFrom = " [Налог взят из ini-файла]";
                                //}
                            }
                            tax = taxTypeNumber_;
                            log.WriteToLog("tax=" + tax.ToString() + taxTakenFrom);
                            break;
                        }
                }
            }

            // Запись тегов
            log.WriteToLog("Tag 1223 ->");
            if (!String.IsNullOrEmpty(ip.OperatorName))
            {
                log.WriteToLog("Установка значения параметра: Наименование оператора перевода [1026]");
                kkt.SetParam(1026, ip.OperatorName);
            }

            if (!String.IsNullOrEmpty(ip.OperatorInn))
            {
                log.WriteToLog("Установка значения параметра: ИНН оператора перевода [1016]");
                kkt.SetParam(1016, ip.OperatorInn);
            }

            if (!String.IsNullOrEmpty(ip.OperatorAddress))
            {
                log.WriteToLog("Установка значения параметра: Адрес оператора перевода [1005]");
                kkt.SetParam(1005, ip.OperatorAddress);
            }

            if (!String.IsNullOrEmpty(ip.OperatorPhone))
            {
                log.WriteToLog("Установка значения параметра: Телефон оператора перевода [1075]");
                kkt.SetParam(1075, ip.OperatorPhone);
            }

            if (!String.IsNullOrEmpty(ip.AgentPhone))
            {
                log.WriteToLog("Установка значения параметра: Телефон платежного агента [1073]");
                kkt.SetParam(1073, ip.AgentPhone);
            }

            /*
            // 1074 -  у нас нет оператора по приёму платежей (а значит и его телефона) [05.10.2021]
            if (!String.IsNullOrEmpty(ip.PayAgentPhone))
            {
                log.WriteToLog("Установка значения параметра: Телефон платежного агента [1074]");
                kkt.SetParam(1074, ip.PayAgentPhone);
            }
            */

            if (!String.IsNullOrEmpty(ip.OperatorOper))
            {
                log.WriteToLog("Установка значения параметра: Операция платежного агента [1044]");
                kkt.SetParam(1044, ip.OperatorOper);
            }

            kkt.UtilFormTlv();
            byte[] agentInfo = kkt.GetParamByteArray(Constants.LIBFPTR_PARAM_TAG_VALUE);
            log.WriteToLog("<- Tag 1223");


            log.WriteToLog("Tag 1224 ->");
            if (!String.IsNullOrEmpty(ip.PuPhone))
            {
                log.WriteToLog("Установка значения параметра: Телефон поставщика [1171]");
                kkt.SetParam(1171, ip.PuPhone);  // max 19 symbols
            }

            if (!String.IsNullOrEmpty(ip.PuName))
            {
                log.WriteToLog("Установка значения параметра: Наименование поставщика [1225]");
                kkt.SetParam(1225, ip.PuName); 
            }

            kkt.UtilFormTlv();
            byte[] suplierInfo = kkt.GetParamByteArray(Constants.LIBFPTR_PARAM_TAG_VALUE);
            log.WriteToLog("<- Tag 1224");


            kkt.SetParam(Constants.LIBFPTR_PARAM_COMMODITY_NAME, srvText);
            kkt.SetParam(Constants.LIBFPTR_PARAM_PRICE, sum);
            kkt.SetParam(Constants.LIBFPTR_PARAM_QUANTITY, quantity);
            kkt.SetParam(Constants.LIBFPTR_PARAM_TAX_TYPE, tax);

            kkt.SetParam(1212, 4); // Признак предмета расчета. 1-ТОВАР; 4-УСЛУГА (Таблица 29)
            kkt.SetParam(1214, 4); // Признак способа расчета. 4-ПОЛНЫЙ РАСЧЕТ (Таблица 28)


            // Признак агента (1222 и 1057)
            uint tag1222 = (uint)config.Tag1057;
            if (tag1222 > 0)
            {
                log.WriteToLog("Признак агента [1222] взят из ini-файла: " + tag1222.ToString());
                // Внимание: если признак агента из ини-файла будет отличаться от того, который в регистрации, то возникнет ошибка: 
                // Объекту 0x009489B0 присвоен код ошибки 148 [Ошибка программирования реквизита 1057 (Недопустимое сочетание реквизитов)]
            }
            else
            {
                tag1222 = kkt.GetRegInfoParamUInt(1057);
                log.WriteToLog("Признак агента [1222] взят из регистрационных данных: " + tag1222.ToString());
            }
            log.WriteToLog("Установка значения параметра: Признак агента [1222]");
            kkt.SetParam(1222, tag1222);

            log.WriteToLog("Установка значения параметра: Данные агента [1223]");
            kkt.SetParam(1223, agentInfo);

            if (!String.IsNullOrEmpty(ip.PuInn))
            {
                log.WriteToLog("Установка значения параметра: ИНН поставщика [1226]");
                kkt.SetParam(1226, ip.PuInn);
            }

            log.WriteToLog("Установка значения параметра: Данные поставщика [1224]");
            kkt.SetParam(1224, suplierInfo);

            result = kkt.Registration();

            return result;
        }

        public long CloseCheque(String income) // TODO?:  , PaymentType type = PaymentType.CASH)
        {
            long result = 0;

            if (ip.IsPreCheque)
            {
                log.WriteToLog("CloseCheque: пречек");
                result = kkt.EndNonFiscalDoc();
                return result;
            }

            //uint v = (uint)((income * 100) + 0.5);
            //double sum = v;
            //sum = sum / 100;
            double sum = Utils.ToDoubleSumma(income);

            long typeOpl = Constants.LIBFPTR_PT_CASH; // 0 - наличные
            String typeTakenFrom = "";
            uint chequeState = kkt.GetChequeState();

            //try
            //{ 

            // Регистрация оплаты
            if ((ChequeType)chequeState == ChequeType.SELL && !ip.IsCashlessCheque)
            {
                // Открыт чек продажи за наличные
                // Со сдачей
                // Возможно только при наличной оплате. При безналичной - идем в другую ветку, при чеке возврата - тоже
                kkt.SetParam(Constants.LIBFPTR_PARAM_PAYMENT_TYPE, Constants.LIBFPTR_PT_CASH);
                kkt.SetParam(Constants.LIBFPTR_PARAM_PAYMENT_SUM, sum);
                result = kkt.Payment();
                log.WriteToLog("CloseCheque: [Тип оплаты по-умолчанию: НАЛ] Сумма полученная=" + sum.ToString() +
                    " => " + result.ToString());
            }
            else
            {
                // Без сдачи - безнал или чек возврата
                // Если безнал, то закрываем безналичный чек определенным типом оплаты
                if (ip.IsCashlessCheque)
                {
                    // Если бы тип оплаты передавали из АРМ:
                    //if (ip.TypeOpl > 0)
                    //{
                    //    typeOpl = ip.TypeOpl;
                    //    typeTakenFrom = "[Тип оплаты взят из внешнего параметра (АРМ)]";
                    //}
                    //else
                    //{
                    typeOpl = config.CashlessTypeClose;
                    typeTakenFrom = "[Тип оплаты взят из ini-файла]";
                    //}
                }

                kkt.SetParam(Constants.LIBFPTR_PARAM_PAYMENT_TYPE, typeOpl);
                kkt.SetParam(Constants.LIBFPTR_PARAM_PAYMENT_SUM, sum);
                result = kkt.Payment();
                log.WriteToLog("CloseCheque: " + typeTakenFrom + "=" + typeOpl.ToString() + " Сумма полученная=" + sum.ToString() +
                    " => " + result.ToString());
            }

            //}
            //catch (Exception ex)
            //{
            //  kkt.CancelCheque();
            //  log.WriteToLog("Чек был отменен: " + ex.Message);
            //}


            // Закрытие чека
            result = kkt.CloseCheque();

            // Получение информации о номере чека из ФН
            uint documentNumber = kkt.GetLastDocumentNumber();
            log.WriteToLog(">>> ФД № = " + documentNumber.ToString());

            return result;
        }

        public long CancelCheque()
        {
            return kkt.CancelCheque();
        }

        public long XReport()
        {
            long result = -1;
            
            this.Open();
            try
            {
                result = kkt.XReport();
            }
            finally
            {
                this.Close();
            }
            return result;
        }

        public long ZReport()
        {
            long result = -1;

            this.Open();
            try
            {
                result = kkt.ZReport();
            }
            finally
            {
                this.Close();
            }
            return result;
        }

        public long Income(string sum)
        {
            long result = -1;

            if (String.IsNullOrEmpty(sum))
            {
                log.WriteToLog("Income(): не указана сумма");
                return -1;
            }

            this.Open();
            try
            {
                // Сумма выручки
                double revenue = kkt.GetRevenue();
                log.WriteToLog("Сумма выручки: " + revenue.ToString());

                // Сумма внесений
                double sumCashIn = kkt.GetCashIn();
                log.WriteToLog("Сумма внесений: " + sumCashIn.ToString());

                // Сумма выплат
                double sumCashOut = kkt.GetCashOut();
                log.WriteToLog("Сумма выплат: " + sumCashOut.ToString());

                result = kkt.Income(Utils.ToDoubleSumma(sum));
            }
            finally
            {
                this.Close(); 
            }
            return result;
        }

        public long Outcome(string sum)
        {
            long result = -1;

            if (String.IsNullOrEmpty(sum))
            {
                log.WriteToLog("Outcome(): не указана сумма");
                return -1;
            }

            this.Open();
            try
            {
                // Сумма выручки
                double revenue = kkt.GetRevenue();
                log.WriteToLog("Сумма выручки: " + revenue.ToString());

                // Сумма внесений
                double sumCashIn = kkt.GetCashIn();
                log.WriteToLog("Сумма внесений: " + sumCashIn.ToString());

                // Сумма выплат
                double sumCashOut = kkt.GetCashOut();
                log.WriteToLog("Сумма выплат: " + sumCashOut.ToString());

                result = kkt.Outcome(Utils.ToDoubleSumma(sum));

                if (result == -1)
                {
                    if (revenue + sumCashIn - sumCashOut <= sumCashOut)
                    {
                        log.WriteToLog(Log.Level.Warning, "Указанной суммы нет в кассе!");
                        MessageBox.Show("Указанной суммы нет в кассе!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            finally
            {
                this.Close();
            }
            return result;
        }

        public string GetInfoKKT()
        {
            throw new NotImplementedException();
        }

        public void SetCashierInn(String cashierInn)
        {
            throw new NotImplementedException();
        }

        public void SetIsPreCheck(bool isPreCheck)
        {
            throw new NotImplementedException();
        }

        public void SetTaxTypeNumber(int taxTypeNumber)
        {
            throw new NotImplementedException();
        }

        public void SetTypeOplNumber(int typeOplNumber)
        {
            throw new NotImplementedException();
        }

        public uint GetMaxCashStringLength()
        {
            throw new NotImplementedException();
        }

        public bool IsPaper()
        {
            return kkt.IsPaper();
        }

        public void End()
        {
            log.WriteToLog("========== Работа завершена ==========");
        }

    }
}

using System;
using System.Collections.Generic;
using shared;
using System.Windows.Forms;

namespace chib.cash.shtrih64
{
    public class Cash : ICash
    {
        Config config;
        KKT kkt;
        Log log;
        InputParams ip;

        double chequeSumma = 0.0;

        private static Cash instance;

        protected Cash()
        {
            String iniFile = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".ini";
            config = new Config(iniFile);
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
            //kkt.Close();
            //kkt.Deinitialize();
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
                kkt.Open(config.AdministratorPassword, config.CashierPassword, 
                    config.ConnectionType, config.PortName, config.IpAddress, config.TCPPort);
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
                case 2:
                    log.WriteToLog("Смена открыта");
                    result = 0;
                    break;
                case 4:
                    log.WriteToLog("Смена закрыта");
                    break;
                case 3:
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
            ///  2 Открытая смена, 24 часа не кончились
            ///  3 Открытая смена, 24 часа кончились
            ///  4 Закрытая смена
            uint shiftState = kkt.GetShiftState();
            if (shiftState == 3)
            {
                // (3) - смена истекла (продолжительность смены больше 24 часов)
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
                    if (shiftState == 4)
                    {
                        log.WriteToLog("=> Смена закрыта");
                    }
                    else if (shiftState == 2 || shiftState == 3)
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
            if (shiftState == 4)
            {
                // (4) - смена закрыта
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

            /*
            // Регистрация кассира
            kkt.CashierRegistration(ip.CashierName, ip.CashierInn);

            // Установка значения тегов
            int tag1057 = config.Tag1057;
            if (tag1057 > 0)
            {
                log.WriteToLog("Признак агента [1057] взят из ini-файла: " + tag1057.ToString());
            }
            else
            {
                tag1057 = kkt.Get1057();
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

                // 1074 -  у нас нет оператора по приёму платежей (а значит и его телефона) [05.10.2021]
                //if (!String.IsNullOrEmpty(ip.PayAgentPhone))
                //{
                //    log.WriteToLog("Установка значения параметра: Телефон платежного агента [1074]");
                //    kkt.SetParam(1074, ip.PayAgentPhone);
                //}

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
            */

            // Установка типа чека
            // Диапазон значений: 0…3: «0» - продажа, «1» - покупка, «2» - возврат продажи, «3» - возврат покупки.
            log.WriteToLog("CheckType: Диапазон значений: 0…3: «0» - продажа, «1» - покупка, «2» - возврат продажи, «3» - возврат покупки");
            int typeCheque = 0;
            if (chequeType == ChequeType.SELL)
                typeCheque = 0;
            else if (chequeType == ChequeType.SELL_RETURN)
                typeCheque = 2;
            kkt.SetChequeType(typeCheque);

            if (ip.IsECheque)
            {
                // Печатать чек на кассовой ленте или это электронный чек
                log.WriteToLog("ip.IsECheque = " + (ip.IsECheque ? "Да" : "Нет"));
                kkt.SetNoPrintCheque(ip.IsECheque);
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
                            String taxTakenFrom = "";
                            uint taxTypeNumber_ = 0;
                            String value = pair.Value;
                            if (uint.TryParse(value, out uint val))
                            {
                                if (val >= 1102 && val <= 1107)
                                {
                                    if (val == 1102) taxTypeNumber_ = 1;      // 20%
                                    else if (val == 1103) taxTypeNumber_ = 2; // 10%
                                    else if (val == 1106) taxTypeNumber_ = 5; // 20/120
                                    else if (val == 1107) taxTypeNumber_ = 6; // 10/110
                                    else if (val == 1104) taxTypeNumber_ = 3; // 0%
                                    else if (val == 1105) taxTypeNumber_ = 4; // НЕ ОБЛАГАЕТСЯ
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

            chequeSumma += sum;
            result = kkt.Registration(srvText, (int)section, quantity, (decimal)sum, (int)tax, 4, 4);

            // Установка значения тегов
            // Признак агента (1222 и 1057)
            int tag1222 = config.Tag1057;
            if (tag1222 > 0)
            {
                log.WriteToLog("Признак агента [1222] взят из ini-файла: " + tag1222.ToString());
            }
            else
            {
                tag1222 = kkt.Get1057();
                log.WriteToLog("Признак агента [1222] взят из регистрационных данных: " + tag1222.ToString());
            }
            log.WriteToLog("Установка значения параметра: Признак агента [tag1222]");
            kkt.FNSendTagOperationInt(1222, 0, tag1222);

            // Запись тегов
            log.WriteToLog("Tag 1223 ->");
            kkt.FNBeginSTLVTag(1223);

            if (!String.IsNullOrEmpty(ip.OperatorName))
            {
                log.WriteToLog("Установка значения параметра: Наименование оператора перевода [1026]");
                kkt.FNAddTag(1026, KKT.TAGTYPE_STRING, ip.OperatorName);
            }

            if (!String.IsNullOrEmpty(ip.OperatorInn))
            {
                log.WriteToLog("Установка значения параметра: ИНН оператора перевода [1016]");
                kkt.FNAddTag(1016, KKT.TAGTYPE_STRING, ip.OperatorInn);
            }

            if (!String.IsNullOrEmpty(ip.OperatorAddress))
            {
                log.WriteToLog("Установка значения параметра: Адрес оператора перевода [1005]");
                kkt.FNAddTag(1005, KKT.TAGTYPE_STRING, ip.OperatorAddress);
            }

            if (!String.IsNullOrEmpty(ip.OperatorPhone))
            {
                log.WriteToLog("Установка значения параметра: Телефон оператора перевода [1075]");
                kkt.FNAddTag(1075, KKT.TAGTYPE_STRING, ip.OperatorPhone);
            }

            if (!String.IsNullOrEmpty(ip.AgentPhone))
            {
                log.WriteToLog("Установка значения параметра: Телефон платежного агента [1073]");
                kkt.FNAddTag(1073, KKT.TAGTYPE_STRING, ip.AgentPhone);
            }

            /*
            // 1074 -  у нас нет оператора по приёму платежей (а значит и его телефона) [05.10.2021]
            if (!String.IsNullOrEmpty(ip.PayAgentPhone))
            {
                log.WriteToLog("Установка значения параметра: Телефон платежного агента [1074]");
                kkt.FNAddTag(1074, KKT.TAGTYPE_STRING, ip.PayAgentPhone);
            }
            */

            if (!String.IsNullOrEmpty(ip.OperatorOper))
            {
                log.WriteToLog("Установка значения параметра: Операция платежного агента [1044]");
                kkt.FNAddTag(1044, KKT.TAGTYPE_STRING, ip.OperatorOper);
            }

            kkt.FNSendSTLVTagOperation();
            log.WriteToLog("<- Tag 1223");


            log.WriteToLog("Tag 1224 ->");
            kkt.FNBeginSTLVTag(1224);

            if (!String.IsNullOrEmpty(ip.PuPhone))
            {
                log.WriteToLog("Установка значения параметра: Телефон поставщика [1171]");
                kkt.FNAddTag(1171, KKT.TAGTYPE_STRING, ip.PuPhone);  // max 19 symbols
            }

            if (!String.IsNullOrEmpty(ip.PuName))
            {
                log.WriteToLog("Установка значения параметра: Наименование поставщика [1225]");
                kkt.FNAddTag(1225, KKT.TAGTYPE_STRING, ip.PuName);
            }

            kkt.FNSendSTLVTagOperation();
            log.WriteToLog("<- Tag 1224");

            return result;
        }

        public long CloseCheque(String income) // TODO?:  , PaymentType type = PaymentType.CASH)
        {
            long result = 0;

            if (ip.IsPreCheque)
            {
                log.WriteToLog("CloseCheque: пречек");

                // Частичная отрезка ленты
                int linesBeforeCut = config.LinesBeforeCut;
                log.WriteToLog("Пропустить строк перед отрезкой: " + linesBeforeCut.ToString());
                if (linesBeforeCut >= 0)
                {
                    for (int i = 0; i < linesBeforeCut; i++)
                        kkt.Print("");

                    kkt.Cut(true);  // true - неполная, false - полная отрезка
                }

                //shtrih_->FinishDocument();
                result = kkt.EndNonFiscalDoc();
                return result;
            }

          
            // Регистрация кассира
            kkt.CashierRegistration(ip.CashierName, ip.CashierInn);

/*
// Установка значения тегов
int tag1057 = config.Tag1057;
if (tag1057 > 0)
{
    log.WriteToLog("Признак агента [1057] взят из ini-файла: " + tag1057.ToString());
}
else
{
    tag1057 = kkt.Get1057();
    log.WriteToLog("Признак агента [1057] взят из регистрационных данных: " + tag1057.ToString());
}
log.WriteToLog("Установка значения параметра: Признак агента [1057]");
            kkt.FNSendTagOperationInt(1057, 0, tag1057);
            
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

    // 1074 -  у нас нет оператора по приёму платежей (а значит и его телефона) [05.10.2021]
    //if (!String.IsNullOrEmpty(ip.PayAgentPhone))
    //{
    //    log.WriteToLog("Установка значения параметра: Телефон платежного агента [1074]");
    //    kkt.SetParam(1074, ip.PayAgentPhone);
    //}

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
*/

            //uint v = (uint)((income * 100) + 0.5);
            //double sum = v;
            //sum = sum / 100;
            double sum = Utils.ToDoubleSumma(income);

            int typeOpl = 0; // 0 - наличные
            int chequeState = kkt.GetChequeState();

            // Регистрация оплаты
            if (chequeState == 0 /* SELL */) // открыт чек продажи
            {
                log.WriteToLog("CloseCheque: chequeSumma=" + chequeSumma.ToString());
                log.WriteToLog("CloseCheque: sum[before]=" + sum.ToString());

                // Если тип оплаты не наличные и была введена сумма оплаты, превышающая сумму чека
                // (если сумма оплаты меньше суммы чека - будет стандартная ошибка драйвера:
                //  77 - Вносимая безналичной оплатой сумма больше суммы чека)
                if (ip.IsCashlessCheque && sum > chequeSumma)
                {
                    // скорректировать сумму: сделать ее равной сумме чека
                    sum = chequeSumma;
                }

                log.WriteToLog("CloseCheque: sum[after]=" + sum.ToString());
                log.WriteToLog("CloseCheque: [Тип оплаты по-умолчанию: НАЛ] Сумма полученная=" + sum.ToString() +
                    " => " + result.ToString());
            }

            try
            {
                // Если БЕЗНАЛ, а тип платежа PayType не установлен (а по умолчанию он  = 1 (НАЛ)), 
                // то установить тип платежа = 2 (см Таблицу 5). 
                // А вообще для платежей по безналу надо прописыать в chib.cash.strih.ini нужный PayType
                if (ip.IsCashlessCheque)
                {
                    typeOpl = config.CashlessTypeClose;
                    if (typeOpl == 1) // Ошибочно стоит НАЛ -> взять 2-ю строку в таблице
                        typeOpl = 2;
                }
                else
                {
                    typeOpl = 1; // НАЛ
                }

                if (kkt.GetShiftState() != (uint)8)
                {
                    // Если не "8 Открытый документ"
                    kkt.CancelCheque();
                    return 0;
                }

                // Закрытие чека
                result = kkt.CloseCheque(sum, ip.IsCashlessCheque ? typeOpl : 1);

            }
            catch (Exception ex)
            {
              kkt.CancelCheque();
              log.WriteToLog("Чек был отменен: " + ex.Message);
            }

            // Получение информации о номере чека из ФН
            //uint documentNumber = kkt.GetLastDocumentNumber();
            //log.WriteToLog(">>> ФД № = " + documentNumber.ToString());

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
                /*
                // Сумма выручки
                double revenue = kkt.GetRevenue();
                log.WriteToLog("Сумма выручки: " + revenue.ToString());

                // Сумма внесений
                double sumCashIn = kkt.GetCashIn();
                log.WriteToLog("Сумма внесений: " + sumCashIn.ToString());

                // Сумма выплат
                double sumCashOut = kkt.GetCashOut();
                log.WriteToLog("Сумма выплат: " + sumCashOut.ToString());
                */
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
                /*
                // Сумма выручки
                double revenue = kkt.GetRevenue();
                log.WriteToLog("Сумма выручки: " + revenue.ToString());

                // Сумма внесений
                double sumCashIn = kkt.GetCashIn();
                log.WriteToLog("Сумма внесений: " + sumCashIn.ToString());

                // Сумма выплат
                double sumCashOut = kkt.GetCashOut();
                log.WriteToLog("Сумма выплат: " + sumCashOut.ToString());
                */
                result = kkt.Outcome(Utils.ToDoubleSumma(sum));

                /*
                if (result == -1)
                {
                    if (revenue + sumCashIn - sumCashOut <= sumCashOut)
                    {
                        log.WriteToLog(Log.Level.Warning, "Указанной суммы нет в кассе!");
                        MessageBox.Show("Указанной суммы нет в кассе!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                */
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

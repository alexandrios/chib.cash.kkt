using System;
using DrvFRLib;
using shared;

namespace chib.cash.shtrih64
{
    public class KKT
    {
        public const int TAGTYPE_BYTE = 0;
        public const int TAGTYPE_UINT16 = 1;
        public const int TAGTYPE_UINT32 = 2;
        public const int TAGTYPE_VLN = 3;
        public const int TAGTYPE_FVLN = 4;
        public const int TAGTYPE_BITMASK = 5;
        public const int TAGTYPE_UNIXTIME = 6;
        public const int TAGTYPE_STRING = 7;

        Log log;
        DrvFR fptr;
        int userPassword;
        int adminPassword;

        public KKT(Log log)
        {
            this.log = log;
        }

        public void Initialize()
        {
            log.WriteToLog("fptr -> new DrvFR()");
            try
            {
                fptr = new DrvFR();
                log.WriteToLog("fptr = new DrvFR()");
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, ex.StackTrace + " " + ex.Message);
            }
        }

        public void Deinitialize()
        {
            /*
            try
            {
                fptr.Disconnect();
                log.WriteToLog("fptr.Disconnect()");
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "fptr.Disconnect(): " + ex.Message);
            }
            */
        }

        public bool IsOpened()
        {
            bool isOpened = fptr.Connected;
            log.WriteToLog("fptr.Connected: => " + isOpened.ToString());
            return isOpened;
        }

        public bool Open(int adminPassword, int userPassword, int connectionType, String portName, String ipAddress, String tcpPort)
        {
            this.adminPassword = adminPassword;
            this.userPassword = userPassword;
            bool isOpened = false;
            try
            {
                if (portName == "AUTO" && connectionType != 6)
                {
                    log.WriteToLog("PortName = AUTO");
                }
                else
                {
                    log.WriteToLog("Установка типа соединения с ККТ: " + connectionType.ToString());

                    // Установка способа соединения
                    fptr.ConnectionType = connectionType;

                    if (fptr.ConnectionType == 6) // подключение через TCP socket
                    {
                        fptr.ProtocolType = 0;    // Стандартный протокол 

                        fptr.IPAddress = ipAddress; // IP адрес ККТ
                        log.WriteToLog("IPAddress=" + fptr.IPAddress);

                        // Используем свойство IPAddress для указания адреса ККТ(в противном случае будет использоваться свойство ComputerName)
                        fptr.UseIPAddress = true;

                        try
                        {
                            fptr.TCPPort = int.Parse(tcpPort);   // TCP порт ККТ
                            log.WriteToLog("TCPPort=" + fptr.TCPPort.ToString());
                        }
                        catch (Exception ex)
                        {
                            log.WriteToLog(Log.Level.Error, "Ошибка установки TCPPort " + ex.Message);
                            throw new Exception("");
                        }
                    }
                    else if (fptr.ConnectionType == 0) // Подключение через COM порт
                    {
                        try
                        {
                            int portNumber = int.Parse(portName.Substring(3));
                            fptr.ComNumber = portNumber;
                            log.WriteToLog("ComNumber=" + fptr.ComNumber.ToString());
                        }
                        catch (Exception ex)
                        {
                            log.WriteToLog(Log.Level.Error, "Ошибка установки ComNumber " + ex.Message);
                            throw new Exception("");
                        }
                    }
                }

                // Открыть соединение с ККТ
                fptr.Password = this.userPassword;
                fptr.Connect();
                isOpened = fptr.Connected;
                log.WriteToLog("fptr.Connect(): isOpened()=" + isOpened.ToString());
                log.WriteToLog("Версия драйвера: " + fptr.DriverVersion);
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "fptr.Connect(): " + ex.Message);
            }

            return isOpened;
        }

        public void RequestInfoKKT()
        {
            log.WriteToLog("Запрос общей информации и статуса ККТ:");

            fptr.Password = this.adminPassword;

            fptr.TableNumber = 18;
            fptr.FieldNumber = 2;
            fptr.RowNumber = 1;
            fptr.GetFieldStruct();
            fptr.ReadTable();
            log.WriteToLog("UserINN = " + fptr.ValueOfFieldString);

            fptr.TableNumber = 18;
            fptr.FieldNumber = 7;
            fptr.RowNumber = 1;
            fptr.GetFieldStruct();
            fptr.ReadTable();
            log.WriteToLog("User = " + fptr.ValueOfFieldString);

            fptr.TableNumber = 18;
            fptr.FieldNumber = 9;
            fptr.RowNumber = 1;
            fptr.GetFieldStruct();
            fptr.ReadTable();
            log.WriteToLog("Address = " + fptr.ValueOfFieldString);

            fptr.Password = this.userPassword;
        }

        public void RequestRegInfo()
        {
            //log.WriteToLog("Запрос реквизитов регистрации ККТ:");

            /*
            fptr.setParam(Constants.LIBFPTR_PARAM_FN_DATA_TYPE, Constants.LIBFPTR_FNDT_REG_INFO);
            fptr.fnQueryData();

            uint taxationTypes = fptr.getParamInt(1062);
            uint agentSign = fptr.getParamInt(1057);
            uint ffdVersion = fptr.getParamInt(1209);

            bool autoModeSign = fptr.getParamBool(1001);
            bool offlineModeSign = fptr.getParamBool(1002);
            bool encryptionSign = fptr.getParamBool(1056);
            bool internetSign = fptr.getParamBool(1108);
            bool serviceSign = fptr.getParamBool(1109);
            bool bsoSign = fptr.getParamBool(1110);
            bool lotterySign = fptr.getParamBool(1126);
            bool gamblingSign = fptr.getParamBool(1193);
            bool exciseSign = fptr.getParamBool(1207);
            bool machineInstallationSign = fptr.getParamBool(1221);

            String fnsUrl = fptr.getParamString(1060);
            String organizationAddress = fptr.getParamString(1009);
            String organizationVATIN = fptr.getParamString(1018);
            String organizationName = fptr.getParamString(1048);
            String organizationEmail = fptr.getParamString(1117);
            String paymentsAddress = fptr.getParamString(1187);
            String registrationNumber = fptr.getParamString(1037);
            String machineNumber = fptr.getParamString(1036);
            String ofdVATIN = fptr.getParamString(1017);
            String ofdName = fptr.getParamString(1046);

            log.WriteToLog("taxationTypes(1062)=" + taxationTypes.ToString());
            log.WriteToLog("agentSign(1057)=" + agentSign.ToString());
            log.WriteToLog("ffdVersion(1209)=" + ffdVersion.ToString());

            log.WriteToLog("autoModeSign(1001)=" + autoModeSign.ToString());
            log.WriteToLog("offlineModeSign(1002)=" + offlineModeSign.ToString());
            log.WriteToLog("encryptionSign(1056)=" + encryptionSign.ToString());
            log.WriteToLog("internetSign(1108)=" + internetSign.ToString());
            log.WriteToLog("serviceSign(1109)=" + serviceSign.ToString());
            log.WriteToLog("bsoSign(1110)=" + bsoSign.ToString());
            log.WriteToLog("lotterySign(1126)=" + lotterySign.ToString());
            log.WriteToLog("gamblingSign(1193)=" + gamblingSign.ToString());
            log.WriteToLog("exciseSign(1207)=" + exciseSign.ToString());
            log.WriteToLog("machineInstallationSign(1221)=" + machineInstallationSign.ToString());

            log.WriteToLog("fnsUrl(1060)=" + fnsUrl);
            log.WriteToLog("organizationAddress(1009)=" + organizationAddress);
            log.WriteToLog("organizationVATIN(1018)=" + organizationVATIN);
            log.WriteToLog("organizationName(1048)=" + organizationName);
            log.WriteToLog("organizationEmail(1117)=" + organizationEmail);
            log.WriteToLog("paymentsAddress(1187)=" + paymentsAddress);
            log.WriteToLog("registrationNumber(1037)=" + registrationNumber);
            log.WriteToLog("machineNumber(1036)=" + machineNumber);
            log.WriteToLog("ofdVATIN(1017)=" + ofdVATIN);
            log.WriteToLog("ofdName(1046)=" + ofdName);
            */
        }

        public void Close()
        {
            try
            {
                if (fptr.Connected)
                {
                    int result = fptr.Disconnect();
                    log.WriteToLog("fptr.Disconnect(): " + result.ToString());
                }
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "fptr.Disconnect(): " + ex.Message);
            }
        }

        public int Get1057()
        {
            fptr.Password = this.adminPassword;
            fptr.TableNumber = 18;
            fptr.FieldNumber = 16;
            fptr.RowNumber = 1;
            fptr.GetFieldStruct();
            int result = fptr.ReadTable();
            log.WriteToLog("ReadTable[18, 16, 1] => " + result.ToString());
            fptr.Password = this.userPassword;
            log.WriteToLog("Признак агента (1057) = " + fptr.ValueOfFieldInteger.ToString());
            return fptr.ValueOfFieldInteger;
        }

        /// <summary>
        /// Проверка наличия кассовой ленты.
        /// </summary>
        /// <returns></returns>
        public bool IsPaper()
        {
            /*
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_SHORT_STATUS);
            fptr.queryData();
            bool isPaperPresent = fptr.getParamBool(Constants.LIBFPTR_PARAM_RECEIPT_PAPER_PRESENT);
            log.WriteToLog("Проверка наличия кассовой ленты: isPaperPresent=" + isPaperPresent.ToString());
            return isPaperPresent;
            */
            return true;
        }

        /// <summary>
        /// Получить состояние смены.
        ///  0 Принтер в рабочем режиме
        ///  1 Выдача данных
        ///  2 Открытая смена, 24 часа не кончились
        ///  3 Открытая смена, 24 часа кончились
        ///  4 Закрытая смена
        ///  5 Блокировка по неправильному паролю налогового инспектора
        ///  6 Ожидание подтверждения ввода даты
        ///  7 Разрешение изменения положения десятичной точки
        ///  8 Открытый документ
        ///  9 Режим разрешения технологического обнуления
        /// 10 Тестовый прогон
        /// 11 Печать полного фискального отчета
        /// 12 Печать длинного отчета ЭКЛЗ
        /// 13 Работа с фискальным подкладным документом
        /// 14 Печать подкладного документа
        /// 15 Фискальный подкладной документ сформирован
        /// </summary>
        /// <returns></returns>
        public uint GetShiftState()
        {
            fptr.Password = this.userPassword;
            fptr.GetShortECRStatus();
            uint state = (uint)fptr.ECRMode; 
            log.WriteToLog("Состояние смены: state=" + state.ToString());
            return state;
        }

        /// <summary>
        /// Регистрация кассира.
        /// </summary>
        /// <param name="cashierName"></param>
        /// <param name="cashierInn"></param>
        public int CashierRegistration(String cashierName, String cashierInn)
        {
            int result = 0;

            if (!String.IsNullOrEmpty(cashierName))
            {
                fptr.TagNumber = 1021;
                fptr.TagType = TAGTYPE_STRING;
                fptr.TagValueStr = cashierName;
                result += fptr.FNSendTag();
            }

            if (!String.IsNullOrEmpty(cashierInn))
            {
                fptr.TagNumber = 1203;
                fptr.TagType = TAGTYPE_STRING;
                fptr.TagValueStr = cashierInn;
                result += fptr.FNSendTag();
            }

            log.WriteToLog("Регистрация кассира: cashierName=" + cashierName + " cashierInn=" + cashierInn);
            return result;
        }

        /// <summary>
        /// Открытие смены.
        /// </summary>
        /// <returns></returns>
        public int OpenShift()
        {
            fptr.Password = this.userPassword;
            int result = fptr.OpenSession();
            log.WriteToLog("fptr.OpenSession() => " + result.ToString());
            CheckDocumentClosed();
            return result;
        }

        /// <summary>
        /// Проверка закрытия документа
        /// </summary>
        public void CheckDocumentClosed()
        {
            /*
            int cnt = 0;
            while (fptr.checkDocumentClosed() < 0 && cnt < 10)
            {
                // Не удалось проверить состояние документа. 
                // Вывести пользователю текст ошибки, попросить устранить неполадку и повторить запрос
                log.WriteToLog("Не удалось проверить состояние документа: " + fptr.errorDescription());
                cnt++;
                System.Threading.Thread.Sleep(500);
                continue;
            }

            if (!fptr.getParamBool(Constants.LIBFPTR_PARAM_DOCUMENT_CLOSED))
            {
                // Документ не закрылся. Требуется его отменить (если это чек) и сформировать заново
                log.WriteToLog("Документ не закрылся. Требуется его отменить (если это чек) и сформировать заново");
                fptr.cancelReceipt();
                return;
            }

            if (!fptr.getParamBool(Constants.LIBFPTR_PARAM_DOCUMENT_PRINTED))
            {
                cnt = 0;
                // Можно сразу вызвать метод допечатывания документа, он завершится с ошибкой, если это невозможно
                while (fptr.continuePrint() < 0 && cnt < 10)
                {
                    // Если не удалось допечатать документ - показать пользователю ошибку и попробовать еще раз.
                    log.WriteToLog(String.Format("Не удалось напечатать документ (Ошибка \"{0}\"). Устраните неполадку и повторите.",
                        fptr.errorDescription()));
                    cnt++;
                    System.Threading.Thread.Sleep(500);
                    continue;
                }
            }
            */
        }

        /// <summary>
        /// Открытие чека.
        /// </summary>
        /// <returns></returns>
        public int OpenCheque()
        {
            int result = fptr.OpenCheck();
            log.WriteToLog("fptr.OpenCheck() => " + result.ToString());
            return result;
        }

        /// <summary>
        /// (Получить состояние чека).
        /// Тип открываемого документа/чека.
        /// Диапазон значений: 0…3: «0» - продажа, «1» - покупка, «2» - возврат продажи, «3» - возврат покупки.
        /// Используется методами OpenCheck
        /// </summary>
        /// <returns></returns>
        public int GetChequeState()
        {
            int result = fptr.CheckType;
            log.WriteToLog("Состояние чека: " + result.ToString());
            return result;
        }

        public int Registration(String stringForPrinting, int department, double quantity, decimal price, int tax,
            int paymentTypeSign, int paymentItemSign)
        {
            fptr.StringForPrinting = stringForPrinting;
            fptr.Department = department;
            fptr.Quantity = quantity;
            fptr.Price = price;
            fptr.Tax1 = tax;
            if (tax >= 1 && tax <= 6)
            {
                fptr.TaxValue1Enabled = true;
            }
            else
            {
                fptr.TaxValue1Enabled = false;
            }
            fptr.PaymentTypeSign = paymentTypeSign;  // Признак способа расчета (Полный расчет). Необходим для ФФД 1.05
            fptr.PaymentItemSign = paymentItemSign;  // Признак предмета расчета (Услуга). Необходим для ФФД 1.05

            int result = fptr.FNOperation();
            log.WriteToLog("fptr.FNOperation() => " + result.ToString());
            return result;
        }

        /*
        public int Payment()
        {
            int result = fptr.payment();
            log.WriteToLog("fptr.payment() => " + result.ToString());
            return result;
        }
        */

        /*
        public uint GetLastDocumentNumber()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_FN_DATA_TYPE, Constants.LIBFPTR_FNDT_LAST_RECEIPT);
            fptr.fnQueryData();
            uint documentNumber = fptr.getParamInt(Constants.LIBFPTR_PARAM_DOCUMENT_NUMBER);
            return documentNumber;
        }
        */

        public int XReport()
        {
            fptr.Password = this.adminPassword;
            int result = fptr.PrintReportWithoutCleaning();
            log.WriteToLog("XReport() => " + result.ToString());
            CheckDocumentClosed();
            return result;
        }

        public int ZReport()
        {
            fptr.Password = this.adminPassword;
            int result = fptr.PrintReportWithCleaning();
            log.WriteToLog("ZReport() => " + result.ToString());
            CheckDocumentClosed();
            return result;
        }

        public int Print(String text)
        {
            fptr.UseReceiptRibbon = true;
            fptr.UseJournalRibbon = true;
            fptr.StringForPrinting = text;
            int result = fptr.PrintString();
            log.WriteToLog("PrintString() => " + result.ToString() + " text=" + text);
            return result;
        }

        public int CancelCheque()
        {
            int result = fptr.CancelCheck();
            log.WriteToLog("fptr.CancelCheck() => " + result.ToString());
            return result;
        }

        public int CloseCheque(double sum, int typeOpl)
        {
            SetSummaPayType((decimal)sum, typeOpl);

            fptr.DiscountOnCheck = 0;
            //Налог был определен ранее при регистрации продажи или возврата
            //fptr.Tax1 = tax;
            fptr.StringForPrinting = "";

            //WaitCash(shtrih_);
            int result = fptr.FNCloseCheckEx();

            log.WriteToLog("fptr.FNCloseCheckEx() => " + result.ToString());
            CheckDocumentClosed();
            return result;
        }

        private void SetSummaPayType(decimal sum, int typeOpl)
        {
            fptr.Summ1 = 0;
            fptr.Summ2 = 0;
            fptr.Summ3 = 0;
            fptr.Summ4 = 0;

            switch (typeOpl)
            {
                case 1: // наличные
                    fptr.Summ1 = sum;
                    break;
                case 2:
                    fptr.Summ2 = sum;
                    break;
                case 3:
                    fptr.Summ3 = sum;
                    break;
                case 4:
                    fptr.Summ4 = sum;
                    break;
            }
        }

        public int BeginNonFiscalDoc()
        {
            int result = fptr.OpenNonfiscalDocument();
            log.WriteToLog("fptr.OpenNonfiscalDocument() => " + result.ToString());
            return result;
        }

        public int EndNonFiscalDoc()
        {
            int result = fptr.CloseNonFiscalDocument();
            log.WriteToLog("fptr.CloseNonFiscalDocument() => " + result.ToString());
            return result;
        }

        public int Income(double value)
        {
            fptr.Summ1 = (decimal)value; // to_currency(sum);
            fptr.Password = this.userPassword;
            //WaitCash(shtrih_);
            int result = fptr.CashIncome();
            log.WriteToLog("fptr.CashIncome(): value=" + value.ToString() + " => " + result.ToString());
            return result;
        }

        public int Outcome(double value)
        {
            fptr.Summ1 = (decimal)value; // to_currency(sum);
            fptr.Password = this.userPassword;
            //WaitCash(shtrih_);
            int result = fptr.CashOutcome();
            log.WriteToLog("fptr.CashOutcome(): value=" + value.ToString() + " => " + result.ToString());
            return result;
        }

        public int FNSendTagOperationInt(int tagNumber, int tagType, int tagValueInt)
        {
            fptr.TagNumber = tagNumber;
            fptr.TagType = tagType;
            log.WriteToLog("Tag " + tagNumber.ToString() + " = " + tagValueInt.ToString());
            fptr.TagValueInt = tagValueInt;
            int result = fptr.FNSendTagOperation();
            return result;
        }

        public int FNBeginSTLVTag(int tagNumber)
        {
            fptr.TagNumber = tagNumber;
            int result = fptr.FNBeginSTLVTag();
            return result;
        }

        public int FNAddTag(int tagNumber, int tagType, String tagValue)
        {
            fptr.TagNumber = tagNumber;
            fptr.TagType = tagType;
            log.WriteToLog("Tag " + tagNumber.ToString() + " = " + tagValue);
            fptr.TagValueStr = tagValue;
            int result = fptr.FNSendTagOperation();
            return result;
        }

        public int FNSendSTLVTagOperation()
        {
            int result = fptr.FNSendSTLVTagOperation();
            return result;
        }

        /*
        /// <summary>
        /// Возвращает сумму выручки.
        /// </summary>
        /// <returns></returns>
        public double GetRevenue()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_REVENUE);
            fptr.queryData();
            double value = fptr.getParamDouble(Constants.LIBFPTR_PARAM_SUM);
            return value;
        }

        /// <summary>
        /// Возвращает сумму внесений.
        /// </summary>
        /// <returns></returns>
        public double GetCashIn()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_CASHIN_SUM);
            fptr.queryData();
            double value = fptr.getParamDouble(Constants.LIBFPTR_PARAM_SUM);
            return value;
        }

        /// <summary>
        /// Возвращает сумму выплат.
        /// </summary>
        /// <returns></returns>
        public double GetCashOut()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_CASHOUT_SUM);
            fptr.queryData();
            double value = fptr.getParamDouble(Constants.LIBFPTR_PARAM_SUM);
            return value;
        }
        */

        /// <summary>
        /// Установить тип чека.
        /// Диапазон значений: 0…3: «0» - продажа, «1» - покупка, «2» - возврат продажи, «3» - возврат покупки.
        /// ВНИМАНИЕ! Отличается от Атол!
        /// </summary>
        /// <param name="typeCheque"></param>
        public void SetChequeType(int typeCheque)
        {
            fptr.CheckType = typeCheque;
            log.WriteToLog("fptr.CheckType = " + typeCheque.ToString());
        }

        private int GetNoPrintCheck()
        {
            fptr.Password = this.adminPassword;
            fptr.TableNumber = 17;
            fptr.FieldNumber = 7;
            fptr.RowNumber = 1;
            fptr.GetFieldStruct();
            int result = fptr.ReadTable();
            log.WriteToLog("ReadTable[17, 7, 1] = " + result.ToString());
            fptr.Password = this.userPassword;
            log.WriteToLog("Признака печати на кассовой ленте из таблицы: = " + fptr.ValueOfFieldInteger.ToString());
            return fptr.ValueOfFieldInteger;
        }

        public int SetNoPrintCheque(bool isECheque)
        {
            // Параметр, который отключает печать чека на ленте - это поле 7 в таблице 17: 
            // 0 - документ печатается 
            // 1 - не печатается следующий документ (после сбрасывается в 0) 
            // 2 - не печатаются все документы (устанавливается и не сбрасывается)

            int attr = isECheque ? 1 : 0;
            if (this.GetNoPrintCheck() == attr)
            {
                return 0;
            }

            log.WriteToLog(isECheque ? "Отключить печать на кассовой ленте" : "Включить печать на кассовой ленте");
            fptr.Password = this.adminPassword;
            fptr.TableNumber = 17;
            fptr.FieldNumber = 7;
            fptr.RowNumber = 1;
            fptr.ValueOfFieldInteger = attr;
            fptr.GetFieldStruct();
            int result = fptr.WriteTable();
            log.WriteToLog("WriteTable[17, 7, 1] = " + result.ToString());
            fptr.Password = this.userPassword;
            return result;
        }

        public int Cut(bool cutType)
        {
            fptr.CutType = cutType;
            int result = fptr.CutCheck();
            log.WriteToLog("fptr.CutCheck(): [fptr.CutType=" + cutType.ToString() + "] => " + result.ToString());
            return result;
        }
    }
}

using System;
using Atol.Drivers10.Fptr;
using shared;

namespace chib.cash.atol64
{
    public class KKT
    {
        Log log;
        IFptr fptr;

        public KKT(Log log)
        {
            this.log = log;
        }

        public void Initialize()
        {
            fptr = new Fptr();
        }

        public void Deinitialize()
        {
            try
            {
                fptr.destroy();
                log.WriteToLog("fptr.destroy()");
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "fptr.destroy(): " + ex.Message);
            }
        }

        public bool IsOpened()
        {
            bool isOpened = fptr.isOpened();
            log.WriteToLog("fptr.isOpened(): => " + isOpened.ToString());
            return isOpened;
        }

        public bool Open(String portName, String ipAddress, String tcpPort)
        {
            bool isOpened = false;
            try
            {
                // Модель ККТ
                fptr.setSingleSetting(Constants.LIBFPTR_SETTING_MODEL, Constants.LIBFPTR_MODEL_ATOL_AUTO.ToString());

                // Установка способа соединения
                if (portName != "AUTO")
                {
                    String portType = portName.Substring(0, 3);
                    if (portType == "COM")
                    {
                        fptr.setSingleSetting(Constants.LIBFPTR_SETTING_PORT, Constants.LIBFPTR_PORT_COM.ToString());
                        fptr.setSingleSetting(Constants.LIBFPTR_SETTING_COM_FILE, portName);
                    }
                    else if (portType == "TCP")
                    {
                        fptr.setSingleSetting(Constants.LIBFPTR_SETTING_PORT, Constants.LIBFPTR_PORT_TCPIP.ToString());
                        fptr.setSingleSetting(Constants.LIBFPTR_SETTING_IPADDRESS, ipAddress);
                        fptr.setSingleSetting(Constants.LIBFPTR_SETTING_IPPORT, tcpPort);
                    }

                    //fptr.setSingleSetting(Constants.LIBFPTR_SETTING_BAUDRATE, Constants.LIBFPTR_PORT_BR_115200.ToString());
                }

                // Применить настройки
                fptr.applySingleSettings();

                // Открыть соединение с ККТ
                fptr.open();
                isOpened = fptr.isOpened();
                log.WriteToLog("fptr.open(): isOpened()=" + isOpened.ToString());
                log.WriteToLog("Версия драйвера: " + fptr.version());
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "fptr.open(): " + ex.Message);
            }

            return isOpened;
        }

        public void RequestInfoKKT()
        {
            log.WriteToLog("Запрос общей информации и статуса ККТ:");

            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            fptr.queryData();

            uint operatorID = fptr.getParamInt(Constants.LIBFPTR_PARAM_OPERATOR_ID);
            uint logicalNumber = fptr.getParamInt(Constants.LIBFPTR_PARAM_LOGICAL_NUMBER);
            uint shiftState = fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_STATE);
            uint model = fptr.getParamInt(Constants.LIBFPTR_PARAM_MODEL);
            uint mode = fptr.getParamInt(Constants.LIBFPTR_PARAM_MODE);
            uint submode = fptr.getParamInt(Constants.LIBFPTR_PARAM_SUBMODE);
            uint receiptNumber = fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_NUMBER);
            uint documentNumber = fptr.getParamInt(Constants.LIBFPTR_PARAM_DOCUMENT_NUMBER);
            uint shiftNumber = fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_NUMBER);
            uint receiptType = fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_TYPE);
            uint lineLength = fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_LINE_LENGTH);
            uint lineLengthPix = fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_LINE_LENGTH_PIX);

            double receiptSum = fptr.getParamDouble(Constants.LIBFPTR_PARAM_RECEIPT_SUM);

            bool isFiscalDevice = fptr.getParamBool(Constants.LIBFPTR_PARAM_FISCAL);
            bool isFiscalFN = fptr.getParamBool(Constants.LIBFPTR_PARAM_FN_FISCAL);
            bool isFNPresent = fptr.getParamBool(Constants.LIBFPTR_PARAM_FN_PRESENT);
            bool isInvalidFN = fptr.getParamBool(Constants.LIBFPTR_PARAM_INVALID_FN);
            bool isCashDrawerOpened = fptr.getParamBool(Constants.LIBFPTR_PARAM_CASHDRAWER_OPENED);
            bool isPaperPresent = fptr.getParamBool(Constants.LIBFPTR_PARAM_RECEIPT_PAPER_PRESENT);
            bool isPaperNearEnd = fptr.getParamBool(Constants.LIBFPTR_PARAM_PAPER_NEAR_END);
            bool isCoverOpened = fptr.getParamBool(Constants.LIBFPTR_PARAM_COVER_OPENED);
            bool isPrinterConnectionLost = fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_CONNECTION_LOST);
            bool isPrinterError = fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_ERROR);
            bool isCutError = fptr.getParamBool(Constants.LIBFPTR_PARAM_CUT_ERROR);
            bool isPrinterOverheat = fptr.getParamBool(Constants.LIBFPTR_PARAM_PRINTER_OVERHEAT);
            bool isDeviceBlocked = fptr.getParamBool(Constants.LIBFPTR_PARAM_BLOCKED);

            DateTime dateTime = fptr.getParamDateTime(Constants.LIBFPTR_PARAM_DATE_TIME);

            String serialNumber = fptr.getParamString(Constants.LIBFPTR_PARAM_SERIAL_NUMBER);
            String modelName = fptr.getParamString(Constants.LIBFPTR_PARAM_MODEL_NAME);
            String firmwareVersion = fptr.getParamString(Constants.LIBFPTR_PARAM_UNIT_VERSION);

            log.WriteToLog("operatorID=" + operatorID.ToString());
            log.WriteToLog("logicalNumber=" + logicalNumber.ToString());
            log.WriteToLog("shiftState=" + shiftState.ToString());
            log.WriteToLog("model=" + model.ToString());
            log.WriteToLog("mode=" + mode.ToString());
            log.WriteToLog("submode=" + submode.ToString());
            log.WriteToLog("receiptNumber=" + receiptNumber.ToString());
            log.WriteToLog("documentNumber=" + documentNumber.ToString());
            log.WriteToLog("shiftNumber=" + shiftNumber.ToString());
            log.WriteToLog("receiptType=" + receiptType.ToString());
            log.WriteToLog("lineLength=" + lineLength.ToString());
            log.WriteToLog("lineLengthPix=" + lineLengthPix.ToString());

            log.WriteToLog("receiptSum=" + receiptSum.ToString());

            log.WriteToLog("isFiscalDevice=" + isFiscalDevice.ToString());
            log.WriteToLog("isFiscalFN=" + isFiscalFN.ToString());
            log.WriteToLog("isFNPresent=" + isFNPresent.ToString());
            log.WriteToLog("isInvalidFN=" + isInvalidFN.ToString());
            log.WriteToLog("isCashDrawerOpened=" + isCashDrawerOpened.ToString());
            log.WriteToLog("isPaperPresent=" + isPaperPresent.ToString());
            log.WriteToLog("isPaperNearEnd=" + isPaperNearEnd.ToString());
            log.WriteToLog("isCoverOpened=" + isCoverOpened.ToString());
            log.WriteToLog("isPrinterConnectionLost=" + isPrinterConnectionLost.ToString());
            log.WriteToLog("isPrinterError=" + isPrinterError.ToString());
            log.WriteToLog("isCutError=" + isCutError.ToString());
            log.WriteToLog("isPrinterOverheat=" + isPrinterOverheat.ToString());
            log.WriteToLog("isDeviceBlocked=" + isDeviceBlocked.ToString());

            log.WriteToLog("dateTime=" + dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            log.WriteToLog("serialNumber=" + serialNumber);
            log.WriteToLog("modelName=" + modelName);
            log.WriteToLog("firmwareVersion=" + firmwareVersion);
        }

        public void RequestRegInfo()
        {
            log.WriteToLog("Запрос реквизитов регистрации ККТ:");

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
        }

        public void Close()
        {
            try
            {
                if (fptr.isOpened())
                {
                    fptr.close();
                    log.WriteToLog("fptr.close()");
                }
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, "fptr.close(): " + ex.Message);
            }
        }

        public uint GetRegInfoParamUInt(int paramId)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_FN_DATA_TYPE, Constants.LIBFPTR_FNDT_REG_INFO);
            fptr.queryData();
            uint result = fptr.getParamInt(paramId);
            log.WriteToLog("fptr.getParamInt(): paramId=" + paramId.ToString() + " => " + result.ToString());
            return result;
        }

        /// <summary>
        /// Проверка наличия кассовой ленты.
        /// </summary>
        /// <returns></returns>
        public bool IsPaper()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_SHORT_STATUS);
            fptr.queryData();
            bool isPaperPresent = fptr.getParamBool(Constants.LIBFPTR_PARAM_RECEIPT_PAPER_PRESENT);
            log.WriteToLog("Проверка наличия кассовой ленты: isPaperPresent=" + isPaperPresent.ToString());
            return isPaperPresent;
        }

        /// <summary>
        /// Получить состояние смены.
        /// LIBFPTR_SS_CLOSED  (0) - смена закрыта
        /// LIBFPTR_SS_OPENED  (1) - смена открыта
        /// LIBFPTR_SS_EXPIRED (2) - смена истекла (продолжительность смены больше 24 часов)
        /// </summary>
        /// <returns></returns>
        public uint GetShiftState()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            fptr.queryData();
            uint state = fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_STATE);
            uint number = fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_NUMBER);
            DateTime dateTime = fptr.getParamDateTime(Constants.LIBFPTR_PARAM_DATE_TIME);
            log.WriteToLog("Состояние смены: state=" + state.ToString() + " number=" + number.ToString() +
                " dateTime=" + dateTime.ToString("yyyy-MM-dd HH:mm:ss"));

            return state;
        }

        /// <summary>
        /// Регистрация кассира.
        /// </summary>
        /// <param name="cashierName"></param>
        /// <param name="cashierInn"></param>
        public int CashierRegistration(String cashierName, String cashierInn)
        {
            fptr.setParam(1021, cashierName);
            fptr.setParam(1203, cashierInn);
            int result = fptr.operatorLogin();
            log.WriteToLog("Регистрация кассира: cashierName=" + cashierName + " cashierInn=" +
                cashierInn + " fptr.operatorLogin() => " + result.ToString());
            return result;
        }

        /// <summary>
        /// Открытие смены.
        /// </summary>
        /// <returns></returns>
        public int OpenShift()
        {
            int result = fptr.openShift();
            log.WriteToLog("fptr.openShift() => " + result.ToString());
            CheckDocumentClosed();
            return result;
        }

        /// <summary>
        /// Проверка закрытия документа
        /// </summary>
        public void CheckDocumentClosed()
        {
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
        }

        /// <summary>
        /// Открытие чека.
        /// </summary>
        /// <returns></returns>
        public int OpenCheque()
        {
            int result = fptr.openReceipt();
            log.WriteToLog("fptr.openReceipt() => " + result.ToString());
            return result;
        }

        /// <summary>
        /// Получить состояние чека.
        /// LIBFPTR_RT_CLOSED (0) - чек закрыт
        /// LIBFPTR_RT_SELL   (1) - чек прихода(продажи)
        /// LIBFPTR_RT_SELL_RETURN (2) - чек возврата прихода(продажи)
        /// LIBFPTR_RT_SELL_CORRECTION (7) - чек коррекции прихода(продажи)
        /// LIBFPTR_RT_BUY    (4) - чек расхода(покупки)
        /// LIBFPTR_RT_BUY_RETURN  (5) - чек возврата расхода(покупки)
        /// LIBFPTR_RT_BUY_CORRECTION  (9) - чек коррекции расхода(покупки)
        /// </summary>
        /// <returns></returns>
        public uint GetChequeState()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_RECEIPT_STATE);
            fptr.queryData();
            uint receiptType = fptr.getParamInt(Constants.LIBFPTR_PARAM_RECEIPT_TYPE);
            log.WriteToLog("Состояние чека: " + receiptType.ToString());

            return receiptType;
        }

        public int Registration()
        {
            int result = fptr.registration();
            log.WriteToLog("fptr.registration() => " + result.ToString());
            return result;
        }

        public int Payment()
        {
            int result = fptr.payment();
            log.WriteToLog("fptr.payment() => " + result.ToString());
            return result;
        }

        public uint GetLastDocumentNumber()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_FN_DATA_TYPE, Constants.LIBFPTR_FNDT_LAST_RECEIPT);
            fptr.fnQueryData();
            uint documentNumber = fptr.getParamInt(Constants.LIBFPTR_PARAM_DOCUMENT_NUMBER);
            return documentNumber;
        }

        public int XReport()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_REPORT_TYPE, Constants.LIBFPTR_RT_X);
            int result = fptr.report();
            log.WriteToLog("XReport() => " + result.ToString());
            CheckDocumentClosed();
            return result;
        }

        public int ZReport()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_REPORT_TYPE, Constants.LIBFPTR_RT_CLOSE_SHIFT);
            int result = fptr.report();
            log.WriteToLog("ZReport() => " + result.ToString());
            CheckDocumentClosed();
            return result;
        }

        public int Print(String text)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_TEXT, text);
            int result = fptr.printText();
            log.WriteToLog("Print() => " + result.ToString() + " text=" + text);
            return result;
        }

        public int CancelCheque()
        {
            int result = fptr.cancelReceipt();
            log.WriteToLog("fptr.cancelReceipt() => " + result.ToString());
            return result;
        }

        public int CloseCheque()
        {
            int result = fptr.closeReceipt();
            log.WriteToLog("fptr.cancelReceipt() => " + result.ToString());
            CheckDocumentClosed();
            return result;
        }

        public int BeginNonFiscalDoc()
        {
            int result = fptr.beginNonfiscalDocument();
            log.WriteToLog("fptr.beginNonfiscalDocument() => " + result.ToString());
            return result;
        }

        public int EndNonFiscalDoc()
        {
            int result = fptr.endNonfiscalDocument();
            log.WriteToLog("fptr.endNonfiscalDocument() => " + result.ToString());
            return result;
        }

        public int Income(double value)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_SUM, value);
            int result = fptr.cashIncome();
            log.WriteToLog("fptr.cashIncome(): value=" + value.ToString() + " => " + result.ToString());
            return result;
        }

        public int Outcome(double value)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_SUM, value);
            int result = fptr.cashOutcome();
            log.WriteToLog("fptr.cashOutcome(): value=" + value.ToString() + " => " + result.ToString());
            return result;
        }

        public void SetParam(int paramId, uint value)
        {
            fptr.setParam(paramId, value);
            log.WriteToLog("fptr.setParam(): paramId=" + paramId.ToString() + " value=" + value.ToString());
        }

        public void SetParam(int paramId, String value)
        {
            fptr.setParam(paramId, value);
            log.WriteToLog("fptr.setParam(): paramId=" + paramId.ToString() + " value=" + value);
        }

        public void SetParam(int paramId, bool value)
        {
            fptr.setParam(paramId, value);
            log.WriteToLog("fptr.setParam(): paramId=" + paramId.ToString() + " value=" + value.ToString());
        }

        public void SetParam(int paramId, double value)
        {
            fptr.setParam(paramId, value);
            log.WriteToLog("fptr.setParam(): paramId=" + paramId.ToString() + " value=" + value.ToString());
        }

        public void SetParam(int paramId, byte[] value)
        {
            fptr.setParam(paramId, value);
            
            log.WriteToLog("fptr.setParam(): paramId=" + paramId.ToString() + " value=" + value.Length.ToString());
        }

        public int UtilFormTlv()
        {
            return fptr.utilFormTlv();
        }

        public byte[] GetParamByteArray(int value)
        {
            return fptr.getParamByteArray(value);
        }

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

    }
}

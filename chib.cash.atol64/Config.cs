using System;
using IniParser;
using shared;


namespace chib.cash.atol64
{
    public class Config
    {
        Log log;
        readonly String iniFile;
        IniParser.Model.IniData config;

        public String UseLog { get; private set; }

        public String RegInfoLog { get; private set; }

        public String PortName { get; private set; }

        public String IpAddress { get; private set; }

        public String TCPPort { get; private set; }

        public int CashierPassword { get; private set; }

        public int AdministratorPassword { get; private set; }

        public int MaxCashStringLength { get; private set; }

        public uint TaxTypeNumber { get; private set; }

        public int CashlessTypeClose { get; private set; }

        public String AutoShift { get; private set; }

        public int LinesBeforeCut { get; private set; }

        public int Tag1057 { get; private set; }


        public Config(String iniFile)
        {
            this.iniFile = iniFile;
            FileIniDataParser parser = new FileIniDataParser();
            config = parser.ReadFile(iniFile); // "chib.cash.atol64.ini"
            ReadIniFile();
        }

        public Log GetLog()
        {
            return log;
        }

        /// <summary>
        /// Прочитать строковый параметр из ini-файла.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private String GetIniParamString(String paramName, String defaultValue)
        {
            String note = "";
            String value = config.GetKey(paramName);
            if (String.IsNullOrEmpty(value))
            {
                value = defaultValue;
                note = " (по умолчанию)";
            }
            log.WriteToLog(paramName + "=" + value + note);
            return value;
        }

        /// <summary>
        /// Прочитать int параметр из ini-файла.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private int GetIniParamInt(String paramName, int defaultValue)
        {
            String note = "";
            String value = config.GetKey(paramName);
            int intValue;
            if (String.IsNullOrEmpty(value))
            {
                intValue = defaultValue;
                note = " (по умолчанию)";
            }
            else
            {
                intValue = int.Parse(value);
            }
            log.WriteToLog(paramName + "=" + intValue.ToString() + note);
            return intValue;
        }

        private void ReadIniFile()
        {
            String logPath = config.GetKey("LogPath");
            log = new Log(logPath);
            log.WriteToLog("### Чтение параметров из " + this.iniFile + " ###");
            log.WriteToLog("LogPath=" + logPath);

            UseLog = GetIniParamString("UseLog", "D");
            RegInfoLog = GetIniParamString("RegInfoLog", "D");
            PortName = GetIniParamString("PortName", "COM1");
            IpAddress = GetIniParamString("IPAddress", "");
            TCPPort = GetIniParamString("TCPPort", "5555");
            CashierPassword = GetIniParamInt("CashierPassword", 1);
            AdministratorPassword = GetIniParamInt("AdministratorPassword", 30);
            MaxCashStringLength = GetIniParamInt("MaxCashStringLength", 48);
            TaxTypeNumber = (uint)GetIniParamInt("TaxTypeNumber", 6);
            CashlessTypeClose = GetIniParamInt("CashlessTypeClose", 1);
            AutoShift = GetIniParamString("AutoShift", "Y");
            LinesBeforeCut = GetIniParamInt("LinesBeforeCut", 4);
            Tag1057 = GetIniParamInt("Tag1057", 0);
        }

    }
}

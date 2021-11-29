using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atol.Drivers10.Fptr;

namespace chib.cash.atol64
{
    public class KKT
    {
        String nameKKT;
        Boolean isOpened;

        // 
        IFptr fptr;

        public KKT() {}

        public void Initialize()
        {
            fptr = new Fptr();
        }

        public void Deinitialize()
        {
            fptr.destroy();
        }

        public void Settings()
        {
            // можно так:
            /*
            String settings = String.Format("{\"{0}\": {1}, \"{2}\": {3}, \"{4}\": \"{5}\", \"{6}\": {7}}",
                Constants.LIBFPTR_SETTING_MODEL, Constants.LIBFPTR_MODEL_ATOL_AUTO,
                Constants.LIBFPTR_SETTING_PORT, Constants.LIBFPTR_PORT_COM,
                Constants.LIBFPTR_SETTING_COM_FILE, "COM5",
                Constants.LIBFPTR_SETTING_BAUDRATE, Constants.LIBFPTR_PORT_BR_115200);
            fptr.setSettings(settings);
            */

            // можно так:
            fptr.setSingleSetting(Constants.LIBFPTR_SETTING_MODEL, Constants.LIBFPTR_MODEL_ATOL_AUTO.ToString());
            fptr.setSingleSetting(Constants.LIBFPTR_SETTING_PORT, Constants.LIBFPTR_PORT_COM.ToString());
            fptr.setSingleSetting(Constants.LIBFPTR_SETTING_COM_FILE, "COM5");
            fptr.setSingleSetting(Constants.LIBFPTR_SETTING_BAUDRATE, Constants.LIBFPTR_PORT_BR_115200.ToString());
            fptr.applySingleSettings();
        }

        public bool Open()
        {
            fptr.open();

            isOpened = fptr.isOpened();
            Console.WriteLine("Opened? " + isOpened.ToString());

            // Запрос версии драйвера
            String version = fptr.version();
            Console.WriteLine(version);

            return isOpened;
        }

        public void Close()
        {
            fptr.close();
        }

        public int GetShiftState()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_DATA_TYPE, Constants.LIBFPTR_DT_STATUS);
            fptr.queryData();
            uint shiftState = fptr.getParamInt(Constants.LIBFPTR_PARAM_SHIFT_STATE);

            return (int)shiftState;
        }

        public int OpenShift()
        {
            return fptr.openShift();
        }

        public void X_Report()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_REPORT_TYPE, Constants.LIBFPTR_RT_X);
            fptr.report();
        }

        public void Z_Report()
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_REPORT_TYPE, Constants.LIBFPTR_RT_CLOSE_SHIFT);
            fptr.report();
        }

        public void Print(String text)
        {
            fptr.setParam(Constants.LIBFPTR_PARAM_TEXT, text);
            fptr.printText();
        }
    }
}

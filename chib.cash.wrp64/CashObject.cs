using System;
using System.Reflection;
using System.IO;

using shared;

namespace chib.cash.wrp64
{
    public class CashObject
    {
        Log log = new Log(@"c:\gorod\chelyabinvestbank");

        readonly String driverName;
        readonly String driverPath;
        readonly Object cash;
        readonly Type cashType;

        public CashObject(String driverNameExt)
        {
            this.driverName = Path.GetFileNameWithoutExtension(driverNameExt);
            this.driverPath = Directory.GetCurrentDirectory() + @"\" + driverNameExt;

            log.WriteToLog(this.driverName);
            log.WriteToLog(this.driverPath);

            // Получить объект Cash драйвера driverName
            Assembly assembly = Assembly.LoadFile(this.driverPath);
            this.cashType = assembly.GetType(this.driverName + ".Cash", true, true);
            //this.cashType = assembly.GetType(this.driverName + ".Instance", true, true);
            //this.cash = Activator.CreateInstance(this.cashType);

            MethodInfo executeMethod = this.cashType.GetMethod("Instance");
            log.WriteToLog(executeMethod.Name);
            try
            {
                this.cash = executeMethod.Invoke(null, null);
            }
            catch (Exception ex)
            {
                log.WriteToLog(Log.Level.Error, ex.Message);
            }
        }

        public long ExecuteMethod(String methodName, object args)
        {
            long result = -1;
            MethodInfo executeMethod = this.cashType.GetMethod(methodName);
            if (args != null)
            {
                object[] parametersArray = new object[] { args };
                result = (long)executeMethod.Invoke(this.cash, parametersArray);
            }
            else
            {
                result = (long)executeMethod.Invoke(this.cash, null);
            }
            return result;
        }
    }
}

using System;
using System.Reflection;
using System.IO;

namespace chib.cash.wrp64
{
    public class CashObject
    {
        readonly String driverName;
        readonly String driverPath;
        readonly Object cash;
        readonly Type cashType;

        public CashObject(String driverName)
        {
            this.driverName = driverName;
            this.driverPath = Directory.GetCurrentDirectory() + @"\" + driverName + ".dll";

            // Получить объект Cash драйвера driverName
            Assembly assembly = Assembly.LoadFile(this.driverPath);
            this.cashType = assembly.GetType(this.driverName + ".Cash", true, true);
            //this.cashType = assembly.GetType(this.driverName + ".Instance", true, true);
            //this.cash = Activator.CreateInstance(this.cashType);

            MethodInfo executeMethod = this.cashType.GetMethod("Instance");
            this.cash = executeMethod.Invoke(null, null);
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

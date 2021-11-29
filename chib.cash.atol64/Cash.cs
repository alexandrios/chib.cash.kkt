using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using shared;
using Atol.Drivers10.Fptr;

namespace chib.cash.atol64
{
    public class Cash : ICash
    {
        //public Cash() {}

        private void Test()
        {
            ICash c = new Cash();
            c.End();
        }

        public long CancelCheque()
        {
            throw new NotImplementedException();
        }

        public long CloseCheque(uint income, PaymentType type = PaymentType.CASH)
        {
            throw new NotImplementedException();
        }

        public void End()
        {
            throw new NotImplementedException();
        }

        public ChequeType GetChequeType()
        {
            throw new NotImplementedException();
        }

        public string GetInfoKKT()
        {
            throw new NotImplementedException();
        }

        public uint GetMaxCashStringLength()
        {
            throw new NotImplementedException();
        }

        public void Income(string sum)
        {
            throw new NotImplementedException();
        }

        public long IsSessionOpen()
        {
            KKT kkt = new KKT();
            kkt.Initialize();
            kkt.Settings();
            kkt.Open();

            long shiftState = (long)kkt.GetShiftState();
            if (shiftState == Constants.LIBFPTR_SS_CLOSED)
            {
                kkt.OpenShift();
            }


            kkt.X_Report();
            kkt.Close();
            kkt.Deinitialize();

            return shiftState;
        }

        public long IsSessionOver()
        {
            throw new NotImplementedException();
        }

        public long NewCheque(ChequeType type)
        {
            throw new NotImplementedException();
        }

        public long OpenSession()
        {
            throw new NotImplementedException();
        }

        public void OpenTreasure()
        {
            throw new NotImplementedException();
        }

        public void Outcome(string sum)
        {
            throw new NotImplementedException();
        }

        public long Print(String text)
        {
            return 0;
        }
        public long Printer(List<String> texts)
        //public long Printer(params object[] texts)
        //public long Printer(String a, String b)
        {
            //foreach (String line in texts)
            //{
            //    Console.WriteLine("atol: " + line);
            //}
                        
            KKT kkt = new KKT();
            kkt.Initialize();
            kkt.Settings();
            kkt.Open();

            Log log = new Log(@"c:\gorod\chelyabinvestbank");
            log.WriteToLog("Print:");
            foreach (String line in texts)
            {
                log.WriteToLog(line);
                kkt.Print(line);
            }

            //kkt.Print(a);
            //kkt.Print(b);

            //kkt.X_Report();
            //kkt.Z_Report();
            kkt.Close();
            kkt.Deinitialize();

            return 0;
        }

        public long RegisterSale(Dictionary<string, string> regParams)
        {
            throw new NotImplementedException();
        }

        public long Return(Dictionary<string, string> returnParams)
        {
            throw new NotImplementedException();
        }

        public void SetCashierInn(string cashierInn)
        {
            throw new NotImplementedException();
        }

        public void SetIsPreCheck(bool isPreCheck)
        {
            throw new NotImplementedException();
        }

        public void SetParamsMap(Dictionary<string, string> chequeParams)
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

        public void XReport()
        {
            throw new NotImplementedException();
        }

        public void ZReport()
        {
            throw new NotImplementedException();
        }
    }
}

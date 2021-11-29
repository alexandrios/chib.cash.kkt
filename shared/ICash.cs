using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public enum ChequeType
    {
        SALE = 1, 
        RETURN
    }

    public enum PaymentType
    {
        CASH = 1,
        CREDIT,
        TARE,
        CARD
    }

    interface ICash
    {
        ChequeType GetChequeType();

        long NewCheque(ChequeType type);

        void Outcome(String sum);
        void Income(String sum);

        long RegisterSale(Dictionary<String, String> regParams);
        /*
                const std::string& text,
				unsigned int sum,
                unsigned int quantity = 1000,
                unsigned int section = 0,
                unsigned int tax = 0) = 0;
                */
        long Return(Dictionary<String, String> returnParams);
        /*
                const std::string& text,
				unsigned int sum,
                unsigned int quantity = 1000) = 0;
                */
        long CancelCheque();

        long CloseCheque(uint income, PaymentType type = PaymentType.CASH);

        long Print(String text);

        long OpenSession();

        long IsSessionOpen();
        long IsSessionOver();

        void XReport();
        void ZReport();

        uint GetMaxCashStringLength();

        void OpenTreasure();

        void End();

        /*
		 void SetFz54Params(int isCashlessCheck, int isECheck, int bankComiss,

                const std::string&  clientPhone, const std::string&  clientEmail, 
				const std::string&  service, const std::string&  cashier, 
				const std::string&  agentPhone, const std::string&  operatorOper, 
				const std::string& operatorName, const std::string& operatorAddress, 
				const std::string& operatorInn, const std::string& operatorPhone) = 0;
                */
        void SetTaxTypeNumber(int taxTypeNumber);
        void SetTypeOplNumber(int typeOplNumber);

        void SetIsPreCheck(bool isPreCheck);

        void SetCashierInn(String cashierInn);
        String GetInfoKKT();

        void SetParamsMap(Dictionary<String, String> chequeParams);

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    // Тип чека (LIBFPTR_PARAM_RECEIPT_TYPE) может принимать следующие значения:
    /*
	LIBFPTR_RT_SELL - чек прихода (продажи)
	LIBFPTR_RT_SELL_RETURN - чек возврата прихода (продажи)
	LIBFPTR_RT_SELL_CORRECTION - чек коррекции прихода
	LIBFPTR_RT_SELL_RETURN_CORRECTION - чек коррекции возврата прихода
	LIBFPTR_RT_BUY - чек расхода (покупки)
	LIBFPTR_RT_BUY_RETURN - чек возврата расхода (покупки)
	LIBFPTR_RT_BUY_CORRECTION - чек коррекции расхода
	LIBFPTR_RT_BUY_RETURN_CORRECTION - чек коррекции возврата расхода
	*/
    public enum ChequeType
    {
        SELL = 1, 
        SELL_RETURN
    }

    // Тип оплаты
    /*
    LIBFPTR_PT_CASH = 0;
    LIBFPTR_PT_ELECTRONICALLY = 1;
    LIBFPTR_PT_PREPAID = 2;
    LIBFPTR_PT_CREDIT = 3;
    LIBFPTR_PT_OTHER = 4;
    */
    public enum PaymentType  // (Пока не используется)
    {
        CASH = 0,
        ELECTRONICALLY,
        PREPAID,
        CREDIT,
        OTHER
    }

    interface ICash
    {
        long SetInputParams(Dictionary<String, String> inputParams);

        long IsSessionOpen();
        long IsSessionOver();
        long OpenSession();

        long NewCheque(ChequeType type);

        long RegisterSale(Dictionary<String, String> regSaleParams);

        long CloseCheque(String income);
        long CancelCheque();

        long Print(String text);

        long Income(String sum);
        long Outcome(String sum);

        long XReport();
        long ZReport();

        void End();
        String GetInfoKKT();

        uint GetMaxCashStringLength();
        void SetTaxTypeNumber(int taxTypeNumber);
        void SetTypeOplNumber(int typeOplNumber);
        void SetIsPreCheck(bool isPreCheck);
        void SetCashierInn(String cashierInn);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public static class Attributes
    {
        // Наименования параметров, передаваемых из АРМ в драйвер
        public const String BANK_COMISS = "BANK_COMISS";
        public const String IS_PRECHEQUE = "IS_PRECHEQUE";
        public const String IS_CASHLESSCHEQUE = "IS_CASHLESSCHEQUE";
        public const String IS_ECHEQUE = "IS_ECHEQUE";
        public const String CLIENT_PHONE = "CLIENT_PHONE";
        public const String CLIENT_EMAIL = "CLIENT_EMAIL";
        public const String SERVICE_NAME = "SERVICE_NAME";
        public const String CASHIER_NAME = "CASHIER_NAME";
        public const String CASHIER_INN = "CASHIER_INN";
        public const String AGENT_PHONE = "AGENT_PHONE";
        public const String OPERATOR_OPER = "OPERATOR_OPER";
        public const String OPERATOR_NAME = "OPERATOR_NAME";
        public const String OPERATOR_ADDRESS = "OPERATOR_ADDRESS";
        public const String OPERATOR_INN = "OPERATOR_INN";
        public const String OPERATOR_PHONE = "OPERATOR_PHONE";
        public const String PU_PHONE = "PU_PHONE";
        public const String PU_NAME = "PU_NAME";
        public const String PU_INN = "PU_INN";


        // Наименования параметров, получаемых из шаблона чека
        public const String TEMPL_SRVTEXT = "TEMPL_SRVTEXT";
        public const String TEMPL_PRICE = "TEMPL_PRICE";
        public const String TEMPL_QUANTITY = "TEMPL_QUANTITY";
        public const String TEMPL_SECTION = "TEMPL_SECTION";
        public const String TEMPL_TAX = "TEMPL_TAX";
    }
}

using System;
using System.Collections.Generic;
using shared;

namespace shared
{
    class InputParams
    {
        Log log;

        public bool IsCashlessCheque { get; private set; }

        public bool IsECheque { get; private set; }

        public bool IsPreCheque { get; private set; }

        public int BankComiss { get; private set; }

        public String ClientPhone { get; private set; }

        public String ClientEmail { get; private set; }

        public String ServiceName { get; private set; }

        public String CashierName { get; private set; }

        public String CashierInn { get; private set; }

        public String AgentPhone { get; private set; }

        public String OperatorOper { get; private set; }

        public String OperatorName { get; private set; }

        public String OperatorAddress { get; private set; }

        public String OperatorInn { get; private set; }

        public String OperatorPhone { get; private set; }

        public String PuPhone { get; private set; }

        public String PuName { get; private set; }

        public String PuInn { get; private set; }


        public InputParams(Log log)
        {
            this.log = log;
        }

        public void SetInputParams(Dictionary<String, String> inputParams)
        {
            Dictionary<String, bool> isReceived = new Dictionary<String, bool>
            {
                [Attributes.BANK_COMISS] = false,
                [Attributes.IS_PRECHEQUE] = false,
                [Attributes.IS_CASHLESSCHEQUE] = false,
                [Attributes.IS_ECHEQUE] = false,
                [Attributes.CLIENT_PHONE] = false,
                [Attributes.CLIENT_EMAIL] = false,
                [Attributes.SERVICE_NAME] = false,
                [Attributes.CASHIER_NAME] = false,
                [Attributes.CASHIER_INN] = false,
                [Attributes.AGENT_PHONE] = false,
                [Attributes.OPERATOR_OPER] = false,
                [Attributes.OPERATOR_NAME] = false,
                [Attributes.OPERATOR_ADDRESS] = false,
                [Attributes.OPERATOR_INN] = false,
                [Attributes.OPERATOR_PHONE] = false,
                [Attributes.PU_PHONE] = false,
                [Attributes.PU_NAME] = false,
                [Attributes.PU_INN] = false
            };

            log.WriteToLog("### Прием внешних параметров (из АРМ) ###");

            foreach (KeyValuePair<String, String> pair in inputParams)
            {
                log.WriteToLog("Получен параметр: " + pair.Key + "=" + pair.Value);

                switch (pair.Key)
                {
                    case Attributes.BANK_COMISS:
                        {
                            BankComiss = int.Parse(pair.Value);
                            isReceived[Attributes.BANK_COMISS] = true;
                            break;
                        }
                    case Attributes.IS_PRECHEQUE:
                        {
                            IsPreCheque = pair.Value == "1";
                            isReceived[Attributes.IS_PRECHEQUE] = true;
                            break;
                        }
                    case Attributes.IS_CASHLESSCHEQUE:
                        {
                            IsCashlessCheque = pair.Value == "1";
                            isReceived[Attributes.IS_CASHLESSCHEQUE] = true;
                            break;
                        }
                    case Attributes.IS_ECHEQUE:
                        {
                            IsECheque = pair.Value == "1";
                            isReceived[Attributes.IS_ECHEQUE] = true;
                            break;
                        }
                    case Attributes.CLIENT_PHONE:
                        {
                            ClientPhone = pair.Value;
                            isReceived[Attributes.CLIENT_PHONE] = true;
                            break;
                        }
                    case Attributes.CLIENT_EMAIL:
                        {
                            ClientEmail = pair.Value;
                            isReceived[Attributes.CLIENT_EMAIL] = true;
                            break;
                        }
                    case Attributes.SERVICE_NAME:
                        {
                            ServiceName = pair.Value;
                            isReceived[Attributes.SERVICE_NAME] = true;
                            break;
                        }
                    case Attributes.CASHIER_NAME:
                        {
                            CashierName = pair.Value;
                            isReceived[Attributes.CASHIER_NAME] = true;
                            break;
                        }
                    case Attributes.CASHIER_INN:
                        {
                            CashierInn = pair.Value;
                            isReceived[Attributes.CASHIER_INN] = true;
                            break;
                        }
                    case Attributes.AGENT_PHONE:
                        {
                            AgentPhone = pair.Value;
                            isReceived[Attributes.AGENT_PHONE] = true;
                            break;
                        }
                    case Attributes.OPERATOR_OPER:
                        {
                            OperatorOper = pair.Value;
                            isReceived[Attributes.OPERATOR_OPER] = true;
                            break;
                        }
                    case Attributes.OPERATOR_NAME:
                        {
                            OperatorName = pair.Value;
                            isReceived[Attributes.OPERATOR_NAME] = true;
                            break;
                        }
                    case Attributes.OPERATOR_ADDRESS:
                        {
                            OperatorAddress = pair.Value;
                            isReceived[Attributes.OPERATOR_ADDRESS] = true;
                            break;
                        }
                    case Attributes.OPERATOR_INN:
                        {
                            OperatorInn = pair.Value;
                            isReceived[Attributes.OPERATOR_INN] = true;
                            break;
                        }
                    case Attributes.OPERATOR_PHONE:
                        {
                            OperatorPhone = pair.Value;
                            isReceived[Attributes.OPERATOR_PHONE] = true;
                            break;
                        }
                    case Attributes.PU_PHONE:
                        {
                            PuPhone = pair.Value;
                            isReceived[Attributes.PU_PHONE] = true;
                            break;
                        }
                    case Attributes.PU_NAME:
                        {
                            PuName = pair.Value;
                            isReceived[Attributes.PU_NAME] = true;
                            break;
                        }
                    case Attributes.PU_INN:
                        {
                            PuInn = pair.Value;
                            isReceived[Attributes.PU_INN] = true;
                            break;
                        }
                    default:
                        {
                            log.WriteToLog("Параметр не используется: " + pair.Key + "=" + pair.Value);
                            break;
                        }
                }
            }

            foreach (KeyValuePair<String, bool> pair in isReceived)
            {
                if (!pair.Value)
                    log.WriteToLog("Не получен ожидаемый параметр: " + pair.Key);
            }
        }
    }
}

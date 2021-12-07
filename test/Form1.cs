using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using shared;

namespace Test
{
    public partial class Form1 : Form
    {
        Type wrapperType;
        object wrapper;

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            String wrapperPath = Directory.GetCurrentDirectory() + @"\chib.cash.wrp64.dll";
            Assembly assembly = Assembly.LoadFile(wrapperPath);
            foreach (var t in assembly.GetTypes())
            {
                Console.WriteLine(t.FullName);
            }

            wrapperType = assembly.GetType("chib.cash.wrp64.Wrapper", true, true);
            
            // Если статические методы у объекта, то можно не создавать instance
            wrapper = Activator.CreateInstance(wrapperType);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //String wrapperPath = Directory.GetCurrentDirectory() + @"\chib.cash.wrp64.dll";

            try
            {
                //Assembly assembly = Assembly.LoadFile(wrapperPath);

                /*
                String chequeTemplate = @"SEPARATE=NOLOGENABLE=YESPRINTDOCLIST=&2
                ~hdr: кассовый чек
                ~hdr: ========================================
                ~hdr: Операция: 1341643248
                ~hdr: Дата: 06 / 04 / 2021 08:17:22
                ~hdr: Кассир: ОПЛАТА БАНКОВСКИМИ КАРТАМИ ЧЕРЕЗ МОБИЛЬНОЕ ПРИЛОЖЕНИЕ СИСТЕМА ГОРОД
                ~hdr: Сумма: 1589.10
                ~hdr: Одна тысяча пятьсот восемьдесят девять рублей 10 копеек
                ~hdr: "ФИО: П.Р.В."
                ~hdr: Л / с: 6635700762
                ~hdr: Адрес: ЖУКОВА УЛ, 10, 145
                ~hdr: ----------------------------------------
                ~hdr:1 - Обсл - е и ремонт:               1037.88
                ~hdr:2 - Лифт:                           320.62
                ~hdr:4 - ВДГО:                            50.44
                ~hdr:5 - ГВС при СОИ:                     47.04
                ~hdr:6 - ХВС при СОИ:                      9.64
                ~hdr:7 - ВО при СОИ:                      17.29
                ~hdr:8 - Эл / эн при СОИ:                  106.19
                ~hdr:----------------------------------------
                ~hdr: Получатель: ООО УК ПРАВЫЙ БЕРЕГ Г.МАГНИТОГОРСК
                ~hdr: ИНН: 7455033958 БИК: 047501779 Р / c: 40702810290200004598
                ~hdr: ----------------------------------------
                ~hdr: Назначение перевода: МАГНИТ - СК ООО УК ПРАВЫЙ БЕРЕГ: ЖИЛИЩНЫЕ УСЛУГИ
                ~hdr: ----------------------------------------
                ~hdr: Подпись плательщика:
                ~hdr: ----------------------------------------
                ~hdr: Адрес пункта оказания услуг: ЧЕЛЯБИНСК, РЕВОЛЮЦИИ ПЛ, 8  Оказание услуг по переводу осуществляет: ИНН 7421000200 МОБИЛЬНОЕ ПРИЛОЖЕНИЕ  СИСТЕМА ГОРОД  ЧЕЛЯБИНСК, РЕВОЛЮЦИИ ПЛ, 8 Тел.(351) 268 - 00 - 88  На основании договора: ПАО ЧЕЛЯБИНВЕСТБАНК
                ~srv 1   1589.10
                ~pay 1   1589.10
                ~pos: Обсл - е и ремонт^ 1037.88 ^ 1102 | Лифт ^ 320.62 ^ 1102 | ВДГО ^ 50.44 ^ 1102 | ГВС при СОИ^ 47.04 ^ 1102 | ХВС при СОИ^ 9.64 ^ 1102 | ВО при СОИ^ 17.29 ^ 1102 | Эл / эн при СОИ^ 106.19 ^ 1102";
                */

                String chequeTemplate = "";
                using (StreamReader sr = new StreamReader("cheque.txt", System.Text.Encoding.Default))
                {
                    chequeTemplate = sr.ReadToEnd();
                }

                Dictionary<String, String> inputParams = new Dictionary<String, String>
                {
                    [Attributes.BANK_COMISS] = "0",
                    [Attributes.IS_PRECHEQUE] = "0", // 1
                    [Attributes.IS_CASHLESSCHEQUE] = "0",
                    [Attributes.IS_ECHEQUE] = "0",
                    [Attributes.CLIENT_PHONE] = "",
                    [Attributes.CLIENT_EMAIL] = "no-reply@chelinvest.ru",
                    [Attributes.SERVICE_NAME] = "",
                    [Attributes.CASHIER_NAME] = "Красин Иван Аронович",
                    [Attributes.CASHIER_INN] = "745003661891",
                    [Attributes.AGENT_PHONE] = "",
                    [Attributes.OPERATOR_OPER] = "",
                    [Attributes.OPERATOR_NAME] = "",
                    [Attributes.OPERATOR_ADDRESS] = "",
                    [Attributes.OPERATOR_INN] = "",
                    [Attributes.OPERATOR_PHONE] = "8-351-774359",
                    [Attributes.PU_PHONE] = "83512234567",
                    [Attributes.PU_NAME] = "АО Энерго",
                    [Attributes.PU_INN] = "7450567890"
                };

                //Type wrapperType = assembly.GetType("chib.cash.wrp64.Wrapper", true, true);

                // Если статические методы у объекта, то можно не создавать instance
                //object wrapper = Activator.CreateInstance(wrapperType);

                MethodInfo executeMethod = wrapperType.GetMethod("PrintDocument");
                object[] parametersArray = new object[] { chequeTemplate, inputParams };

                // Если метод статический, то первый параметр (объект) игнорируется
                //executeMethod.Invoke(null, parametersArray);

                // Если не статический, то нужно указать instance
                executeMethod.Invoke(wrapper, parametersArray);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void _XReportButton_Click(object sender, EventArgs e)
        {
            try
            {
                MethodInfo executeMethod = wrapperType.GetMethod("X_Report");
                executeMethod.Invoke(wrapper, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void _ZReportButton_Click(object sender, EventArgs e)
        {
            try
            {
                MethodInfo executeMethod = wrapperType.GetMethod("Z_Report");
                executeMethod.Invoke(wrapper, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            TestToDouble("0.34");
            TestToDouble("1.05");
            TestToDouble("1.79");
            TestToDouble("1.01");
            TestToDouble("12.49");
            TestToDouble("15.51");
            TestToDouble("12.99");
            TestToDouble("123451.08");
            TestToDouble("15200234.99");
            TestToDouble("-15.99");
            TestToDouble("-2");
            TestToDouble("99");
            TestToDouble("-0");
            TestToDouble("-15.99999");
            TestToDouble("2.123456789");
            TestToDouble("99.9999999");
            TestToDouble("-99.9999999");
            TestToDouble("99.99");
            TestToDouble("-99.99");

        }

        private void TestToDouble(String value)
        {
            Console.WriteLine(Utils.ToDoubleSumma(value).ToString());
        }

        private void _incomeButton_Click(object sender, EventArgs e)
        {
            try
            {
                MethodInfo executeMethod = wrapperType.GetMethod("MoneyIn");
                object[] parametersArray = new object[] { "17.98" };
                executeMethod.Invoke(wrapper, parametersArray);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void _outcomeButton_Click(object sender, EventArgs e)
        {
            try
            {
                MethodInfo executeMethod = wrapperType.GetMethod("MoneyOut");
                object[] parametersArray = new object[] { "4.79" };
                executeMethod.Invoke(wrapper, parametersArray);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

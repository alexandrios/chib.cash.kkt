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

namespace Test
{
    public partial class Form1 : Form
    {
        //[DllImport("ccwrp.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "Execute")]
        //static extern void Execute();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Execute();

            //var assemblyBytes = System.IO.File.ReadAllBytes("myassembly.dll");
            //var assembly = System.Reflection.Assembly.Load(assemblyBytes);

            String wrapperPath = Directory.GetCurrentDirectory() + @"\chib.cash.wrp64.dll";

            try
            {
                Assembly assembly = Assembly.LoadFile(wrapperPath);
                foreach (var t in assembly.GetTypes())
                {
                    Console.WriteLine(t.FullName);
                }

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

                Type wrapperType = assembly.GetType("chib.cash.wrp64.Wrapper", true, true);

                // Если статические методы у объекта, то можно не создавать instance
                object wrapper = Activator.CreateInstance(wrapperType);

                MethodInfo executeMethod = wrapperType.GetMethod("Start");
                object[] parametersArray = new object[] { chequeTemplate };

                // Если метод статический, то первый параметр (объект) игнорируется
                //executeMethod.Invoke(null, parametersArray);

                // Если не статический, то нужно указать instance
                executeMethod.Invoke(wrapper, parametersArray);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + wrapperPath);
            }

        }
    }
}

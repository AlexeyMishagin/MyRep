using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBF_Work;

namespace BIC_Test_work
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            BICView view = new BICView();
            Message message = new Message();
            DBFManager dbf = new DBFManager();

            BICPresenter presenter = new BICPresenter(view, dbf, message);

            Application.Run(view);
        }
    }
}

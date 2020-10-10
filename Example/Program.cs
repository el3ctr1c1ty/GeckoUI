using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeckoUI;

namespace GeckoUI.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            var form = new GeckoForm { ClickIndeficator = "app:btn", EnableBorder = false, EnableEffects = false, EnableShadow = false,RoundRadius = 10, MoveYLimit = 200 };

            form.ClearCache(); form.ClearCookies();

            form.LoadPage();
            Application.Run(form);
        }
    }
}

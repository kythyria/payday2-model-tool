//Original code by PoueT

using PD2ModelParser.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PD2ModelParser
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Updates.Startup();

            Form1 form = new Form1();
            Application.Run(form);

        }

    }
}

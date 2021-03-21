using System;
using System.Reflection;
using System.Windows.Forms;

namespace PD2ModelParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            var assemblyProduct = assembly.GetCustomAttribute<AssemblyProductAttribute>() as AssemblyProductAttribute;
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var version = informationalVersion?.InformationalVersion ?? "BUG: AssemblyInformationalVersionAttribute missing!";
            Text = $"{assemblyProduct.Product} (cpone-fuck-you-edition-or-something)";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}

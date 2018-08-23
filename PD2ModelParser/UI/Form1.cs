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
            AssemblyProductAttribute assemblyProduct = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0] as AssemblyProductAttribute;
            Text = assemblyProduct.Product;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}

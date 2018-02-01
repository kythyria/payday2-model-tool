using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PD2ModelParser
{
    public partial class Form1 : Form
    {
        public static FileManager fm = new FileManager();

        public Form1()
        {
            this.InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        // Export... button
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.textBox2.Text.Length == 0)
            {
                MessageBox.Show("Please enter an export file name.", "Error");
                return;
            }
            if (Form1.fm.GenerateNewModel(this.textBox2.Text))
            {
                MessageBox.Show("Model generated successfully");
                return;
            }
            MessageBox.Show("There was an error generating your model");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (NewObjImporter.ImportNewObj(fm, openFileDialog.FileName, this.addNewObjects_checkbox.Checked))
                {
                    MessageBox.Show("OBJ imported successfully");
                    return;
                }
                MessageBox.Show("Theere was an error importing OBJ");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (Form1.fm.ImportNewObjPatternUV(openFileDialog.FileName))
                {
                    MessageBox.Show("Pattern UV imported successfully");
                    return;
                }
                MessageBox.Show("Theere was an error importing OBJ");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirstWinApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Thread.Sleep(5000);

            // dataGridView1.DataSource = new List<string>() { "Du" };

            new Thread(() =>
            {
                Thread.Sleep(2000);
                dataGridView1.Invoke(new MethodInvoker(() =>
                {
                    dataGridView1.DataSource = new List<string>() { "Du" };
                }));
            }).Start();

        }
    }
}

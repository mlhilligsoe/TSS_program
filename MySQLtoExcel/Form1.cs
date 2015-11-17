using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.IO;

namespace Mysql_to_Excel
{
    public partial class Form1 : Form
    {
        string connectionString = "Server=127.0.0.1;Database=bosch;Uid=bosch;Pwd=12345;SslMode=None;Convert Zero Datetime=True;";
        string query;

        public Form1()
        {
            InitializeComponent();
            try

            {
                MySqlConnection conn = new MySqlConnection("Server=127.0.0.1;Database=bosch;Uid=bosch;Pwd=12345;SslMode=None;");
                MessageBox.Show("connected with sql server");
            }
            catch (Exception es)
            {
                MessageBox.Show(es.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            query = "SELECT ";

            query += "`batch_id`";


            if (checkBox2.Checked == true)
            {
                query += ", `batch_order_id`";
            }

            if (checkBox3.Checked == true)
            {
                query += ", `batch_storage`";
            }

            if (checkBox4.Checked == true)
            {
                query += ", `batch_quantity`";
            }

            if (checkBox5.Checked == true)
            {
                query += ", `batch_start`";
            }

            if (checkBox6.Checked == true)
            {
                query += ", `batch_complete`";
            }

            if (checkBox7.Checked == true)
            {
                query += ", `batch_n_events`";
            }

            if (checkBox8.Checked == true)
            {
                query += ", `batch_comment`";
            }

            query += " FROM `batches` ";
            //query = "SELECT `batch_id`, `batch_order_id`, `batch_storage`, `batch_quantity`, `batch_start`, `batch_complete`, `batch_n_events`, `batch_comment` FROM `batches` ";




            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {

                using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    dataGridView1.DataSource = ds.Tables[0];
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

            string strValue = string.Empty;


            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)

            {

                for (int j = 0; j < dataGridView1.Rows[i].Cells.Count; j++)

                {

                    if (!string.IsNullOrEmpty(dataGridView1[j, i].Value.ToString()))

                    {

                        if (j > 0)

                            strValue = strValue + ";" + dataGridView1[j, i].Value.ToString();

                        else

                        {

                            if (string.IsNullOrEmpty(strValue))

                                strValue = dataGridView1[j, i].Value.ToString();

                            else

                                strValue = strValue + Environment.NewLine + dataGridView1[j, i].Value.ToString();

                        }

                    }

                }

                // strValue = strValue + Environment.NewLine;

            }

            string strFile = @"C:\Users\amminex\Desktop\" + textBox1.Text + ".csv";
            MessageBox.Show("your file have been saved");


            if (File.Exists(strFile) && !string.IsNullOrEmpty(strValue))

            {
                MessageBox.Show("Der eksistere allerede en fil med samme navn.");
                

            }
            else
            {
                File.Create(strFile).Close();
                
                File.WriteAllText(strFile, strValue);
            }

        }


    }

}


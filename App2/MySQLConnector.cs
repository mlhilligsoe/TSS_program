using MySql.Data.MySqlClient;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace App2
{
    class MySqlConnector
    {
        private MySqlConnection conn;

        public MySqlConnector()
        {
            // Required for SQL connection to use latin characterset
            EncodingProvider ppp = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(ppp);

            // Configure connection
            conn = new MySqlConnection("Server=" + Storage.GetSetting<string>("SQLServer") //127.0.0.1
                                    + ";Database=" + Storage.GetSetting<string>("SQLDB") //bosch
                                    + ";Uid=" + Storage.GetSetting<string>("SQLUser") //bosch
                                    + ";Pwd=" + Storage.GetSetting<string>("SQLPass") //12345
                                    + ";SslMode=None;");
        }

        // Configures, Opens and Tests MySQL connection
        public bool testConnection(TextBlock status)
        {
            // Try establisgn a connection
            try
            {
                conn.Open();

                if (conn.State != ConnectionState.Open)
                {
                    status.Text = "Could not load order from DB. \nConnection status: " + conn.State;
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                status.Text = "Could not load order from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
                return false;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }

            return true;
        }

        public bool loadOrder(int id, ref Order order, TextBlock status)
        {
            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                string stm = "SELECT * FROM `orders` WHERE `order_id` = " + id.ToString() + " ORDER BY `order_id` DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    order = new Order(rdr.GetInt32("order_id"),
                                        rdr.GetString("order_code"),
                                        rdr.GetString("order_product"),
                                        rdr.GetDateTime("order_start"),
                                        rdr.GetDateTime("order_change"),
                                        rdr.GetBoolean("order_complete"),
                                        rdr.GetInt32("order_quantity"));
                }
            }
            catch (MySqlException ex)
            {
                status.Text = "Could not load order from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
                return false;
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();
                
                if (conn != null)
                    conn.Close();
                
            }

            return true;
        }

        public bool loadOrder(string code, ref Order order, TextBlock status)
        {
            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                string stm = "SELECT * FROM `orders` WHERE `order_code` LIKE '" + code + "' ORDER BY `order_id` DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    order = new Order(rdr.GetInt32("order_id"),
                                        rdr.GetString("order_code"),
                                        rdr.GetString("order_product"),
                                        rdr.GetDateTime("order_start"),
                                        rdr.GetDateTime("order_change"),
                                        rdr.GetBoolean("order_complete"),
                                        rdr.GetInt32("order_quantity"));
                }
            }
            catch (MySqlException ex)
            {
                status.Text = "Could not load order from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
                return false;
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();

                if (conn != null)
                    conn.Close();
            }

            return true;
        }

        public bool loadProcess(int id, ref Process process, TextBlock status)
        {
            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                string stm = "SELECT * FROM `processes` WHERE `process_id` = " + id.ToString() + " ORDER BY `process_id` DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    process = new Process(rdr.GetInt32("process_id"),
                                            rdr.GetString("process_code"),
                                            rdr.GetDateTime("process_start"),
                                            rdr.GetDateTime("process_change"),
                                            rdr.GetBoolean("process_complete"),
                                            rdr.GetInt32("process_quantity"),
                                            rdr.GetInt32("process_waste"));
                }
            }
            catch (MySqlException ex)
            {
                status.Text = "Could not load process from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
                return false;
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();

                if (conn != null)
                    conn.Close();
            }

            return true;
        }

        public bool loadProcessFromOrder(int id, ref Process process, TextBlock status)
        {
            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                string stm = "SELECT * FROM `processes` WHERE `process_order_id` = " + id.ToString() + " ORDER BY `process_id` DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    process = new Process(rdr.GetInt32("process_id"),
                                            rdr.GetString("process_code"),
                                            rdr.GetDateTime("process_start"),
                                            rdr.GetDateTime("process_change"),
                                            rdr.GetBoolean("process_complete"),
                                            rdr.GetInt32("process_quantity"),
                                            rdr.GetInt32("process_waste"));
                }
            }
            catch (MySqlException ex)
            {
                status.Text = "Could not load process from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
                return false;
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();

                if (conn != null)
                    conn.Close();
            }

            return true;
        }

        public int createProcess(Process process, Order order, TextBlock status)
        {
            MySqlDataReader rdr = null;
            int r;

            try
            {
                conn.Open();
                
                string stm = string.Format("INSERT INTO `bosch`.`processes` "
                                            + "(`process_id`, `process_order_id`, `process_code`, `process_quantity`, `process_waste`, " 
                                            + "`process_start`, `process_change`, `process_complete`, `process_n_events`) "
                                            + "VALUES (NULL, '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}'); "
                                            + "SELECT last_insert_id()",
                                            order.id, process.code, process.quantity, process.waste, 
                                            process.start.ToString("yyyy-MM-dd hh:mm:ss"), process.change.ToString("yyyy-MM-dd hh:mm:ss"), process.complete?1:0, 0);
                
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                    r = rdr.GetInt32("last_insert_id()");
                else
                    r = -1;


            }
            catch (MySqlException ex)
            {
                status.Text = "Could not load process from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
                return -1;
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();

                if (conn != null)
                    conn.Close();
            }

            return r;
        }
    }
}

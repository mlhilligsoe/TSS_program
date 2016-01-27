using TSSDataLogger.Data;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace TSSDataLogger
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
                                    + ";SslMode=None"
                                    + ";Convert Zero Datetime=True;");
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
                                        rdr.GetInt32("order_n_processes"),
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
                                        rdr.GetInt32("order_n_processes"),
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

        public bool loadProcess(int id, string code, ref Process process, TextBlock status)
        {
            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                string stm = "SELECT * FROM `processes` WHERE `process_id` = " + id.ToString() + " AND `process_code` LIKE '" + code + "'ORDER BY `process_id` DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    process = new Process(rdr.GetInt32("process_id"),
                                            rdr.GetString("process_code"),
                                            rdr.GetDateTime("process_start"),
                                            rdr.GetDateTime("process_change"),
                                            rdr.GetInt32("process_n_events"),
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

        public bool loadProcessFromOrder(int id, string code, ref Process process, TextBlock status)
        {
            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                string stm = "SELECT * FROM `processes` WHERE `process_order_id` = " + id.ToString() + " AND `process_code` LIKE '" + code + "' ORDER BY `process_id` DESC LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    process = new Process(rdr.GetInt32("process_id"),
                                            rdr.GetString("process_code"),
                                            rdr.GetDateTime("process_start"),
                                            rdr.GetDateTime("process_change"),
                                            rdr.GetInt32("process_n_events"),
                                            rdr.GetBoolean("process_complete"),
                                            rdr.GetInt32("process_quantity"),
                                            rdr.GetInt32("process_waste"));
                    //status.Text += "\nprocess_n_events: " + rdr.GetInt32("process_n_events");
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

        public int createProcess(string process_code, Order order, TextBlock status)
        {
            MySqlDataReader rdr = null;
            int r;

            try
            {
                conn.Open();

                string stm = string.Format("INSERT INTO `bosch`.`processes` "
                                            + "(`process_order_id`, `process_code`) "
                                            + "VALUES ('{0}', '{1}'); "
                                            + "SELECT last_insert_id()",
                                            order.id, process_code);

                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                    r = rdr.GetInt32("last_insert_id()");
                else
                    r = -1;


            }
            catch (MySqlException ex)
            {
                status.Text = "Could not insert process into DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
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

        public int createOrder(Order order, TextBlock status)
        {
            MySqlDataReader rdr = null;
            int r;

            try
            {
                conn.Open();
                //status.Text += order.start.ToString();
                string stm = string.Format("INSERT INTO `bosch`.`orders` "
                                            + "(`order_code`, `order_product`, `order_quantity`, "
                                            + "`order_start`, `order_change`, `order_complete`, `order_n_processes`) "
                                            + "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}'); "
                                            + "SELECT last_insert_id()",
                                            order.code, order.product, order.quantity,
                                            order.start.ToString("yyyy-MM-dd H:mm:ss"), order.change.ToString("yyyy-MM-dd H:mm:ss"), order.complete ? 1 : 0, 0);

                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                    r = rdr.GetInt32("last_insert_id()");
                else
                    r = -1;


            }
            catch (MySqlException ex)
            {
                status.Text = "Could not create insert order into DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
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

        public bool updateOrder(Order order, TextBlock status)
        {
            if (order != null)
            {

                try
                {
                    conn.Open();

                    string stm = string.Format("UPDATE `bosch`.`orders` "
                                             + "SET `order_change` = '{0}', `order_complete` = '{1}', `order_n_processes` = '{2}'"
                                             + "WHERE `orders`.`order_id` = {3};",
                                                order.change, order.complete ? 1 : 0, order.n_processes, order.id);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    cmd.ExecuteReader();

                }
                catch (MySqlException ex)
                {
                    status.Text = "Could not update order in DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
                    return false;
                }
                finally
                {
                    if (conn != null)
                        conn.Close();
                }

                return true;

            }
            else {
                return false;
            }
        }

        public bool updateProcess(Process process, TextBlock status)
        {
            if(process != null)
            {

            try
            {
                conn.Open();

                string stm = string.Format("UPDATE `bosch`.`processes` "
                                        + "SET `process_code` = '{0}', `process_quantity` = '{1}', `process_waste` = '{2}', "
                                        + "`process_change` = '{3}', `process_complete` = '{4}', `process_n_events` = '{5}'"
                                        + "WHERE `processes`.`process_id` = {6};",
                                           process.code, process.quantity, process.waste, process.change, process.complete ? 1 : 0, process.n_events, process.id);

                MySqlCommand cmd = new MySqlCommand(stm, conn);
                cmd.ExecuteReader();


            }
            catch (MySqlException ex)
            {
                status.Text = "Could not update process in DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
                return false;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }

            return true;

            }
            else
            {
                return false;
            }
        }

        public bool loadEventsFromProcess(int processId, int noEvents, ref ObservableCollection<Event> events, TextBlock status)
        {
            MySqlDataReader rdr = null;
            
            try
            {
                conn.Open();

                string stm = "SELECT * FROM `events` WHERE `event_process_id` = " + processId.ToString() + " ORDER BY `event_id` DESC LIMIT " + noEvents.ToString();
                //status.Text += stm;
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                events.Clear();
                while (rdr.Read())
                {
                    events.Insert(0, new Event() {id = rdr.GetInt32("event_id"),
                                            code = rdr.GetString("event_code"),
                                            description = rdr.GetString("event_description"),
                                            start = rdr.GetDateTime("event_start"),
                                            change = rdr.GetDateTime("event_change"),
                                            complete = rdr.GetBoolean("event_complete") } );
                }

                for(int i = 0; i < events.Count; i++)
                {
                    events[i].listId = events.Count - i;
                }

            }
            catch (MySqlException ex)
            {
                status.Text = "Could not load event from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
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

        public int createEvent(string event_code, string event_description, Process process, TextBlock status)
        {
            MySqlDataReader rdr = null;
            int r;
            
            try
            {
                conn.Open();

                string stm = string.Format("INSERT INTO `bosch`.`events` "
                                            + "(`event_process_id`, `event_code`, `event_description`) "
                                            + "VALUES ('{0}', '{1}', '{2}'); "
                                            + "SELECT last_insert_id()",
                                            process.id, event_code, event_description);

                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    r = rdr.GetInt32("last_insert_id()");
                    process.n_events++;
                    rdr.Close();

                    stm = string.Format(  "UPDATE `bosch`.`processes` "
                                        + "SET `process_n_events` = '{0}' "
                                        + "WHERE `processes`.`process_id` = {1};",
                                        process.n_events, process.id);
                    cmd = new MySqlCommand(stm, conn);
                    cmd.ExecuteReader();
                }
            
                else
                    r = -1;


            }
            catch (MySqlException ex)
            {
                status.Text = "Could not insert event into DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
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

        public bool updateEvents(ObservableCollection<Event> events, TextBlock status)
        {
            bool failures = false;

            for (int i = 0; i < events.Count; i++)
            {
                failures = updateEvent(events[i], status);
            }

            return failures;
        }

        public bool updateEvent(Event evt, TextBlock status)
        {

            try
            {
                conn.Open();

                string stm = string.Format("UPDATE `bosch`.`events` "
                                         + "SET `event_code` = '{0}', `event_description` = '{1}', "
                                         + "`event_change` = '{2}', `event_complete` = '{3}'"
                                         + "WHERE `events`.`event_id` = {4};",
                                            evt.code, evt.description, evt.change, evt.complete?1:0, evt.id);

                MySqlCommand cmd = new MySqlCommand(stm, conn);
                cmd.ExecuteReader();


            }
            catch (MySqlException ex)
            {
                status.Text = "Could not update event in DB. \nConnection status: " + conn.State + " \nException: " + ex.Message;
                return false;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }

            return true;
        }

    }
}

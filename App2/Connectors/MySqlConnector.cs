using TSSDataLogger.Data;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Text;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using System;

namespace TSSDataLogger
{
    public class MySqlConnector
    {
        private MySqlConnection conn;
        private MainPage mainPage;
        Machine machine;
        Order order;
        Process process;
        ObservableCollection<Event> events;

        public MySqlConnector(MainPage mainPage, Machine machine, ref Order order, ref Process process, ObservableCollection<Event> events)
        {
            Debug.WriteLine("MySqlConnector");

            this.mainPage = mainPage;
            this.machine = machine;
            this.order = order;
            this.process = process;
            this.events = events;

            // Required for SQL connection to use latin characterset
            EncodingProvider ppp = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(ppp);

            // Configure connection
            conn = new MySqlConnection("Server=" + Storage.GetSetting<string>("SQLServer") // 192.168.0.100
                                    + ";Database=" + Storage.GetSetting<string>("SQLDB") // bosch
                                    + ";Uid=" + Storage.GetSetting<string>("SQLUser") // bosch
                                    + ";Pwd=" + Storage.GetSetting<string>("SQLPass") // 12345
                                    + ";SslMode=None"
                                    + ";Convert Zero Datetime=True;");
        }

        /* Verifies that the MySQL connection is Working */
        public bool testConnection()
        {
            Debug.WriteLine("MySqlConnector.testConnection");

            // Try establishing a connection
            try
            {
                conn.Open();

                if (conn.State != ConnectionState.Open)
                {
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                    mainPage.setStatus("Could not connect to DB. Please check configuration.");

                    return false;
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                return false;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }

            return true;
        }

        /* Create objects */
        public void createOrder()
        {
            Debug.WriteLine("MySqlConnector.createOrder");
            
            try
            {
                conn.Open();

                if(conn.State == ConnectionState.Open){
                    string stm = string.Format("INSERT INTO `bosch`.`orders` "
                                            + "(`order_code`, `order_start`, `order_change`) "
                                            + "VALUES ('{0}', '{1}', '{2}')",
                                            order.dbCode, order.dbStart, order.dbChange);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    cmd.ExecuteNonQuery();
                    order.id = (int)cmd.LastInsertedId;
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not create in DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public void createProcess()
        {
            Debug.WriteLine("MySqlConnector.createProcess");

            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = string.Format("INSERT INTO `bosch`.`processes` "
                                            + "(`process_order_id`, `process_code`, `process_start`, `process_change`, `process_complete`, `process_quantity`, `process_waste`) "
                                            + "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}'); ",
                                            order.id, process.dbCode, process.dbStart, process.dbChange, process.dbComplete, process.quantity, process.waste);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    cmd.ExecuteNonQuery();
                    process.id = (int)cmd.LastInsertedId;
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not create process in DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public void createEvent(Event evt)
        {
            Debug.WriteLine("MySqlConnector.createEvent");

            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = string.Format("INSERT INTO `bosch`.`events` "
                                            + "(`event_process_id`, `event_code`, `event_start`, `event_change`, `event_complete`) "
                                            + "VALUES('{0}', '{1}', '{2}', '{3}', '{4}')",
                                            process.id, evt.dbCode, evt.dbStart, evt.dbChange, evt.dbComplete);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    cmd.ExecuteNonQuery();
                    evt.id = (int)cmd.LastInsertedId;
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not create event in DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        /* Update objects */
        public void updateOrder()
        {
            Debug.WriteLine("MySqlConnector.updateOrder");

            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = string.Format("UPDATE `bosch`.`orders` "
                                            + "SET `order_change` = '{0}', `order_complete` = '{1}' "
                                            + "WHERE `orders`.`order_id` = '{2}';",
                                            order.dbChange, order.dbComplete, order.id);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not update order in DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public void updateProcess()
        {
            Debug.WriteLine("MySqlConnector.updateProcess");
        
            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = string.Format("UPDATE `bosch`.`processes` "
                                        + "SET `process_change` = '{0}', `process_complete` = '{1}', `process_quantity` = '{2}', `process_waste` = '{3}' "
                                        + "WHERE `processes`.`process_id` = '{4}';",
                                            process.dbChange, process.dbComplete, process.quantity, process.waste, process.id);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not update process in DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }
        
        public void updateEvents()
        {
            Debug.WriteLine("MySqlConnector.updateEvents");

            foreach (Event evt in events)
                updateEvent(evt);
        }

        public void updateEvent(Event evt)
        {
            Debug.WriteLine("MySqlConnector.updateEvent");

            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = string.Format("UPDATE `bosch`.`events` "
                                         + "SET `event_change` = '{0}', `event_complete` = '{1}' "
                                         + "WHERE `events`.`event_id` = '{2}';",
                                            evt.dbChange, evt.dbComplete, evt.id);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not update event in DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }
        
        /* Load objects */
        public void loadOrder(int orderId)
        {
            Debug.WriteLine("MySqlConnector.loadOrder");

            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = String.Format("SELECT * FROM `orders` WHERE `order_id` = '{0}' LIMIT 1", orderId);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                    {
                        order.load(rdr.GetInt32("order_id"),
                                    rdr.GetString("order_code"),
                                    rdr.GetDateTime("order_start"),
                                    rdr.GetDateTime("order_change"),
                                    rdr.GetBoolean("order_complete"));
                    }
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not load order from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();
                
                if (conn != null)
                    conn.Close();
            }
        }

        public bool loadOrder(string orderCode)
        {
            Debug.WriteLine("MySqlConnector.loadOrder");

            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = String.Format("SELECT * FROM `orders` WHERE `order_code` LIKE '{0}' LIMIT 1", orderCode);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                    {
                        order.load(rdr.GetInt32("order_id"),
                                    rdr.GetString("order_code"),
                                    rdr.GetDateTime("order_start"),
                                    rdr.GetDateTime("order_change"),
                                    rdr.GetBoolean("order_complete"));
                    }
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not load order from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
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

        public void loadProcess(int processId)
        {
            Debug.WriteLine("MySqlConnector.loadProcess");

            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = String.Format("SELECT * FROM `processes` WHERE `process_id` = '{0}' LIMIT 1", processId);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                    {
                        process.load(rdr.GetInt32("process_id"),
                                        rdr.GetString("process_code"),
                                        rdr.GetDateTime("process_start"),
                                        rdr.GetDateTime("process_change"),
                                        rdr.GetBoolean("process_complete"),
                                        rdr.GetInt32("process_quantity"),
                                        rdr.GetInt32("process_waste"));
                    }
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not load process from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();

                if (conn != null)
                    conn.Close();
            }
        }

        public void loadProcessFromOrder(int orderId, string processCode)
        {
            Debug.WriteLine("MySqlConnector.loadProcessFromOrder");

            MySqlDataReader rdr = null;

            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = String.Format("SELECT * FROM `processes` WHERE `process_order_id` = '{0}' AND `process_code` LIKE '{1}' LIMIT 1", orderId, processCode);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                    {
                        process.load(rdr.GetInt32("process_id"),
                                        rdr.GetString("process_code"),
                                        rdr.GetDateTime("process_start"),
                                        rdr.GetDateTime("process_change"),
                                        rdr.GetBoolean("process_complete"),
                                        rdr.GetInt32("process_quantity"),
                                        rdr.GetInt32("process_waste"));
                    }
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not load process from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();

                if (conn != null)
                    conn.Close();
            }
        }

        public void loadEventsFromProcess(int processId)
        {
            Debug.WriteLine("MySqlConnector.loadEventsFromProcess");

            MySqlDataReader rdr = null;
            
            try
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    string stm = String.Format("SELECT * FROM `events` WHERE `event_process_id` = '{0}' ORDER BY `event_id` ASC", processId);
                    Debug.WriteLine("stm: " + stm);

                    MySqlCommand cmd = new MySqlCommand(stm, conn);
                    rdr = cmd.ExecuteReader();

                    events.Clear();
                    while (rdr.Read())
                    {
                        events.Add(new Event(rdr.GetInt32("event_id"),
                                                rdr.GetString("event_code"),
                                                rdr.GetDateTime("event_start"),
                                                rdr.GetDateTime("event_change"),
                                                rdr.GetBoolean("event_complete")));
                    }
                }
                else
                {
                    mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
                    Debug.WriteLine("Could not connect to DB. \nConnection state: " + conn.State);
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine("Could not load events from DB. \nConnection status: " + conn.State + " \nException: " + ex.Message);
                mainPage.setStatus("FEJL I DATABASE FORBINDELSE - Tjek konfiguration eller tilkald support");
            }
            finally
            {
                if (rdr != null)
                    rdr.Close();

                if (conn != null)
                    conn.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSSDataLogger
{
    public class Order
    {
        private bool active = false;

        // Object variables
        public int id { get; set; }
        public string code { get; set; }
        public DateTime start { get; set; }
        public DateTime change { get; set; }
        public bool complete { get; set; }
        public string product { get; set; }
        public int quantity { get; set; }

        // GUI representation
        public string guiStart { get { return start.ToString("HH:mm dd/MM"); } }
        public string guiChange { get { return change.ToString("HH:mm dd/MM"); } }

        //DB Representation
        public string dbCode { get { return code.ToString(); } }
        public string dbStart { get { return start.ToString("yyyy-MM-dd H:mm:ss"); } }
        public string dbChange { get { return change.ToString("yyyy-MM-dd H:mm:ss"); } }
        public string dbComplete { get { return complete ? "1" : "0" ; } }

        public Order()
        {
        }

        public void load(int id, string code, DateTime start, DateTime change, bool complete = false)
        {
            active = true;
            this.id = id;
            this.code = code;
            this.start = start;
            this.change = change;
            this.complete = complete;
            this.product = "??Pakning??";
            this.quantity = 123;
        }

        public void load(string code, DateTime start, DateTime change, bool complete = false)
        {
            active = true;
            this.id = -1;
            this.code = code;
            this.start = start;
            this.change = change;
            this.complete = complete;
            this.product = "??Pakning??";
            this.quantity = 123;
        }

        public void unload()
        {
            active = false;
            this.id = -1;
        }

        public bool isLoaded()
        {
            return active;
        }

        public static void saveToStorage(Order order)
        {
            Storage.SetSetting("order", Storage.Serialize(order));
        }

        public static Order loadFromStorage()
        {
            return Storage.Deserialize<Order>(Storage.GetSetting<string>("order"));
        }
    }

    
}

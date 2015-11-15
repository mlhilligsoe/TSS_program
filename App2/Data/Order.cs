using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App2
{
    public class Order
    {
        public int id;
        public string code;
        public string product;
        public DateTime start;
        public DateTime change;
        public bool complete;
        public int quantity;
        
        
        public Order(int id, string code, string product,  DateTime start, DateTime change, bool complete = false, int quantity = 0)
        {
            this.id = id;
            this.code = code;
            this.product = product;
            this.start = start;
            this.change = change;
            this.complete = complete;
            this.quantity = quantity;
        }

        public Order(int id, string code, string product, DateTime start, bool complete = false, int quantity = 0)
        {
            this.id = id;
            this.code = code;
            this.product = product;
            this.start = start;
            this.change = start;
            this.complete = complete;
            this.quantity = quantity;
        }

        public Order(int id, string code, string product, bool complete = false, int quantity = 0)
        {
            this.id = id;
            this.code = code;
            this.product = product;
            this.start = DateTime.Now;
            this.change = start;
            this.complete = complete;
            this.quantity = quantity;
        }

        public static void save(Order order)
        {
            Storage.SetSetting("order", Storage.Serialize(order));
        }

        public static Order load()
        {
            return Storage.Deserialize<Order>(Storage.GetSetting<string>("order"));
        }
    }

    
}

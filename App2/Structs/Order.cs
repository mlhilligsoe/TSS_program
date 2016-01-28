using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSSDataLogger
{
    public class Order
    {
        private MainPage mainPage;
        private bool active = false;

        public int id;
        public string code;
        public string product;
        public DateTime start;
        public DateTime change;
        public bool complete;
        public int quantity;
        public int n_processes;

        public Order(MainPage mainPage)
        {
            this.mainPage = mainPage;
        }

        public void load(int id, string code, string product, DateTime start, DateTime change, int n_processes, bool complete = false, int quantity = 0)
        {
            active = true;
            this.id = id;
            this.code = code;
            this.product = product;
            this.start = start;
            this.change = change;
            this.complete = complete;
            this.quantity = quantity;
            this.n_processes = n_processes;
        }

        public void unload()
        {
            active = false;
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

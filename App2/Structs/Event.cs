using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;

namespace TSSDataLogger.Data
{
    [DataContract, KnownType(typeof(DateTime))]
    public class Event
    {
        // Object variables
        public int id { get; set; }
        public string code { get; set; }
        public DateTime start { get; set; }
        public DateTime change { get; set; }
        public bool complete { get; set; }

        // GUI representation
        public string guiStart { get { return start.ToString("HH:mm"); } }
        public string guiChange { get { return change.ToString("HH:mm"); } }

        //DB Representation
        public string dbCode { get { return code.ToString(); } }
        public string dbStart { get { return start.ToString("yyyy-MM-dd H:mm:ss"); } }
        public string dbChange { get { return change.ToString("yyyy-MM-dd H:mm:ss"); } }
        public string dbComplete { get { return complete ? "1" : "0"; } }

        public Event(int id, string code, DateTime start, DateTime change, bool complete = false)
        {
            this.id = id;
            this.code = code;
            this.start = start;
            this.change = change;
            this.complete = complete;
        }

        public Event(string code, DateTime start, DateTime change, bool complete = false)
        {
            this.id = -1;
            this.code = code;
            this.start = start;
            this.change = change;
            this.complete = complete;
        }

        public Event(string code)
        {
            this.id = -1;
            this.code = code;
            this.start = DateTime.Now;
            this.change = DateTime.Now;
            this.complete = false;
        }

        public static void Save(Event @event)
        {
            Storage.SetSetting("event", Storage.Serialize(@event));
        }

        public static Event Load()
        {
            return Storage.Deserialize<Event>(Storage.GetSetting<string>("event"));
        }
    }
}

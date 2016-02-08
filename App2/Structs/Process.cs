using TSSDataLogger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TSSDataLogger;

[DataContract, KnownType(typeof(DateTime))]
public class Process
{
    private bool active = false;
    public bool stopped { get; set; }

    // Object variables
    public int id { get; set; }
    public string code { get; set; }
    public DateTime start { get; set; }
    public DateTime change { get; set; }
    public bool complete { get; set; }
    public int quantity { get; set; }
    public int waste { get; set; }

    // GUI representation
    public string guiStart { get { return start.ToString("HH:mm dd/MM"); } }
    public string guiChange { get { return change.ToString("HH:mm dd/MM"); } }

    //DB Representation
    public string dbCode { get { return code.ToString(); } }
    public string dbStart { get { return start.ToString("yyyy-MM-dd H:mm:ss"); } }
    public string dbChange { get { return change.ToString("yyyy-MM-dd H:mm:ss"); } }
    public string dbComplete { get { return complete ? "1" : "0"; } }

    public Process()
    {
    }

    public void load(int id, string code, DateTime start, DateTime change, bool complete = false, int quantity = 0, int waste = 0)
    {
        active = true;
        this.id = id;
        this.code = code;
        this.start = new DateTime(start.Ticks, DateTimeKind.Local);
        this.change = new DateTime(change.Ticks, DateTimeKind.Local);
        this.complete = complete;
        this.quantity = quantity;
        this.waste = waste;
        this.stopped = true;
    }

    public void load(string code, DateTime start, DateTime change, bool complete = false, int quantity = 0, int waste = 0)
    {
        active = true;
        this.id = -1;
        this.code = code;
        this.start = new DateTime(start.Ticks, DateTimeKind.Local);
        this.change = new DateTime(change.Ticks, DateTimeKind.Local);
        this.complete = complete;
        this.quantity = quantity;
        this.waste = waste;
        this.stopped = true;
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

    public static void Save(Process process)
    {
        Storage.SetSetting("process", Storage.Serialize(process));
    }

    public static Process Load()
    {
        return Storage.Deserialize<Process>(Storage.GetSetting<string>("process"));
    }
}

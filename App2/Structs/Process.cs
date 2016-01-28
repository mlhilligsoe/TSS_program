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
    private MainPage mainPage;
    private bool active = false;

    public int id;
    public string code;
    public int quantity;
    public int waste;
    public DateTime start;
    public DateTime change;
    public bool complete;
    public int n_events;
    public bool hasLatched;

    public Process(MainPage mainPage)
    {
        this.mainPage = mainPage;
    }

    public void load(int id, string code, DateTime start, DateTime change, int n_events = 0, bool complete = false, int quantity = 0, int waste = 0)
    {
        active = true;
        this.id = id;
        this.code = code;
        this.start = new DateTime(start.Ticks, DateTimeKind.Local);
        this.change = new DateTime(change.Ticks, DateTimeKind.Local);
        this.complete = complete;
        this.quantity = quantity;
        this.waste = waste;
        this.n_events = n_events;
        this.hasLatched = false;
    }

    public void unload()
    {
        active = false;
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

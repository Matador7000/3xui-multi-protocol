using Newtonsoft.Json;

while (true)
{
    using var db = new MultiProtocolContext();
    var Clients = db.Client_Traffics.ToList();

    if (!File.Exists("LocalDB.json"))
    {
        localDB local = new localDB() { Sec = 10, clients = Clients };
        File.WriteAllText("LocalDB.json", JsonConvert.SerializeObject(local));
    }

    localDB localDB = JsonConvert.DeserializeObject<localDB>(File.ReadAllText("LocalDB.json"));
    List<Client> ALLClients = new List<Client>();

    var inbounds = db.Inbounds.ToList();
    foreach (var item in inbounds)
    {
        inboundsetting setting = JsonConvert.DeserializeObject<inboundsetting>(item.Settings);
        ALLClients.AddRange(setting.clients);
    }

    List<Client> FinalClients = new List<Client>();
    List<Client_Traffics> FinalClients_Traffic = new List<Client_Traffics>();

    foreach (var client in ALLClients.DistinctBy(x => x.subId))
    {
        var Calculate = ALLClients.Where(x => x.subId == client.subId).ToList();
        var Calculate2 = Calculate.Select(c => Clients.FirstOrDefault(x => x.Email == c.email)).ToList();

        Int64? maxTotalGB = Calculate.Max(x => x.totalGB);
        Int64? maxTotal = Calculate2.Max(x => x.Total);
        Int64? maxUP = Calculate2.Max(x => x.Up);
        Int64? maxDOWN = Calculate2.Max(x => x.Down);

        Int64? UP = 0;
        Int64? DOWN = 0;

        Int64? DateMax = Calculate2.Max(x => x.Expiry_Time);
        Int64? DateMin = Calculate2.Min(x => x.Expiry_Time);
        Int64? ExpireTime = DateMax > 0 ? DateMax : DateMin;

        foreach (var client2 in Calculate2)
        {
            if (client2.Up != maxUP)
            {
                Int64? oldusage = localDB.clients.FirstOrDefault(x => x.Email == client2.Email)?.Up;
                if (client2.Up > oldusage && oldusage.HasValue)
                    UP += client2.Up - oldusage;
            }
            if (client2.Down != maxDOWN)
            {
                Int64? oldusage = localDB.clients.FirstOrDefault(x => x.Email == client2.Email)?.Down;
                if (client2.Down > oldusage && oldusage.HasValue)
                    DOWN += client2.Down - oldusage;
            }
        }

        bool check = Calculate2.Any(x => x.Enable);

        foreach (var cal2 in Calculate2)
        {
            cal2.Total = maxTotal;
            cal2.Up = maxUP + UP;
            cal2.Down = maxDOWN + DOWN;
            cal2.Expiry_Time = ExpireTime;
            FinalClients_Traffic.Add(cal2);
        }

        foreach (var cal in Calculate)
        {
            cal.totalGB = maxTotalGB;
            cal.expiryTime = ExpireTime;
            FinalClients.Add(cal);
        }
    }

    db.Client_Traffics.UpdateRange(FinalClients_Traffic);

    List<Inbound> FinalInbounds = new List<Inbound>();
    foreach (var inbound in db.Inbounds.Where(x => x.Protocol == "vmess" || x.Protocol == "vless"))
    {
        inboundsetting setting = JsonConvert.DeserializeObject<inboundsetting>(inbound.Settings);
        var clis = FinalClients_Traffic.Where(x => x.Inbound_Id == inbound.Id).ToList();
        var addtoInbound = clis.Select(client => FinalClients.FirstOrDefault(x => x.email == client.Email)).ToList();

        if (addtoInbound.Any())
        {
            var pastclients = setting.clients.Where(client => !addtoInbound.Any(x => x.email == client.email)).ToList();
            pastclients.AddRange(addtoInbound);
            setting.clients = pastclients;
            inbound.Settings = JsonConvert.SerializeObject(setting);
            FinalInbounds.Add(inbound);
        }
    }

    db.Inbounds.UpdateRange(FinalInbounds);
    db.SaveChanges();

    var client_Traffics = db.Client_Traffics.ToList();
    localDB updateLocal = new localDB() { Sec = localDB.Sec, clients = client_Traffics };
    File.WriteAllText("LocalDB.json", JsonConvert.SerializeObject(updateLocal));

    Console.WriteLine("Done");
    Thread.Sleep(25 * 1000);
}

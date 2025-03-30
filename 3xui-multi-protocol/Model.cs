using Microsoft.EntityFrameworkCore;
using System.IO;

public class MultiProtocolContext : DbContext
{
    public DbSet<Inbound> Inbounds { get; set; }
    public DbSet<Client_Traffics> Client_Traffics { get; set; }

    public string DbPath { get; }

    public MultiProtocolContext()
    {
        // استفاده از Path.Combine برای اطمینان از سازگاری با سیستم‌های مختلف
        var folder = "/etc/x-ui/";
        DbPath = Path.Combine(folder, "x-ui.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public class Inbound
{
    public int? Id { get; set; }
    public string? Listen { get; set; }
    public int? UserId { get; set; }
    public long? Up { get; set; }
    public long? Down { get; set; }
    public long? Total { get; set; }
    public string? Settings { get; set; }
    public string? Tag { get; set; }
    public string? Sniffing { get; set; }
    public string? StreamSettings { get; set; }
    public string? Remark { get; set; }
    public bool? Enable { get; set; }
    public long? ExpiryTime { get; set; }
    public int? Port { get; set; }
    public string? Protocol { get; set; }
}

public class Client_Traffics
{
    public int? Id { get; set; }
    public int? InboundId { get; set; }
    public int? Reset { get; set; }
    public string? Email { get; set; }
    public long? Up { get; set; }
    public long? Down { get; set; }
    public long? Total { get; set; }
    public long? ExpiryTime { get; set; }
    public bool? Enable { get; set; }
}

public class Client
{
    public string? Email { get; set; }
    public bool? Enable { get; set; }
    public long? ExpiryTime { get; set; }
    public string? Flow { get; set; }
    public string? Id { get; set; }
    public int? LimitIp { get; set; }
    public bool? Reset { get; set; }
    public string? SubId { get; set; }
    public string? TgId { get; set; }
    public long? TotalGB { get; set; }
}

public class InboundSetting
{
    public List<Client> Clients { get; set; }
    public string Decryption { get; set; }
    public List<object> Fallbacks { get; set; }
}

public class LocalDB
{
    public int Sec { get; set; }
    public List<Client_Traffics> Clients { get; set; }
}

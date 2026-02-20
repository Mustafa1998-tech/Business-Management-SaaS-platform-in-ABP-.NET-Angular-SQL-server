namespace SaasSystem.Caching;

[Serializable]
public class DashboardStatsCacheItem
{
    public int TotalCustomers { get; set; }
    public int ActiveProjects { get; set; }
    public int PendingTasks { get; set; }
    public decimal OutstandingInvoices { get; set; }
    public decimal MonthRevenue { get; set; }
    public List<DashboardRevenueCacheItem> RevenueByMonth { get; set; } = new();
}

[Serializable]
public class DashboardRevenueCacheItem
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

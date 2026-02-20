namespace SaasSystem.Dashboard;

public class DashboardStatsDto
{
    public int TotalCustomers { get; set; }
    public int ActiveProjects { get; set; }
    public int PendingTasks { get; set; }
    public decimal OutstandingInvoices { get; set; }
    public decimal MonthRevenue { get; set; }
    public IReadOnlyList<RevenuePointDto> RevenueByMonth { get; set; } = Array.Empty<RevenuePointDto>();
}

public class RevenuePointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

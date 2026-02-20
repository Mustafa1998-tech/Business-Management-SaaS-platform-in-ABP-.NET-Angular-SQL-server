using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using SaasSystem.Caching;
using SaasSystem.Domain.Entities;
using SaasSystem.Enums;
using SaasSystem.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;

namespace SaasSystem.Dashboard;

[Authorize(SaasSystemPermissions.Dashboard.Default)]
public class DashboardAppService : ApplicationService, IDashboardAppService
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<WorkTask, Guid> _taskRepository;
    private readonly IRepository<Invoice, Guid> _invoiceRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IDistributedCache<DashboardStatsCacheItem, string> _cache;

    public DashboardAppService(
        IRepository<Customer, Guid> customerRepository,
        IRepository<Project, Guid> projectRepository,
        IRepository<WorkTask, Guid> taskRepository,
        IRepository<Invoice, Guid> invoiceRepository,
        IRepository<Payment, Guid> paymentRepository,
        IDistributedCache<DashboardStatsCacheItem, string> cache)
    {
        _customerRepository = customerRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _cache = cache;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        string cacheKey = $"dashboard:stats:{CurrentTenant.Id?.ToString() ?? "host"}";
        DashboardStatsCacheItem? cached = await _cache.GetAsync(cacheKey);

        if (cached is not null)
        {
            return new DashboardStatsDto
            {
                TotalCustomers = cached.TotalCustomers,
                ActiveProjects = cached.ActiveProjects,
                PendingTasks = cached.PendingTasks,
                OutstandingInvoices = cached.OutstandingInvoices,
                MonthRevenue = cached.MonthRevenue,
                RevenueByMonth = cached.RevenueByMonth
                    .Select(x => new RevenuePointDto { Month = x.Month, Amount = x.Amount })
                    .ToList()
            };
        }

        DateTime now = Clock.Now;
        DateTime monthStart = new(now.Year, now.Month, 1);

        IQueryable<Customer> customers = await _customerRepository.GetQueryableAsync();
        IQueryable<Project> projects = await _projectRepository.GetQueryableAsync();
        IQueryable<WorkTask> tasks = await _taskRepository.GetQueryableAsync();
        IQueryable<Invoice> invoices = await _invoiceRepository.GetQueryableAsync();
        IQueryable<Payment> payments = await _paymentRepository.GetQueryableAsync();

        int totalCustomers = await AsyncExecuter.CountAsync(customers);
        int activeProjects = await AsyncExecuter.CountAsync(projects.Where(x => x.Status == ProjectStatus.Active));
        int pendingTasks = await AsyncExecuter.CountAsync(tasks.Where(x => x.Status != WorkTaskStatus.Done));

        decimal outstandingInvoices = await AsyncExecuter.SumAsync(
            invoices.Where(x => x.Status != InvoiceStatus.Paid),
            x => x.TotalAmount);

        decimal monthRevenue = await AsyncExecuter.SumAsync(
            payments.Where(x => x.PaidAt >= monthStart),
            x => x.Amount);

        List<RevenuePointDto> revenueByMonth = await BuildRevenueSeriesAsync(payments, now);

        DashboardStatsCacheItem item = new()
        {
            TotalCustomers = totalCustomers,
            ActiveProjects = activeProjects,
            PendingTasks = pendingTasks,
            OutstandingInvoices = outstandingInvoices,
            MonthRevenue = monthRevenue,
            RevenueByMonth = revenueByMonth
                .Select(x => new DashboardRevenueCacheItem { Month = x.Month, Amount = x.Amount })
                .ToList()
        };

        await _cache.SetAsync(
            cacheKey,
            item,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return new DashboardStatsDto
        {
            TotalCustomers = totalCustomers,
            ActiveProjects = activeProjects,
            PendingTasks = pendingTasks,
            OutstandingInvoices = outstandingInvoices,
            MonthRevenue = monthRevenue,
            RevenueByMonth = revenueByMonth
        };
    }

    private async Task<List<RevenuePointDto>> BuildRevenueSeriesAsync(IQueryable<Payment> payments, DateTime now)
    {
        DateTime firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-11);

        List<(int Year, int Month, decimal Amount)> grouped = await AsyncExecuter.ToListAsync(
            payments
                .Where(x => x.PaidAt >= firstMonth)
                .GroupBy(x => new { x.PaidAt.Year, x.PaidAt.Month })
                .Select(x => new ValueTuple<int, int, decimal>(x.Key.Year, x.Key.Month, x.Sum(p => p.Amount))));

        List<RevenuePointDto> result = new();

        for (int i = 0; i < 12; i++)
        {
            DateTime month = firstMonth.AddMonths(i);
            decimal amount = grouped
                .Where(x => x.Year == month.Year && x.Month == month.Month)
                .Select(x => x.Amount)
                .FirstOrDefault();

            result.Add(new RevenuePointDto
            {
                Month = month.ToString("yyyy-MM"),
                Amount = amount
            });
        }

        return result;
    }
}

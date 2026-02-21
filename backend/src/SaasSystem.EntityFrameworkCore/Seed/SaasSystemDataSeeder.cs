using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaasSystem.Domain.Entities;
using SaasSystem.Domain.MultiTenancy;
using SaasSystem.EntityFrameworkCore.EntityFrameworkCore;
using SaasSystem.Enums;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SaasSystem.EntityFrameworkCore.Seed;

public class SaasSystemDataSeeder : ITransientDependency
{
    private readonly SaasSystemDbContext _dbContext;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<SaasSystemDataSeeder> _logger;

    public SaasSystemDataSeeder(
        SaasSystemDbContext dbContext,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<SaasSystemDataSeeder> logger)
    {
        _dbContext = dbContext;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        foreach (Guid? tenantId in SaasSystemSeedTenants.WithHost())
        {
            using (_currentTenant.Change(tenantId))
            {
                await SeedTenantAsync(tenantId);
            }
        }
    }

    private async Task SeedTenantAsync(Guid? tenantId)
    {
        if (await _dbContext.Customers.AnyAsync())
        {
            _logger.LogInformation("Seed skipped for tenant {Tenant} because data already exists.", tenantId?.ToString() ?? "host");
            return;
        }

        using IUnitOfWork uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

        string tenantName = tenantId.HasValue
            ? $"Tenant {tenantId.Value.ToString()[..8]}"
            : "Host Workspace";
        string tenantCode = tenantId.HasValue
            ? tenantId.Value.ToString()[..4]
            : "HOST";

        TenantProfile tenant = new(_guidGenerator.Create(), tenantId, tenantName, "Enterprise");
        await _dbContext.TenantProfiles.AddAsync(tenant);

        DateTime dataStart = new(DateTime.UtcNow.Year - 5, 1, 1);
        int reportableDays = Math.Max(365, (DateTime.UtcNow.Date - dataStart.Date).Days);
        Random rng = new((tenantId ?? Guid.Empty).GetHashCode());

        List<Customer> customers = new();
        for (int i = 1; i <= 160; i++)
        {
            Customer customer = new(
                _guidGenerator.Create(),
                tenantId,
                $"Customer {i:000}",
                $"customer{i:000}@tenant{tenantCode.ToLowerInvariant()}.com",
                $"+1-555-{rng.Next(1000, 9999)}",
                $"{rng.Next(10, 999)} Main Street");

            if (i % 9 == 0)
            {
                customer.SetActive(false);
            }

            customers.Add(customer);
        }

        await _dbContext.Customers.AddRangeAsync(customers);

        List<Project> projects = new();
        List<WorkTask> tasks = new();
        List<Invoice> invoices = new();
        List<Payment> payments = new();

        int invoiceCounter = 1;

        foreach (Customer customer in customers)
        {
            int projectCount = rng.Next(4, 10);
            for (int p = 1; p <= projectCount; p++)
            {
                DateTime projectStart = dataStart.AddDays(rng.Next(0, reportableDays));

                Project project = new(
                    _guidGenerator.Create(),
                    tenantId,
                    customer.Id,
                    $"Project {customer.Name}-{p}",
                    "Customer delivery project",
                    projectStart,
                    rng.Next(25000, 500000));

                project.ChangeStatus((ProjectStatus)rng.Next(1, 5));
                if (project.Status == ProjectStatus.Completed)
                {
                    project.SetEndDate(projectStart.AddDays(rng.Next(45, 420)));
                }

                projects.Add(project);

                int taskCount = rng.Next(25, 70);
                for (int t = 1; t <= taskCount; t++)
                {
                    WorkTask task = new(
                        _guidGenerator.Create(),
                        tenantId,
                        project.Id,
                        $"Task {t} for {project.Name}",
                        "Planned implementation task",
                        t);

                    task.MoveTo((WorkTaskStatus)rng.Next(1, 6), t);
                    decimal estimated = rng.Next(4, 96);
                    decimal spent = Math.Max(0, estimated + rng.Next(-8, 24));
                    task.UpdateEffort(estimated, spent);
                    task.SetDueDate(projectStart.AddDays(rng.Next(7, 180)));

                    tasks.Add(task);
                }

                int invoiceCount = rng.Next(8, 21);
                for (int inv = 1; inv <= invoiceCount; inv++)
                {
                    DateTime issueDate = dataStart.AddDays(rng.Next(0, reportableDays));
                    decimal subTotal = rng.Next(3000, 90000);
                    decimal tax = decimal.Round(subTotal * 0.15m, 2);

                    Invoice invoice = new(
                        _guidGenerator.Create(),
                        tenantId,
                        customer.Id,
                        project.Id,
                        $"INV-{tenantCode}-{invoiceCounter:00000}",
                        issueDate,
                        issueDate.AddDays(30),
                        subTotal,
                        tax);

                    invoiceCounter++;
                    InvoiceStatus status = PickInvoiceStatus(rng);
                    invoice.ChangeStatus(status);
                    invoices.Add(invoice);

                    if (status == InvoiceStatus.Paid)
                    {
                        int paymentParts = rng.Next(1, 4);
                        decimal remaining = invoice.TotalAmount;
                        for (int part = 1; part <= paymentParts; part++)
                        {
                            decimal amount = part == paymentParts
                                ? remaining
                                : decimal.Round(invoice.TotalAmount * (decimal)(rng.NextDouble() * 0.45 + 0.2), 2);

                            remaining = decimal.Max(0, remaining - amount);

                            Payment payment = new(
                                _guidGenerator.Create(),
                                tenantId,
                                invoice.Id,
                                amount,
                                invoice.IssueDate.AddDays(rng.Next(2, 75)),
                                (PaymentMethod)rng.Next(1, 5),
                                $"PAY-{invoice.InvoiceNumber}-{part:00}");

                            payments.Add(payment);
                        }
                    }
                    else if (status == InvoiceStatus.PartiallyPaid)
                    {
                        int paymentParts = rng.Next(1, 3);
                        decimal paidTarget = decimal.Round(invoice.TotalAmount * (decimal)(rng.NextDouble() * 0.6 + 0.2), 2);
                        decimal remaining = paidTarget;

                        for (int part = 1; part <= paymentParts; part++)
                        {
                            decimal amount = part == paymentParts
                                ? remaining
                                : decimal.Round(paidTarget * (decimal)(rng.NextDouble() * 0.6 + 0.2), 2);

                            remaining = decimal.Max(0, remaining - amount);

                            Payment payment = new(
                                _guidGenerator.Create(),
                                tenantId,
                                invoice.Id,
                                amount,
                                invoice.IssueDate.AddDays(rng.Next(2, 90)),
                                (PaymentMethod)rng.Next(1, 5),
                                $"PAY-{invoice.InvoiceNumber}-{part:00}");

                            payments.Add(payment);
                        }
                    }
                }
            }
        }

        await _dbContext.Projects.AddRangeAsync(projects);
        await _dbContext.Tasks.AddRangeAsync(tasks);
        await _dbContext.Invoices.AddRangeAsync(invoices);
        await _dbContext.Payments.AddRangeAsync(payments);

        await _dbContext.SaveChangesAsync();
        await uow.CompleteAsync();
    }

    private static InvoiceStatus PickInvoiceStatus(Random rng)
    {
        int roll = rng.Next(1, 101);
        return roll switch
        {
            <= 6 => InvoiceStatus.Draft,
            <= 34 => InvoiceStatus.Sent,
            <= 56 => InvoiceStatus.PartiallyPaid,
            <= 84 => InvoiceStatus.Paid,
            <= 95 => InvoiceStatus.Overdue,
            _ => InvoiceStatus.Cancelled
        };
    }
}

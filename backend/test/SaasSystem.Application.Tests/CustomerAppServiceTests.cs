using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using SaasSystem.Customers;
using SaasSystem.Domain.Entities;
using SaasSystem.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SaasSystem.Application.Tests;

public class CustomerAppServiceTests
{
    private readonly Mock<IRepository<Customer, Guid>> _repository;
    private readonly Mock<ICurrentTenant> _currentTenant;
    private readonly Mock<IGuidGenerator> _guidGenerator;
    private readonly CustomerAppService _service;

    public CustomerAppServiceTests()
    {
        _repository = new Mock<IRepository<Customer, Guid>>();
        _currentTenant = new Mock<ICurrentTenant>();
        _guidGenerator = new Mock<IGuidGenerator>();

        _service = new CustomerAppService(_repository.Object, _currentTenant.Object, _guidGenerator.Object);
    }

    [Fact]
    public async Task GetListAsync_Should_Filter_By_Name()
    {
        Guid tenantId = Guid.NewGuid();

        _repository
            .Setup(x => x.GetQueryableAsync())
            .ReturnsAsync(new List<Customer>
            {
                new(Guid.NewGuid(), tenantId, "Alpha Corp", "alpha@demo.com", "111", "A"),
                new(Guid.NewGuid(), tenantId, "Beta", "beta@demo.com", "222", "B")
            }.AsQueryable());

        var result = await _service.GetListAsync(new CustomerListRequestDto
        {
            SkipCount = 0,
            MaxResultCount = 10,
            Filter = "Alpha"
        });

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(x => x.Name == "Alpha Corp");
    }

    [Fact]
    public async Task CreateAsync_Should_Insert_Customer()
    {
        Guid tenantId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();

        _currentTenant.SetupGet(x => x.Id).Returns(tenantId);
        _guidGenerator.Setup(x => x.Create()).Returns(customerId);

        _repository
            .Setup(x => x.InsertAsync(It.IsAny<Customer>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer entity, bool _, CancellationToken _) => entity);

        var result = await _service.CreateAsync(new CreateUpdateCustomerDto
        {
            Name = "Acme",
            Email = "acme@demo.com",
            Phone = "333",
            Address = "Address",
            IsActive = true
        });

        result.Id.Should().Be(customerId);
        result.Name.Should().Be("Acme");

        _repository.Verify(x => x.InsertAsync(It.IsAny<Customer>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Call_Repository()
    {
        Guid id = Guid.NewGuid();

        await _service.DeleteAsync(id);

        _repository.Verify(x => x.DeleteAsync(id, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Authorization_Attributes_Should_Be_Configured()
    {
        typeof(CustomerAppService)
            .GetMethod(nameof(CustomerAppService.CreateAsync))
            ?.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .Single()
            .Policy
            .Should()
            .Be(SaasSystemPermissions.Customers.Create);

        typeof(CustomerAppService)
            .GetMethod(nameof(CustomerAppService.UpdateAsync))
            ?.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .Single()
            .Policy
            .Should()
            .Be(SaasSystemPermissions.Customers.Update);

        typeof(CustomerAppService)
            .GetMethod(nameof(CustomerAppService.DeleteAsync))
            ?.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .Single()
            .Policy
            .Should()
            .Be(SaasSystemPermissions.Customers.Delete);
    }
}

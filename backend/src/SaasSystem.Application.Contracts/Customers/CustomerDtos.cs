using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace SaasSystem.Customers;

public class CustomerDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateUpdateCustomerDto
{
    [Required]
    [StringLength(SaasSystemConsts.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(SaasSystemConsts.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(SaasSystemConsts.PhoneMaxLength)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(SaasSystemConsts.DescriptionMaxLength)]
    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class CustomerListRequestDto : PagedAndSortedResultRequestDto
{
    [StringLength(SaasSystemConsts.NameMaxLength)]
    public string? Filter { get; set; }

    public bool? IsActive { get; set; }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBoilerplate.Shared.Dto.Tenant
{
    public class TenantDto
    {
        public string Title { get; set; }
        public Guid OwnerUserId { get; set; }
        public string OwnerName { get; set; }
        public List<Guid> Users { get; set; }
    }
}
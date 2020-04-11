using System;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Dto.Tenant
{
    public class TenantDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Guid OwnerUserId { get; set; }
        public string OwnerName { get; set; }
        public List<Guid> Users { get; set; }
    }
}
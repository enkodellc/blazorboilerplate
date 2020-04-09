using BlazorBoilerplate.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Dto.Admin
{
    public class RoleDto
    {
        [StringLength(64, ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "ErrorInvalidLength", MinimumLength = 2)]
        [RegularExpression(@"[^\s]+", ErrorMessageResourceType = typeof(Strings), ErrorMessageResourceName = "SpacesNotPermitted")]
        [Display(Name = "Name", ResourceType = typeof(Strings))]
        public string Name { get; set; }

        public List<string> Permissions { get; set; }

        public string FormattedPermissions
        {
            get
            {
                return String.Join(", ", Permissions.ToArray());
            }
        }
    }
}

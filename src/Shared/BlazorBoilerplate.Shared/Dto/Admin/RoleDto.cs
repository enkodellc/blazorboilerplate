using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Shared.Dto.Admin
{
    public class RoleDto
    {
        [StringLength(64, ErrorMessage = "ErrorInvalidLength", MinimumLength = 2)]
        [RegularExpression(@"[^\s]+", ErrorMessage = "SpacesNotPermitted")]
        [Display(Name = "Name")]
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

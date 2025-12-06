using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
    }

    public class RoleCreateDto
    {
        [Required]
        [StringLength(256, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }

    public class RoleUpdateDto
    {
        [Required]
        [StringLength(256, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }

    public class RoleClaimDto
    {
        public int Id { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public string ClaimType { get; set; } = string.Empty;
        public string ClaimValue { get; set; } = string.Empty;
    }

    public class RoleClaimCreateDto
    {
        [Required]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string ClaimType { get; set; } = string.Empty;

        [Required]
        [StringLength(1024)]
        public string ClaimValue { get; set; } = string.Empty;
    }

    public class UserWithRolesDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class AssignRoleDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RoleIdOrName { get; set; } = string.Empty;
    }

    public class RemoveRoleDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RoleIdOrName { get; set; } = string.Empty;
    }
}

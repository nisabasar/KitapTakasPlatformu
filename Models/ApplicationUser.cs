using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KitapTakasPlatformu.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? SchoolNumber { get; set; }

        [StringLength(100)]
        public string? SchoolName { get; set; }
    }
}
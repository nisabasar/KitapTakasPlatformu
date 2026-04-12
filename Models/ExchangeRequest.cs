using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapTakasPlatformu.Models
{
    public class ExchangeRequest
    {
        public int Id { get; set; }

        [Required]
        public string RequesterId { get; set; } = string.Empty;

        [ForeignKey("RequesterId")]
        [ValidateNever]
        public ApplicationUser? Requester { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [ForeignKey("OwnerId")]
        [ValidateNever]
        public ApplicationUser? Owner { get; set; }

        [Required]
        public int RequestedBookId { get; set; }

        [ForeignKey("RequestedBookId")]
        [ValidateNever]
        public Book? RequestedBook { get; set; }

        [Required]
        public int OfferedBookId { get; set; }

        [ForeignKey("OfferedBookId")]
        [ValidateNever]
        public Book? OfferedBook { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Bekliyor";

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public DateTime? RespondedAt { get; set; }
    }
}
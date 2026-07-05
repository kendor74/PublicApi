using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Public.Application.DTO
{
    public sealed class SisosLoginRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; init; } = string.Empty;

        [Required]
        [MinLength(16)]
        [MaxLength(100)]
        public string Password { get; init; } = string.Empty;
    }
}

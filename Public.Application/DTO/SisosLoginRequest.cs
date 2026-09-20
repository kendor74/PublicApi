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
        public string Email { get; init; } = string.Empty;

        [Required]
        [MaxLength(16)]
        public string Password { get; init; } = string.Empty;
    }
}

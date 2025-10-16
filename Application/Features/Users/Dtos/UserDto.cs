using Application.Common.Dtos;
using Domain.Enums;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Dtos
{
    public record UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string? ProfilePictureUrl { get; set; }
        public string Gender { get; set; } = default!;
        public AddressDto? Address { get; set; }
        public bool IsActive { get; set; }
    }
}
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace TaskMarket.Dtos;

public class SendMessageRequestDto
{
    [Required, MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;

namespace OrderProcessing.Api.Dtos;

public class CreateOrderDto
{
    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "An order must have at least one item.")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
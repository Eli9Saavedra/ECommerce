using System.Collections.Generic;

namespace ECommerceAPI.Models
{
	public class Order
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public decimal Total { get; set; }
		public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // Ensure it's initialized
	}
}

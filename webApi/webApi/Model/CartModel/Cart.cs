using System;
using System.Collections.Generic;

namespace webApi.Model.CartModel
{
    public class Cart
    {
        public int CartId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
} 
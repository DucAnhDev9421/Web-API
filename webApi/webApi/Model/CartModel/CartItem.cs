using System;
using webApi.Model.CourseModel;

namespace webApi.Model.CartModel
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int CourseId { get; set; }
        public DateTime AddedAt { get; set; }
        
        public Cart Cart { get; set; }
        public courses Course { get; set; }
    }
} 
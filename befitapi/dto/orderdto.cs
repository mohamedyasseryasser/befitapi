namespace befitapi.dto
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; }
        public InvoiceDto Invoice { get; set; }
    }
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
    public class InvoiceDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal AmountDue { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}

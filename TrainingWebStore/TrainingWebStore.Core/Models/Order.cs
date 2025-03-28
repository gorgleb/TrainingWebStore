using System.ComponentModel.DataAnnotations.Schema;
using TrainingWebStore.Core.Enums;

namespace TrainingWebStore.Core.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public OrderStatus Status { get; set; }
        // Новые поля для скидок
        public decimal SubtotalAmount { get; set; } // Сумма без скидки
        public decimal DiscountPercentage { get; set; } // Процент скидки (5, 10, 15)
        public decimal DiscountAmount { get; set; } // Сумма скидки в деньгах


        [NotMapped] // Помечаем, что свойство не должно маппиться в БД
        public decimal TotalAmount
        {
            get { return SubtotalAmount - DiscountAmount; }
            set { } // Setter stores the value in a backing field
        }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

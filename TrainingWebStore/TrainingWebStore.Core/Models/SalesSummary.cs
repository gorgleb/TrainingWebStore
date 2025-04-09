using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingWebStore.Core.Models
{
    public class SalesSummary
    {
        public PeriodInfo Period { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal ConversionRate { get; set; }
        public ComparisonInfo Comparison { get; set; }
        public List<CategorySales> TopCategories { get; set; }
    }
}

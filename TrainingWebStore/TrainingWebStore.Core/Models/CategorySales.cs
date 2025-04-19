using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingWebStore.Core.Models
{
    public class CategorySales
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal SalesAmount { get; set; }
        public int OrdersCount { get; set; }
    }
}

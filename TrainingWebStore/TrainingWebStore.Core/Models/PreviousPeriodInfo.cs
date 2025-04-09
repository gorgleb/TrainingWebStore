using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingWebStore.Core.Models
{
    public class PreviousPeriodInfo
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
    }
}

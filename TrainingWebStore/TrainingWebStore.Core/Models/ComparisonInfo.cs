using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingWebStore.Core.Models
{
    public class ComparisonInfo
    {
        public PreviousPeriodInfo PreviousPeriod { get; set; }
        public decimal SalesGrowth { get; set; }
        public decimal OrdersGrowth { get; set; }
    }
}

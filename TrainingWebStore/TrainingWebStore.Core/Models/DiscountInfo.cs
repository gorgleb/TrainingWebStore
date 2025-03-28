using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingWebStore.Core.Models
{
    public class DiscountInfo
    {
        public decimal CurrentDiscountPercent { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AmountToNextLevel { get; set; }
        public decimal NextLevelThreshold { get; set; }
        public string DiscountTier { get; set; } = string.Empty;
    }
}

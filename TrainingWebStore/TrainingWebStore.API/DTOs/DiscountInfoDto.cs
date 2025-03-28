using System;
namespace TrainingWebStore.API.DTOs
{
    public class DiscountInfoDto
    {
        public decimal CurrentDiscountPercent { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AmountToNextLevel { get; set; }
        public decimal NextLevelThreshold { get; set; }
        public string DiscountTier { get; set; } = string.Empty; 
    }
}
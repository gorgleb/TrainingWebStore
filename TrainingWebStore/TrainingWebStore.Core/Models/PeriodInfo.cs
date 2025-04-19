using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingWebStore.Core.Models
{
    public class PeriodInfo
    {

        public PeriodInfo()
        {
                
        }
        public PeriodInfo(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

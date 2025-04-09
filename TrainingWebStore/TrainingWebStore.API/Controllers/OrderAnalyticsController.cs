using Microsoft.AspNetCore.Mvc;
using TrainingWebStore.API.DTOs;
using TrainingWebStore.Core.Models;
using TrainingWebStore.Core.Services;

namespace TrainingWebStore.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OrderAnalyticsController : ControllerBase
    {
        
           private readonly OrderAnalyticsService _orderAnalyticsService;

        // Конструктор с внедрением зависимости
        public OrderAnalyticsController(OrderAnalyticsService orderAnalyticsService)
        {
            _orderAnalyticsService = orderAnalyticsService ?? throw new ArgumentNullException(nameof(orderAnalyticsService));
        
        }

        [HttpGet("summary")]
        public async Task<ActionResult<SalesSummary>> GetSalesSummary(DateTime startDate , DateTime endDate,bool compareWithPrevious = false)
        {
            try
            {
                var result = await _orderAnalyticsService.GetSalesSummary(
                                  startDate ,
                                  endDate,
                                  compareWithPrevious);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
    }
}

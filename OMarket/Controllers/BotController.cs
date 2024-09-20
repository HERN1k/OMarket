using Microsoft.AspNetCore.Mvc;

using OMarket.Domain.Interfaces.Application.Services.Distributor;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;

using Telegram.Bot.Types;

namespace OMarket.Controllers
{
    [ApiController]
    [Route("api/bot")]
    public class BotController : ControllerBase
    {
        private readonly IUpdateManager _updateManager;

        private readonly IDistributorService _distributor;

        public BotController(
                IUpdateManager updateManager,
                IDistributorService distributor
            )
        {
            _updateManager = updateManager;
            _distributor = distributor;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Result = "Bot is running..." });
        }

        [HttpPost]
        public async Task<IActionResult> Post(Update update, CancellationToken token)
        {
            try
            {
                _updateManager.Update = update;

                await _distributor.Distribute(token);

                return Ok();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
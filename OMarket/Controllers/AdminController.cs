using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OMarket.Domain.DTOs;
using OMarket.Domain.Interfaces.Application.Services.Admin;

namespace OMarket.Controllers
{
    [ApiController]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("add-new-city")]
        public async Task<IActionResult> AddNewCity([FromBody] AddNewCityRequest request, CancellationToken token)
        {
            await _adminService.AddNewCityAsync(request, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("cities")]
        public async Task<IActionResult> Cities(CancellationToken token)
        {
            List<CityDto> cities = await _adminService.GetCitiesAsync(token);

            return Ok(new { Data = cities });
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("remove-city")]
        public async Task<IActionResult> RemoveCity([FromBody] RemoveCityRequest request, CancellationToken token)
        {
            await _adminService.RemoveCityAsync(request, token);

            return Ok();
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("add-new-store")]
        public async Task<IActionResult> AddNewStore([FromBody] AddNewStoreRequest request, CancellationToken token)
        {
            await _adminService.AddNewStoreAsync(request, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("stores")]
        public async Task<IActionResult> Stores(CancellationToken token)
        {
            List<StoreDtoResponse> stores = await _adminService.GetStoresAsync(token);

            return Ok(new { Data = stores });
        }


    }
}
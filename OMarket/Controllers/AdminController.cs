using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OMarket.Domain.DTOs;
using OMarket.Domain.Interfaces.Application.Services.Admin;

namespace OMarket.Controllers
{
    [ApiController]
    [Route("api/admin")]
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

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("remove-store")]
        public async Task<IActionResult> RemoveStore([FromBody] RemoveStoreRequest request, CancellationToken token)
        {
            await _adminService.RemoveStoreAsync(request, token);

            return Ok();
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("add-new-admin")]
        public async Task<IActionResult> AddNewAdmin([FromBody] AddNewAdminRequest request, CancellationToken token)
        {
            await _adminService.AddNewAdminAsync(request, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("admins")]
        public async Task<IActionResult> Admins(CancellationToken token)
        {
            List<AdminDtoResponse> admins = await _adminService.AdminsAsync(token);

            return Ok(new { Data = admins });
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("remove-admin")]
        public async Task<IActionResult> RemoveAdmin([FromBody] RemoveAdminRequest request, CancellationToken token)
        {
            await _adminService.RemoveAdminAsync(request, token);

            return Ok();
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("change-admin-password")]
        public async Task<IActionResult> ChangeAdminPassword([FromBody] ChangeAdminPasswordRequest request, CancellationToken token)
        {
            await _adminService.ChangeAdminPasswordAsync(request, token);

            return Ok();
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("change-city-name")]
        public async Task<IActionResult> ChangeCityName([FromBody] ChangeCityNameRequest request, CancellationToken token)
        {
            await _adminService.ChangeCityNameAsync(request, token);

            return Ok();
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("change-store-info")]
        public async Task<IActionResult> ChangeStoreInfo([FromBody] ChangeStoreInfoRequest request, CancellationToken token)
        {
            await _adminService.ChangeStoreInfoAsync(request, token);

            return Ok();
        }

        [Authorize]
        [HttpPost("change-store-info-base")]
        public async Task<IActionResult> ChangeStoreInfoBase([FromBody] ChangeStoreInfoBaseRequest request, CancellationToken token)
        {
            await _adminService.ChangeStoreInfoBaseAsync(HttpContext, request, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("store-review")]
        public async Task<IActionResult> StoreReview([FromQuery] Guid storeId, [FromQuery] int page, CancellationToken token)
        {
            ReviewResponse response = await _adminService.StoreReviewAsync(storeId, page, token);

            return Ok(new { Data = response });
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpGet("remove-store-review")]
        public async Task<IActionResult> RemoveStoreReview([FromQuery] Guid reviewId, CancellationToken token)
        {
            await _adminService.RemoveStoreReviewAsync(reviewId, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("block-customer-reviews")]
        public async Task<IActionResult> BlockCustomerReviews([FromQuery] long customerId, CancellationToken token)
        {
            await _adminService.BlockReviewsAsync(customerId, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("unblock-customer-reviews")]
        public async Task<IActionResult> UnBlockCustomerReviews([FromQuery] long customerId, CancellationToken token)
        {
            await _adminService.UnBlockReviewsAsync(customerId, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("block-customer-orders")]
        public async Task<IActionResult> BlockCustomerOrders([FromQuery] long customerId, CancellationToken token)
        {
            await _adminService.BlockOrdersAsync(customerId, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("unblock-customer-orders")]
        public async Task<IActionResult> UnBlockCustomerOrders([FromQuery] long customerId, CancellationToken token)
        {
            await _adminService.UnBlockOrdersAsync(customerId, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("get-customer-by-id")]
        public async Task<IActionResult> GetCustomerById([FromQuery] long customerId, CancellationToken token)
        {
            CustomerDtoResponse? result = await _adminService.GetCustomerByIdAsync(customerId, token);

            return Ok(new { Data = result });
        }

        [Authorize]
        [HttpGet("get-customer-by-phone-number")]
        public async Task<IActionResult> GetCustomerByPhoneNumber([FromQuery] string phoneNumber, CancellationToken token)
        {
            CustomerDtoResponse? result = await _adminService.GetCustomerByPhoneNumberAsync(phoneNumber, token);

            return Ok(new { Data = result });
        }

        [Authorize]
        [HttpGet("product-types")]
        public async Task<IActionResult> ProductTypes(CancellationToken token)
        {
            List<ProductTypesDto> result = await _adminService.ProductTypesAsync(token);

            return Ok(new { Data = result });
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("add-new-product")]
        public async Task<IActionResult> AddNewProduct([FromForm] IFormFile file, [FromForm] string metadata, CancellationToken token)
        {
            await _adminService.AddNewProductAsync(file, metadata, token);

            return Ok();
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpPost("change-product")]
        public async Task<IActionResult> ChangeProduct([FromForm] IFormFile? file, [FromForm] string metadata, CancellationToken token)
        {
            if (file != null)
            {
                await _adminService.ChangeProductAsync(file, metadata, token);
            }
            else
            {
                await _adminService.ChangeProductWithoutFileAsync(metadata, token);
            }

            return Ok();
        }

        [Authorize(Policy = "RequireSuperAdminRole")]
        [HttpGet("remove-product")]
        public async Task<IActionResult> RemoveProduct([FromQuery] Guid productId, CancellationToken token)
        {
            await _adminService.RemoveProductAsync(productId, token);

            return Ok();
        }

        [Authorize]
        [HttpGet("get-products")]
        public async Task<IActionResult> GetProducts([FromQuery] Guid typeId, [FromQuery] int page, CancellationToken token)
        {
            ProductResponse result = await _adminService.GetProductsAsync(typeId, page, token);

            return Ok(new { Data = result });
        }

        [Authorize]
        [HttpGet("get-products-with-store")]
        public async Task<IActionResult> GetProductsWithStore([FromQuery] Guid typeId, [FromQuery] int page, CancellationToken token)
        {
            ProductResponse result = await _adminService.GetProductsWithStoreAsync(HttpContext, typeId, page, token);

            return Ok(new { Data = result });
        }

        [Authorize]
        [HttpGet("change-data-store-product-status")]
        public async Task<IActionResult> ChangeDataStoreProductStatus([FromQuery] Guid productId, CancellationToken token)
        {
            await _adminService.ChangeDataStoreProductStatusAsync(HttpContext, productId, token);

            return Ok();
        }
    }
}
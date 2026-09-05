using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// PersonAddressController - HTTP layer; routes preserved under api/Person.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize]
[Route("api/Person")]
[ApiController]
public class PersonAddressController : ApiControllerBase
{
    private readonly IPersonAddressService _addressService;

    public PersonAddressController(IPersonAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpPost("CreateQuickAddress")]
    public async Task<IActionResult> CreateQuickAddress([FromForm] Guid familyguid, [FromForm] string addressType, [FromForm] string? villageOrCity, [FromForm] string? locationType, [FromForm] string address, [FromForm] string houseNumber, [FromForm] double latitude, [FromForm] double longitude, [FromForm] string city, [FromForm] string state, [FromForm] string country, [FromForm] string postalCode, [FromForm] string unionCouncil, [FromForm] string tehsil, [FromForm] string district, [FromForm] int bedrooms, [FromForm] int livingRooms, [FromForm] int hall, [FromForm] int kitchen, [FromForm] string houseType, [FromForm] string ownership, [FromForm] decimal? rent, [FromForm] decimal? deposit)
    {
        var result = await _addressService.CreateQuickAddress(familyguid, addressType, villageOrCity, locationType, address, houseNumber, latitude, longitude, city, state, country, postalCode, unionCouncil, tehsil, district, bedrooms, livingRooms, hall, kitchen, houseType, ownership, rent, deposit);
        return FromService(result);
    }

    [HttpGet("SearchAddressByHeadofthefamilyid")]
    public async Task<IActionResult> SearchAddressByHeadofthefamilyid([FromQuery] Guid FamilyGUID)
    {
        var result = await _addressService.SearchAddressByHeadofthefamilyid(FamilyGUID);
        return FromService(result);
    }
}

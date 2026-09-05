using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IPersonAddressService
{
    Task<PersonServiceResult> CreateQuickAddress(
            Guid familyguid,
            string addressType,
            string? villageOrCity,
            string? locationType,
            string address,
            string houseNumber,
            double latitude,
            double longitude,
            string city,
            string state,
            string country,
            string postalCode,
            string unionCouncil,
            string tehsil,
            string district,
            int bedrooms,
            int livingRooms,
            int hall,
            int kitchen,
            string houseType,
            string ownership,
            decimal? rent,
            decimal? deposit);

    Task<PersonServiceResult> SearchAddressByHeadofthefamilyid(Guid FamilyGUID);

}

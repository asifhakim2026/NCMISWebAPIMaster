using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NCMIS.Models;
using NCMISAPI.Data;
using NCMISAPI.Helpers;

namespace NCMISAPI.Services;

public class PersonAddressService : PersonServiceBase, IPersonAddressService
{
    public PersonAddressService(NcmisDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<PersonAddressService> logger, ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {
    }

    public async Task<PersonServiceResult> CreateQuickAddress(
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
            decimal? deposit)
    {
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(address)) validationErrors.Add("Address is required.");
        if (string.IsNullOrWhiteSpace(houseNumber)) validationErrors.Add("House Number is required.");
        if (string.IsNullOrWhiteSpace(city)) validationErrors.Add("City is required.");
        if (string.IsNullOrWhiteSpace(state)) validationErrors.Add("State is required.");
        if (string.IsNullOrWhiteSpace(country)) validationErrors.Add("Country is required.");
        if (string.IsNullOrWhiteSpace(postalCode)) validationErrors.Add("Postal Code is required.");
        if (string.IsNullOrWhiteSpace(unionCouncil)) validationErrors.Add("Union Council is required.");
        if (string.IsNullOrWhiteSpace(tehsil)) validationErrors.Add("Tehsil is required.");
        if (string.IsNullOrWhiteSpace(district)) validationErrors.Add("District is required.");
        if (bedrooms < 0) validationErrors.Add("Number of Bedrooms must be a non-negative number.");
        if (livingRooms < 0) validationErrors.Add("Living Rooms is required.");
        if (hall < 0) validationErrors.Add("Hall is required.");
        if (kitchen < 0) validationErrors.Add("Kitchen is required.");
        if (string.IsNullOrWhiteSpace(houseType)) validationErrors.Add("House Type is required.");
        if (string.IsNullOrWhiteSpace(ownership)) validationErrors.Add("Ownership is required.");

        if (ownership == "Rented")
        {
            if (rent == null || rent < 0) validationErrors.Add("Monthly Rent must be provided for rented houses.");
            if (deposit == null || deposit < 0) validationErrors.Add("Deposit must be provided for rented houses.");
        }

        if (validationErrors.Count > 0)
            return PersonServiceResult.BadRequest(FailResult(string.Join(", ", validationErrors)));

        var familyGroup = await _dbContext.FamilyGroups
            .FirstOrDefaultAsync(f => f.FamilyGroupGuid == familyguid);

        if (familyGroup == null)
            return PersonServiceResult.Ok(FailResult("Family group not found."));

        if (addressType == "Current" || addressType == "Permanent")
        {
            var existingActive = await _dbContext.PersonAddress
                .Where(a => a.PersonId == familyGroup.HeadPersonId && a.AddressType == addressType && a.IsActive)
                .ToListAsync();

            foreach (var old in existingActive)
            {
                old.IsActive = false;
                old.UpdatedDate = DateTime.Now;
                old.UpdatedBy = CurrentUserName;
            }
        }

        var personAddress = new PersonAddress
        {
            PersonId = (int)familyGroup.HeadPersonId!,
            AddressType = addressType,
            VillageOrCity = villageOrCity ?? "",
            LocationType = locationType ?? "",
            AddressLine1 = address,
            HouseOrFlatNumber = houseNumber,
            Latitude = latitude,
            Longitude = longitude,
            City = city,
            State = state,
            Country = country,
            PostalCode = postalCode,
            UnionCouncil = unionCouncil,
            TaluqaTehsil = tehsil,
            District = district,
            NumberofRooms = bedrooms,
            NumberofLivingRooms = livingRooms,
            NumberOfHall = hall,
            NumberofKitchen = kitchen,
            TypeofHouse = houseType,
            IsOwnedRented = ownership,
            MonthlyRent = rent,
            Deposit = deposit,
            CreatedDate = DateTime.Now,
            CreatedBy = CurrentUserName,
            IsActive = true,
            AddressGuid = Guid.NewGuid()
        };

        _dbContext.PersonAddress.Add(personAddress);
        await _dbContext.SaveChangesAsync();

        return PersonServiceResult.Ok(OkResult("Person and address saved successfully."));
    }

    public async Task<PersonServiceResult> SearchAddressByHeadofthefamilyid(Guid FamilyGUID)
    {
        var familyGroup = await _dbContext.FamilyGroups
            .FirstOrDefaultAsync(fg => fg.FamilyGroupGuid == FamilyGUID);

        if (familyGroup == null)
            return PersonServiceResult.Ok(FailResult("Family group not found"));

        var model = await (
            from pa in _dbContext.PersonAddress
            join p in _dbContext.PersonalInfos on pa.PersonId equals p.PersonId
            where pa.PersonId == familyGroup.HeadPersonId
            orderby pa.Id descending
            select new
            {
                FamilyGuid = FamilyGUID,
                PersonGuid = p.PersonalGuid,
                PersonId = p.PersonId,
                pa.VillageOrCity,
                pa.LocationType,
                pa.UnionCouncil,
                pa.TaluqaTehsil,
                pa.District,
                pa.NumberofRooms,
                pa.NumberOfHall,
                pa.NumberofKitchen,
                pa.NumberofLivingRooms,
                pa.TypeofHouse,
                pa.IsOwnedRented,
                pa.MonthlyRent,
                pa.Deposit,
                pa.AddressType,
                pa.AddressLine1,
                pa.AddressLine2,
                pa.City,
                pa.State,
                pa.Country,
                pa.PostalCode,
                pa.Latitude,
                pa.Longitude,
                pa.AddressGuid,
                FlatNumber = pa.HouseOrFlatNumber,
                pa.CreatedDate,
                pa.CreatedBy,
                pa.UpdatedDate,
                pa.UpdatedBy,
                pa.IsActive
            }).ToListAsync();

        return PersonServiceResult.Ok(OkResult("OK", model));
    }
}

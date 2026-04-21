using System.Text.Json;
using CaseyHub.API.Data;
using CaseyHub.API.ExternalClients;
using CaseyHub.API.Services;
using CaseyHub.Core.Entities;
using CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs.External;
using CaseyHub.Models.DTOs.Internal;
using Microsoft.EntityFrameworkCore;

public class PermitService(ICouncilDataClient councilDataClient, INominatimClient nominatimClient, CaseyHubDbContext dbContext, ILogger<PermitService> logger) : IPermitService
{
    public async Task<PermitDto?> GetPermitByAppNumberAsync (string applicationNumber)
    {
        Permit? permitEntity = await councilDataClient.FetchPermitFromAppNumberAsync(applicationNumber);
        if(permitEntity == null)
        {
            return null;
        }
        PermitDto? permitDto = new PermitDto(
            ApplicationNumber: permitEntity.ApplicationNumber,
            ApplicationCategory: permitEntity.ApplicationCategory,
            Description: permitEntity.Description,
            Status: permitEntity.Status,
            StageDecision: permitEntity.StageDecision,
            Address: permitEntity.Location?.RawAddress,
            LodgedDate: permitEntity.LodgedDate,
            DecisionDate: permitEntity.DecisionDate
        );
        return permitDto;
    }

    
    public async Task AddPermitByAppNumberToDBAsync(string applicationNumber)
        {
            logger.LogInformation("Starting 'AddPermitToDB' process for Application: {AppNumber}", applicationNumber);
            // 1. Fetch from Casey Council
            Permit? permitEntity = await councilDataClient.FetchPermitFromAppNumberAsync(applicationNumber);
            
            if (permitEntity == null)
            {
                logger.LogError("Failed to add permit: {AppNumber} not found in council records.", applicationNumber);
                throw new Exception($"Permit {applicationNumber} not found in council records.");
            }
            logger.LogInformation("Permit received from Casey: {PermitData}", JsonSerializer.Serialize(permitEntity));
            // 2. Fetch Geocode enrichment
            var addressToGeocode = permitEntity.Location?.RawAddress;
            
            if (!string.IsNullOrEmpty(addressToGeocode))
            {
                logger.LogInformation("Calling Nominatim to enrich address: {Address}", addressToGeocode);
                GeoCodeResponse? geoCode = await nominatimClient.EnrichAddressAsync(addressToGeocode, false);

                if (geoCode != null && geoCode.Address != null && permitEntity.Location != null)
                {
                    logger.LogInformation("GeoCode response received: {GeoResponse}", JsonSerializer.Serialize(geoCode));
                    permitEntity.Location.SetStructuredLocation(
                        geoCode.Address.HouseNumber,
                        geoCode.Address.Street,
                        geoCode.Address.Suburb,
                        geoCode.Address.Municipality,
                        geoCode.Address.Postcode,
                        geoCode.Address.State
                    );
                }
                else
                {
                    logger.LogWarning("Nominatim returned no valid results or empty address for: {Address}", addressToGeocode);
                }
            }
            else
            {
                logger.LogCritical("Raw Address from Council Database is null");
            }

            // 4. Save to Database
            try 
            {
                logger.LogInformation("Saving permit {AppNumber} to the database...", applicationNumber);
                await dbContext.Permits.AddAsync(permitEntity);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Successfully saved permit {AppNumber} to DB.", applicationNumber);
            }
            catch (Exception ex)
            {
                // Log database specific errors
                logger.LogCritical(ex, "Database error while saving permit {AppNumber}.", applicationNumber);
                throw new Exception($"Database error: {ex.Message}");
            }
    }

    public async Task GetEnrichSaveAllPermitsAsync()
    {
        try
        {
            logger.LogInformation("Attempting to get All Permits from Casey");
            //1. Get All Permits from Casey
            List<Permit> listOfPermits = await councilDataClient.GetAllPermits();
            if(listOfPermits.Count == 0)
            {
                logger.LogCritical("Get All Permits Returned null array");
            }
            logger.LogInformation("From Permit Service - Received a list of permits!");
            logger.LogInformation("Trynna send all the permits to Nominatim now");

            //2. Enrichment Process (+ save individual permit to db)
            foreach (Permit onePermit in listOfPermits)
            {
                var addressToGeocode = onePermit.Location?.RawAddress;
                if (!string.IsNullOrEmpty(addressToGeocode))
                {
                    logger.LogInformation("Calling Nominatim to enrich address: {Address}", addressToGeocode);
                    GeoCodeResponse? geoCode = await nominatimClient.EnrichAddressAsync(addressToGeocode, true);//using PRIVATE server because SO MANY RECORDS!

                    if (geoCode != null && geoCode.Address != null && onePermit.Location != null)
                    {
                        logger.LogInformation("GeoCode response received: {GeoResponse}", JsonSerializer.Serialize(geoCode));
                        onePermit.Location.SetStructuredLocation(
                            geoCode.Address.HouseNumber,
                            geoCode.Address.Street,
                            geoCode.Address.Suburb,
                            geoCode.Address.Municipality,
                            geoCode.Address.Postcode,
                            geoCode.Address.State
                        );
                        if (double.TryParse(geoCode.Latitude, out double lat) && double.TryParse(geoCode.Longitude, out double lon))
                        {
                            onePermit.Location.SetCoordinates(lat, lon);
                        }
                        
                    }
                    else
                    {
                        logger.LogWarning("Nominatim returned no valid results or empty address for: {Address}", addressToGeocode);
                    }
                    await dbContext.Permits.AddAsync(onePermit); //add to db regardless location received or not
                }
            }

            //3. Save all enriched Permits to DB now
            logger.LogInformation("Finished enrichment. Saving all records to DB...");
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Successfully saved all permits.");

        }catch(Exception ex)
        {
            logger.LogError(ex, "Error received.");
        }
    }

    public async Task SyncPermitsAsync()
    {
        try
        {
            logger.LogInformation("Starting Sync Process");
            List<Permit> allCouncilPermits = await councilDataClient.GetAllPermits();
            logger.LogInformation("All Permits received from Council Data Client");
            var existingPermits = await dbContext.Permits.ToDictionaryAsync(p=>p.ApplicationNumber);
            logger.LogInformation("All Permits received from internal DB");
            var newPermitsToSave = new List<Permit>();
            int updatedCount = 0;
            int createdCount = 0;
            foreach (var oneCouncilPermit in allCouncilPermits)
            {
                if(existingPermits.TryGetValue(oneCouncilPermit.ApplicationNumber, out var dbPermit))
                {
                    //Record exists, now update. EF core only updates if there are changes.
                    dbPermit.UpdateDetails(
                        oneCouncilPermit.ApplicationCategory,
                        oneCouncilPermit.Description,
                        oneCouncilPermit.Status,
                        oneCouncilPermit.StageDecision,
                        oneCouncilPermit.LodgedDate,
                        oneCouncilPermit.DecisionDate
                    );
                    updatedCount++;
                }
                else
                {
                    //That permit wasnt found in our db
                    var addressToGeocode = oneCouncilPermit.Location?.RawAddress;
                    if (!string.IsNullOrWhiteSpace(addressToGeocode)){
                        GeoCodeResponse? geoCode = await nominatimClient.EnrichAddressAsync(addressToGeocode, false); //using public NOMINATIM here
                        logger.LogInformation("GeoCode response received: {GeoResponse}", JsonSerializer.Serialize(geoCode));
                        if(geoCode != null && geoCode.Address != null && oneCouncilPermit.Location != null)
                        {
                            oneCouncilPermit.Location.SetStructuredLocation(
                                geoCode.Address?.HouseNumber,
                                geoCode.Address?.Street,
                                geoCode.Address?.Suburb,
                                geoCode.Address?.Municipality,
                                geoCode.Address?.Postcode,
                                geoCode.Address?.State
                            );
                            if (double.TryParse(geoCode.Latitude, out double lat) && double.TryParse(geoCode.Longitude, out double lon))
                            {
                                oneCouncilPermit.Location.SetCoordinates(lat, lon);
                            }
                        }    
                    }
                    newPermitsToSave.Add(oneCouncilPermit);
                    createdCount++;
                }

            }

            if (newPermitsToSave.Count > 0)
            {
                logger.LogInformation("Found {Count} NEW Permits. Adding to DB", createdCount);
                await dbContext.Permits.AddRangeAsync(newPermitsToSave);
            }

            int dbChanges = await dbContext.SaveChangesAsync();
            logger.LogInformation("Updated records in DB: {updatedCount}", updatedCount);
            logger.LogInformation("Sync Complete! Database rows affected: {Rows}", dbChanges);
        }catch(Exception ex)
        {
            logger.LogError(ex, "An Error Occured.");
        }
    }
}


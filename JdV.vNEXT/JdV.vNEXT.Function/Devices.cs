using JdV.vNEXT.Function.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Polly;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JdV.vNEXT.Function
{
    public static class Devices
    {
        [FunctionName("RegisterDevices")]
        [OpenApiOperation(operationId: "Run")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(DeviceRequestModel), Description = "JSON request body { correlationId, devices }")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response with message.")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "devices")] HttpRequest req,
            ILogger log,
            [Sql("dbo.Devices", "SqlConnectionString")] IAsyncCollector<DeviceEntity> devices)
        {
            try
            {
                log.LogInformation($"RegisterDevices function received a request to register devices... ");

                // Get request body data
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var requestData = JsonConvert.DeserializeObject<DeviceRequestModel>(requestBody);

                // NOTES: For this exercise, I've just directly implemented a simple retry policy here.
                var retryPolicy = Policy
                    .Handle<HttpRequestException>() // Retry and handle HttpRequestException
                    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // Retry and handle non-successful HTTP responses
                    .WaitAndRetryAsync(
                        retryCount: 3, // Number of retry attempts
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                        onRetry: (exception, timespan, retryAttempt, context) =>
                        {
                            // Log retries as warning
                            log.LogWarning($"Get Asset Id API request retry attempt {retryAttempt} after {timespan.TotalSeconds} seconds");
                        }
                    );

                // Initialise HTTP client to get the asset Ids
                using HttpClient client = new() { BaseAddress = new Uri("http://tech-assessment.vnext.com.au/api/") };
                client.DefaultRequestHeaders.Add("x-functions-key", "DRefJc8eEDyJzS19qYAKopSyWW8ijoJe8zcFhH5J1lhFtChC56ZOKQ==");

                // Build request content to get asset Ids
                var devideIds = requestData?.Devices?.Select(e => e.Id).ToList();
                using StringContent jsonContent = new(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        deviceIds = devideIds
                    }),
                    Encoding.UTF8,
                    "application/json");

                log.LogInformation("Initiating API request to get asset Id(s) of the device(s) to be registered...");

                // Execute API request with retry in case of transient failures
                using HttpResponseMessage apiResponse = await retryPolicy.ExecuteAsync(async () =>
                {
                    return await client.PostAsync("devices/assetId", jsonContent);
                });

                log.LogInformation("API request to get asset Ids has completed.");

                // Parse API response
                var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
                var apiResponseData = JsonConvert.DeserializeObject<DeviceAssetsResponseModel>(jsonResponse);

                // Build device entity with asset Ids
                var devicesToRegister = requestData?.Devices?
                    .Join(apiResponseData?.Devices,
                        d => d.Id,
                        da => da.DeviceId,
                        (d, da) => new DeviceEntity
                        {
                            DeviceId = d.Id,
                            Name = d.Name,
                            Location = d.Location,
                            Type = d.Type,
                            AssetId = da.AssetId
                        })
                    .ToList();

                log.LogInformation($"Registering {devicesToRegister.Count} device(s) into the database...");

                // Register devices into the database
                // NOTES: Using SQL Binding to directly upsert data
                foreach (var device in devicesToRegister)
                    await devices.AddAsync(device);

                // Flush / update bindings to save the changes into the database
                await devices.FlushAsync();

                log.LogInformation($"Successfully registered {devicesToRegister.Count} device(s).");

                return new OkObjectResult("Device registration completed");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occured while registering the devices");

                // NOTES: For this exercise, just return the exception message. Best practice is to return a custom result based on agreed upon specifications
                return new BadRequestObjectResult($"The following error occured while registering the device(s): {ex.Message}");
            }
        }
    }
}
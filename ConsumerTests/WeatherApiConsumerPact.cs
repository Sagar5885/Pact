using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace ConsumerTests
{
	public class WeatherApiConsumerPact : IDisposable
    {
        private IMockProviderService _mockProviderService;
        private string _mockProviderServiceBaseUri;

        public WeatherApiConsumerPact()
        {
            _mockProviderService = new MockProviderService(5019, false, "consTest", "ProTest", new PactNet.PactConfig(), PactNet.Models.IPAddress.Loopback);
            _mockProviderService.ClearInteractions(); // Clears any previously registered interactions before the test is run
            _mockProviderServiceBaseUri = _mockProviderServiceBaseUri = "http://localhost:9222";
        }

        [Fact]
        public void ItHandlesValidRequest()
        {
            // Arrange
            _mockProviderService
              .UponReceiving("A valid request for WeatherForecast")
              .With(new ProviderServiceRequest
              {
                  Method = HttpVerb.Get,
                  Path = "/WeatherForecast"
              })
              .WillRespondWith(new ProviderServiceResponse
              {
                  Status = 200,
                  Headers = new Dictionary<string, object>
                  {
                  { "Content-Type", "application/json; charset=utf-8" }
                  },
                  Body = new[]
                  {
                  new
                  {
                      date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                      temperatureC = 25,
                      summary = "Mild",
                      temperatureF = 77
                  }
                  }
              });

            // Act / Assert
            var consumer = new WeatherApiClient(_mockProviderServiceBaseUri);
            var result = consumer.GetWeatherForecast();

            Assert.NotNull(result);
            Assert.Equal(25, result[0].TemperatureC);
            Assert.Equal("Mild", result[0].Summary);

            _mockProviderService.VerifyInteractions(); // Verifies that interactions registered have occurred
        }

        public void Dispose()
        {
            _mockProviderService.Stop(); // Stop the mock service
        }
    }

    public class WeatherApiClient
    {
        private readonly string _baseUri;

        public WeatherApiClient(string baseUri)
        {
            _baseUri = baseUri;
        }

        public List<WeatherForecast> GetWeatherForecast()
        {
            var request = (HttpWebRequest)WebRequest.Create($"{_baseUri}/WeatherForecast");
            request.Method = "GET";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var stream = new StreamReader(response.GetResponseStream()))
                {
                    var json = stream.ReadToEnd();
                    return JsonConvert.DeserializeObject<List<WeatherForecast>>(json);
                }
            }
        }
    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}


using Xunit;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Infrastructure.Outputters;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet;
using Xunit.Sdk;

namespace ProviderVerificationTests
{
	public class WeatherApiProviderPact
    {
        [Fact]
        public void EnsureWeatherApiHonoursPactWithConsumer()
        {
            //Arrange
            var config = new PactVerifierConfig
            {
                Outputters = new List<IOutput>
            {
                new XUnitOutput(new TestOutputHelper())
            },
                Verbose = true //Output verbose verification logs to the test output
            };

            IPactVerifier pactVerifier = new PactVerifier(config);
            pactVerifier.ProviderState($"{_providerUri}/provider-states");

            //Act / Assert
            pactVerifier
              .ServiceProvider("WeatherApi", _providerUri)
              .HonoursPactWith("Consumer")
              .PactUri("..\\..\\..\\pacts\\consumer-weatherapi.json")
              .Verify();
        }
    }
}


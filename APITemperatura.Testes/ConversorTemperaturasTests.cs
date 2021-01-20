using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;
using APITemperatura.Testes.Models;

namespace APITemperatura.Testes
{
    public class ConversorTemperaturasTests
    {
        private readonly HttpClient _client;
        private readonly string _endpointConvFahrenheit;

        public ConversorTemperaturasTests()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json")
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            _client = new ();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _endpointConvFahrenheit = configuration["EndpointConvFahrenheit"];
        }

        [Theory]
        [InlineData(32, 0, 273.15)]
        [InlineData(86, 30, 303.15)]
        [InlineData(47, 8.33, 281.48)]
        [InlineData(90.5, 32.5, 305.65)]
        [InlineData(120.18, 48.99, 322.14)]
        [InlineData(212, 100, 373.15)]
        [InlineData(-459.67, -273.15, 0)]
        public async Task TestarConversoesValidas(
            double vlFahrenheit,
            double vlCelsius,
            double vlKelvin)
        {
            var responseMessage = await EnviarRequisicao(vlFahrenheit);
            responseMessage.StatusCode.Should().Be(HttpStatusCode.OK,
                $"* Ocorreu uma falha: Status Code esperado (200, OK) diferente do resultado gerado *");
            
            var resultado =
                await responseMessage.Content.ReadFromJsonAsync<Temperatura>();
            resultado.Celsius.Should().Be(vlCelsius,
                "* Ocorreu uma falha: os valores na escala Celsius nao correspondem *");
            resultado.Kelvin.Should().Be(vlKelvin,
                "* Ocorreu uma falha: os valores na escala Kelvin nao correspondem *");
        }

        [Theory]
        [InlineData(-459.68)]
        [InlineData(-500)]
        [InlineData(-1000.99)]
        public async Task TestarFalhas(double vlFahrenheit)
        {
            var responseMessage = await EnviarRequisicao(vlFahrenheit);

            responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                "* Ocorreu uma falha: Status Code esperado para a temperatura de" +
               $"{vlFahrenheit} graus Fahrenheit: 400 (Bad Request) *");
        }

        private async Task<HttpResponseMessage> EnviarRequisicao(double vlFahrenheit)
        {
            return await _client.GetAsync(
                _endpointConvFahrenheit + JsonSerializer.Serialize(vlFahrenheit));
        }
    }
}
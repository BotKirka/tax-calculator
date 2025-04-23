using Microsoft.Extensions.Configuration;

namespace TaxCalculatorDesktop.Helpers
{
    public static class Mime
    {
        public static readonly IConfigurationRoot ConfigurationFile = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();
    }
}

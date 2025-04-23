using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TaxCalculatorDesktop.Helpers;
using TaxCalculatorDesktop.Models.Enums;

namespace TaxCalculatorDesktop
{
    [ObservableObject]
    partial class MainWindowViewModel
    {
        private readonly decimal _pensionPercentByEmployee;
        //private readonly decimal _pensionPercentByEmployer;
        private readonly decimal _educationPercentByEmployee;
        //private readonly decimal _educationPercentByEmployer;
        private readonly decimal _baseNekudValue;
        private readonly decimal _baseCountNekudot;
        private readonly decimal _aditionalNekudotForWomen;

        [ObservableProperty]
        private decimal _brutto;

        [ObservableProperty]
        private decimal _netto;

        [ObservableProperty]
        private PensionEnum _pensionType;

        [ObservableProperty]
        private SexEnum _sexType;

        [ObservableProperty]
        private bool _haveEducationFond;

        public MainWindowViewModel()
        {
            _pensionPercentByEmployee = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:PensionPercentByEmployee").Value!);

            _educationPercentByEmployee = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:EducationFondPercentByEmployee").Value!);

            _baseNekudValue = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:BaseNekudValue").Value!);

            _baseCountNekudot = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:BaseCountNekudot").Value!);

            _aditionalNekudotForWomen = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:AditionalNekudotForWomen").Value!);
        }

        [RelayCommand(CanExecute = nameof(CanCalculateNetto))]
        public async Task CalculateNetto()
        {
            var taxBase = CalculateTaxBase();


        }

        private decimal CalculateTaxBase()
        {
            decimal educationTax = 0;
            if (HaveEducationFond) educationTax = _educationPercentByEmployee;

            decimal pensionTax = _pensionPercentByEmployee;
            if (PensionType is PensionEnum.EightyPecent) pensionTax *= 0.8m;

            // for future things
            /*var moneyToEducation = Brutto * educationTax;
            var moneyToPension = Brutto * pensionTax;*/

            return Brutto * (1 - educationTax - pensionTax);
        }

        private bool CanCalculateNetto()
        {
            if ( Brutto > 0 ) return true;

            return false;
        }
    }
}

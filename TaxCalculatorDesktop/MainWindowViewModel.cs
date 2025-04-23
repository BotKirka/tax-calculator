using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TaxCalculatorDesktop.Helpers;
using TaxCalculatorDesktop.Models;
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

        private readonly Dictionary<string, TaxLevel> _taxLevels;
        private readonly TwoLevelTax _bituahLeumiTaxes;
        private readonly TwoLevelTax _kupatHolimTaxes;

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

            _bituahLeumiTaxes = Mime.ConfigurationFile.GetSection("TaxData:BituahLeumi").Get<TwoLevelTax>()!;

            _kupatHolimTaxes = Mime.ConfigurationFile.GetSection("TaxData:KupatHolim").Get<TwoLevelTax>()!;

            _taxLevels = Mime.ConfigurationFile.GetSection("TaxData:IncomeTaxLevels").Get<Dictionary<string, TaxLevel>>()!;
        }

        [RelayCommand(CanExecute = nameof(CanCalculateNetto))]
        public async Task CalculateNetto()
        {
            var taxBase = CalculateTaxBase();

            var taxesPerLevel = CalculateIncomeTaxPerLevel(taxBase, _taxLevels);
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
            if (Brutto > 0) return true;

            return false;
        }

        public Dictionary<string, decimal> CalculateIncomeTaxPerLevel(decimal taxBase, Dictionary<string, TaxLevel> taxLevels)
        {
            var orderedLevels = taxLevels
                .OrderBy(kvp => kvp.Value.Threshold ?? decimal.MaxValue)
                .ToList();

            var result = new Dictionary<string, decimal>();
            decimal previousThreshold = 0;

            foreach (var kvp in orderedLevels)
            {
                string levelName = kvp.Key;
                var level = kvp.Value;

                decimal upperLimit = level.Threshold ?? decimal.MaxValue;
                decimal taxableValueAtThisLevel = Math.Min(taxBase, upperLimit) - previousThreshold;

                if (taxableValueAtThisLevel > 0)
                {
                    decimal tax = taxableValueAtThisLevel * level.Rate;
                    result[levelName] = Math.Round(tax, 3);
                }
                else
                {
                    result[levelName] = 0;
                }

                if (taxBase <= upperLimit)
                    break;

                previousThreshold = upperLimit;
            }

            return result;
        }
    }
}
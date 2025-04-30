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
        private readonly decimal _pensionPercentByEmployer;
        private readonly decimal _educationPercentByEmployee;
        private readonly decimal _educationPercentByEmployer;
        private readonly decimal _baseNekudValue;
        private readonly decimal _baseCountNekudot;
        private readonly decimal _aditionalNekudotForWomen;

        private readonly Dictionary<string, TaxLevel> _taxLevels;
        private readonly TwoLevelTax _bituahLeumiTaxes;
        private readonly TwoLevelTax _kupatHolimTaxes;

        private readonly int _accuracy;

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

        [ObservableProperty] private decimal _educationByEmployee;
        [ObservableProperty] private decimal _educationByEmployer;
        [ObservableProperty] private decimal _pensionByEmployee;
        [ObservableProperty] private decimal _pensionByEmployer;
        [ObservableProperty] private decimal _bituahLeumi;
        [ObservableProperty] private decimal _kupatHolim;
        [ObservableProperty] private decimal _incomeTaxes;
        [ObservableProperty] private decimal _nekudot;

        public MainWindowViewModel()
        {
            _pensionPercentByEmployee = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:PensionPercentByEmployee").Value!);

            _pensionPercentByEmployer = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:PensionPercentByEmployer").Value!);

            _educationPercentByEmployee = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:EducationFondPercentByEmployee").Value!);

            _educationPercentByEmployer = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:EducationFondPercentByEmployer").Value!);

            _baseNekudValue = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:BaseNekudValue").Value!);

            _baseCountNekudot = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:BaseCountNekudot").Value!);

            _aditionalNekudotForWomen = JsonConvert.DeserializeObject<decimal>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:AditionalNekudotForWomen").Value!);

            _bituahLeumiTaxes = Mime.ConfigurationFile.GetSection("TaxData:BituahLeumi").Get<TwoLevelTax>()!;

            _kupatHolimTaxes = Mime.ConfigurationFile.GetSection("TaxData:KupatHolim").Get<TwoLevelTax>()!;

            _taxLevels = Mime.ConfigurationFile.GetSection("TaxData:IncomeTaxLevels").Get<Dictionary<string, TaxLevel>>()!;

            _accuracy = JsonConvert.DeserializeObject<int>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:Accuracy").Value!);
        }

        [RelayCommand(CanExecute = nameof(CanCalculateNetto))]
        public void CalculateNetto()
        {
            var taxBase = CalculateTaxBase();

            var taxesPerLevel = CalculateIncomeTaxPerLevel(taxBase, _taxLevels);

            var bituahLeumi = CalculateTwoLevelTaxes(taxBase, _bituahLeumiTaxes);

            var kupatHolim = CalculateTwoLevelTaxes(taxBase, _kupatHolimTaxes);

            IncomeTaxes = taxesPerLevel.Last().Value;
            BituahLeumi = bituahLeumi.Last().Value;
            KupatHolim = kupatHolim.Last().Value;

            Nekudot = CalculateNekudot();
            if(Nekudot > IncomeTaxes) Nekudot = IncomeTaxes;

            Netto = Math.Round(taxBase - IncomeTaxes - BituahLeumi - KupatHolim + Nekudot, _accuracy);
        }
        
        private decimal CalculateTaxBase()
        {
            var moneyToEducation = CalculateEducationTaxes();
            var moneyToPension = CalculatePension();

            var taxBase = Brutto - moneyToEducation.TaxByEmployee - moneyToPension.TaxByEmployee;

            return taxBase;
        }

        private TwoSideTax CalculatePension()
        {
            var pensionTax = _pensionPercentByEmployee;
            if (PensionType is PensionEnum.EightyPecent) pensionTax *= 0.8m;

            PensionByEmployee = Brutto * pensionTax;
            PensionByEmployer = Brutto * _pensionPercentByEmployer;

            return new TwoSideTax() { TaxByEmployee = PensionByEmployee, TaxByEmployer = PensionByEmployer };
        }

        private TwoSideTax CalculateEducationTaxes()
        {
            if (!HaveEducationFond) return new TwoSideTax() { TaxByEmployee = 0, TaxByEmployer = 0 };

            EducationByEmployee = Brutto * _educationPercentByEmployee;
            EducationByEmployer = Brutto * _educationPercentByEmployer;

            return new TwoSideTax() { TaxByEmployee = EducationByEmployee, TaxByEmployer = EducationByEmployer };
        }

        private decimal CalculateNekudot()
        {
            var nekudotCount = _baseCountNekudot;
            if (SexType is SexEnum.Woman) nekudotCount += _aditionalNekudotForWomen;

            return _baseNekudValue * nekudotCount;
        }

        public Dictionary<string, decimal> CalculateIncomeTaxPerLevel(decimal taxBase, Dictionary<string, TaxLevel> taxLevels)
        {
            var orderedLevels = taxLevels
                .OrderBy(kvp => kvp.Value.Threshold ?? decimal.MaxValue)
                .ToList();

            var result = new Dictionary<string, decimal>();
            decimal previousThreshold = 0;

            var sumOfLevels = 0m;
            foreach (var kvp in orderedLevels)
            {
                string levelName = kvp.Key;
                var level = kvp.Value;

                decimal upperLimit = level.Threshold ?? decimal.MaxValue;
                decimal taxableValueAtThisLevel = Math.Min(taxBase, upperLimit) - previousThreshold;

                if (taxableValueAtThisLevel > 0)
                {
                    decimal tax = taxableValueAtThisLevel * level.Rate;
                    result[levelName] = Math.Round(tax, _accuracy);

                    sumOfLevels += tax;
                }
                else
                {
                    result[levelName] = 0;
                }

                if (taxBase <= upperLimit)
                    break;

                previousThreshold = upperLimit;
            }

            result[nameof(sumOfLevels)] = sumOfLevels;

            return result;
        }

        private Dictionary<string, decimal> CalculateTwoLevelTaxes(decimal taxBase, TwoLevelTax twoLevelTax)
        {
            var result = new Dictionary<string, decimal>();

            decimal amountLower = Math.Min(taxBase, twoLevelTax.LowerThreshold);
            decimal amountUpper = Math.Max(0, taxBase - twoLevelTax.LowerThreshold);

            decimal taxLower = Math.Round(amountLower * twoLevelTax.LowerRate, _accuracy);
            decimal taxUpper = Math.Round(amountUpper * twoLevelTax.UpperRate, _accuracy);

            result[nameof(taxLower)] = taxLower;
            result[nameof(taxUpper)] = taxUpper;

            var sumOfLevels = taxLower + taxUpper;
            result[nameof(sumOfLevels)] = sumOfLevels;

            return result;
        }

        private bool CanCalculateNetto()
        {
            if (Brutto > 0) return true;

            return false;
        }
    }
}
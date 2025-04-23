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
        private readonly float _pensionPercentByEmployee;
        //private readonly float _pensionPercentByEmployer;
        private readonly float _educationPercentByEmployee;
        //private readonly float _educationPercentByEmployer;
        private readonly int _baseNekudValue;
        private readonly float _baseCountNekudot;
        private readonly float _aditionalNekudotForWomen;

        [ObservableProperty]
        private float _brutto;

        [ObservableProperty]
        private float _netto;

        [ObservableProperty]
        private PensionEnum _pensionType;

        [ObservableProperty]
        private SexEnum _sexType;

        [ObservableProperty]
        private bool _haveEducationFond;

        public MainWindowViewModel()
        {
            _pensionPercentByEmployee = JsonConvert.DeserializeObject<float>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:PensionPercentByEmployee").Value!);

            _educationPercentByEmployee = JsonConvert.DeserializeObject<float>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:EducationFondPercentByEmployee").Value!);

            _baseNekudValue = JsonConvert.DeserializeObject<int>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:BaseNekudValue").Value!);

            _baseCountNekudot = JsonConvert.DeserializeObject<float>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:BaseCountNekudot").Value!);

            _aditionalNekudotForWomen = JsonConvert.DeserializeObject<float>
                (Mime.ConfigurationFile.GetRequiredSection("TaxData:AditionalNekudotForWomen").Value!);
        }

        /// <summary>
        /// Нужно добавить такую тему чтобы запомнил все подсчеты для будущих подсчетов и для вывода на экран
        /// Т.е. сколько в пенсию ушло, сколько в фонд и тд
        /// Еще продумать подсчеты со стороны работодателя
        /// </summary>
        /// <returns></returns>

        [RelayCommand(CanExecute = nameof(CanCalculateNetto))]
        public async Task CalculateNetto()
        {
            var taxBase = CalculateTaxBase();
        }

        private float CalculateTaxBase()
        {
            float educationTax = 0;
            if (HaveEducationFond) educationTax = _educationPercentByEmployee;//0.0025

            float pensionTax = _pensionPercentByEmployee;
            if (PensionType is PensionEnum.EightyPecent) pensionTax *= 0.8f;//0.06*0.8

            // for future things
            /*var moneyToEducation = Brutto * educationTax;
            var moneyToPension = Brutto * pensionTax;*/

            return (float)(Brutto * (1 - educationTax - pensionTax));
        }

        private bool CanCalculateNetto()
        {
            if ( Brutto > 0 ) return true;

            return false;
        }
    }
}

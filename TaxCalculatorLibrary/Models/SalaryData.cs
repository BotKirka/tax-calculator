using TaxCalculatorLibrary.Models.Enums;

namespace TaxCalculatorLibrary.Models
{
    public class SalaryData
    {
        public decimal Brutto { get; set; }

        public bool HaveEducationFond { get; set; }

        public PensionEnum Pension { get; set; }

        public SexEnum Sex { get; set; }
    }
}

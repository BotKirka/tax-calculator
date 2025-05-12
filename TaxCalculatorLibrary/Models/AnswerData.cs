namespace TaxCalculatorLibrary.Models
{
    public class AnswerData
    {
        public decimal Netto { get; set; }
        public TwoSideTax PensionTaxes { get; set; } = new TwoSideTax();
        public TwoSideTax EducationFondTaxes { get; set; } = new TwoSideTax();
    }
}

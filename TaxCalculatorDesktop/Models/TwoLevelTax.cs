namespace TaxCalculatorDesktop.Models
{
    public class TwoLevelTax
    {
        public decimal LowerThreshold { get; set; }
        public decimal LowerRate { get; set; }
        public decimal UpperRate { get; set; }
    }
}

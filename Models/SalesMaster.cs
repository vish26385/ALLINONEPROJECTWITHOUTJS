using System.ComponentModel.DataAnnotations.Schema;

namespace ALLINONEPROJECTWITHOUTJS.Models
{
    public class SalesMaster
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; }
        public int DueDays { get; set; }
        public DateTime DueDate { get; set; }
        [ForeignKey("PartyMaster")]
        public int PartyId { get; set; }
        public PartyMaster PartyMaster { get; set; }
        public List<SalesDetail> salesDetails { get; set; } = new List<SalesDetail>();

        [NotMapped]
        public string? CalcSource { get; set; }
    }
}

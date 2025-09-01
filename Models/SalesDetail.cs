using System.ComponentModel.DataAnnotations.Schema;

namespace ALLINONEPROJECTWITHOUTJS.Models
{
    public class SalesDetail
    {
        public int Id { get; set; }
        [ForeignKey("SalesMaster")]
        public int SaleMasterId { get; set; }
        [ForeignKey("ItemMaster")]
        public int ItemId { get; set; }
        public int Qty { get; set; }
        public ItemMaster ItemMaster { get; set; }
        public SalesMaster SalesMaster { get; set; }

        [NotMapped]
        public decimal Rate { get; set; }

        [NotMapped]
        public decimal Amount { get; set; }
    }
}

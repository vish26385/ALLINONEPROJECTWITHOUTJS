using System.ComponentModel.DataAnnotations.Schema;

namespace ALLINONEPROJECTWITHOUTJS.Models
{
    public class Cart
    {
        public int Id { get; set; }
        [ForeignKey("ItemMaster")]
        public int ItemId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public ItemMaster ItemMaster { get; set; }
        public User User { get; set; }
    }
}

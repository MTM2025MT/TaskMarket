using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models
{
    public class ContractorSkill
    {

        public int ContractorId { get; set; }

        [ForeignKey("ContractorId")]
        public virtual Contractor Contractor { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual ServiceCategory ServiceCategory { get; set; }
    
   }
}

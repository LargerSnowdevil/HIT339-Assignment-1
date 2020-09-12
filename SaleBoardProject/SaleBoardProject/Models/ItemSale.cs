using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaleBoardProject.Models
{
    public class ItemSale
    {
        [Key]
        public int itemSaleID { get; set; }

        public int transactionID { get; set; }

        public Item item { get; set; }

        [Display(Name = "Quantity Bought")]
        public int quantity { get; set; }
    }
}

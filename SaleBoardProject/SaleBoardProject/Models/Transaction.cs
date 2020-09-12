using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaleBoardProject.Models
{
    public class Transaction
    {
        public int transactionID { get; set; }

        public List<ItemSale> sales { get; set; }

        [Display(Name = "Buyer Name")]
        public String buyer { get; set; }

        [Display(Name = "Total Cost")]
        [DataType(DataType.Currency)]
        public double price { get; set; }
    }
}

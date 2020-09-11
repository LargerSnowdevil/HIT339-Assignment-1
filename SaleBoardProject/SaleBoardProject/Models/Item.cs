using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaleBoardProject.Models
{
    public class Item
    {
        public int ID { get; set; }
        [Display(Name = "Item Name")]
        public String name { get; set; }
        [Display(Name = "Description")]
        public String description { get; set; }
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public double price { get; set; }
        [Display(Name = "Quantity Remaining")]
        public int Quantity { get; set; }
        [Display(Name = "Seller Name")]
        public String seller { get; set; }
    }
}

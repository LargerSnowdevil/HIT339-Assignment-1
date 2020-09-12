using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaleBoardProject.Models
{
    public class UserHomeViewModel
    {
        public List<Item> itemsForSale { get; set; }

        //----------------------------------------------

        public List<UserSpending> transactionHistory { get; set; }

    }

    public class UserSpending
    {
        [Display(Name = "Buyer Name")]
        public String buyer { get; set; }

        [Display(Name = "Total Spendings")]
        [DataType(DataType.Currency)]
        public double Spending { get; set; }
    }
}

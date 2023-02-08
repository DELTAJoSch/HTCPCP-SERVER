using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTCPCP_Server.Enumerations
{
    /// <summary>
    /// Describes all types of additions
    /// </summary>
    public enum Option
    {
        [Display(Name = "Milk/Cream")] MilkCream,
        [Display(Name = "Milk/Half-and-Half")] MilkHalfAndHalf,
        [Display(Name = "Milk/Whole-milk")] MilkWholeMilk,
        [Display(Name = "Milk/Part-skim")] MilkPartSkim,
        [Display(Name = "Milk/Skim")] MilkSkim,
        [Display(Name = "Milk/Non-dairy")] MilkNonDairy,
        [Display(Name = "Syrup/Vanilla")] SyrupVanilla,
        [Display(Name = "Syrup/Almond")] SyrupAlmond,
        [Display(Name = "Syrup/Raspberry")] SyrupRaspberry,
        [Display(Name = "Syrup/Chocolate")] SyrupChocolate,
        [Display(Name = "Alcohol/Whisky")] AlcoholWhisky,
        [Display(Name = "Alcohol/Rum")] AlcoholRum,
        [Display(Name = "Alcohol/Kahlua")] AlcoholKahlua,
        [Display(Name = "Alcohol/Aquavit")] AlcoholAquavit,
        [Display(Name = "Coffee/Coffee")] Coffee
    }
}

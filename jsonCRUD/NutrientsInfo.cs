using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Var_WebCrawler_CRUD
{
    public class NutrientsInfo
    {
        public string NutrientName { get; set; } = "";
        public string FoodCode { get; set; } = "";
        public string Category { get; set; } = "";
        public string ValueFor100g { get; set; } = "";
        public string Procedures { get; set; } = "";
        public string DataSource { get; set; } = "";
        public string Reference { get; set; } = "";
    }
}

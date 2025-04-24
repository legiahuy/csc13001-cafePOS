using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafePOS.Helpers
{
    public static class WeatherDrinkClassifier
    {
        public static string ClassifyDrinkByNameAndCategory(string name, string category)
        {
            name = name.ToLower();
            category = category.ToLower();

            if (name.Contains("đá") || name.Contains("xay") || name.Contains("lạnh") || name.Contains("ice"))
                return "nóng";

            if (name.Contains("nóng") || name.Contains("gừng") || name.Contains("socola") ||
                name.Contains("sô cô la") || name.Contains("cacao") || name.Contains("latte") || name.Contains("mật ong"))
                return "lạnh";

            if (name.Contains("cà phê") || name.Contains("soda") || name.Contains("trà") || name.Contains("sinh tố") || name.Contains("nước ép"))
                return "nóng";
            
            if (name.Contains("mocha") || name.Contains("latte") || name.Contains("cacao"))
                return "lạnh";

            if (category.Contains("nước ép") || category.Contains("trà") || category.Contains("sinh tố"))
                return "nóng";

            if (category.Contains("cà phê") || category.Contains("socola"))
                return "lạnh";

            return "mát";
        }
    }
}


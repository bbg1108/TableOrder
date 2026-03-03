using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiosk.Models
{
    public class Product
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Kcal { get; set; }
        public string ImagePath { get; set; }
    }

    public class MenuList
    {
        public List<Product> Products;
    }
}

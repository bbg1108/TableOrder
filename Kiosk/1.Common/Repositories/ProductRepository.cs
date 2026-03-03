using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiosk.Models;

namespace Kiosk
{
    public class ProductRepository
    {
        public async Task<List<Product>> GetProductsAsync()
        {
            const string sql = "SELECT Division, Name, Price, Kcal, ImagePath FROM Product";

            var table = await DBHelper.QueryAsync(sql);
            var list = new List<Product>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                list.Add(new Product
                {
                    Division = row["Division"].ToString(),
                    Name = row["Name"].ToString(),
                    Price = Convert.ToInt32(row["Price"]),
                    Kcal = Convert.ToInt32(row["Kcal"]),
                    ImagePath = row["ImagePath"].ToString(),
                });
            }

            return list;
        }
    }
}

using ALLINONEPROJECTWITHOUTJS.Controllers;
using ALLINONEPROJECTWITHOUTJS.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ALLINONEPROJECTWITHOUTJS.Services
{
    public class ItemService : IItemService
    {
        private readonly string _connectionString;
        public ItemService(IConfiguration configuration) 
        {
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }    

        public List<ItemMaster> GetAllItems()
        {
            var items = new List<ItemMaster>();
            using var con = new SqlConnection(_connectionString);
            using var sda = new SqlDataAdapter("sp_GetAllItems", con);
            var itemData = new DataTable();
            sda.Fill(itemData);
            items = itemData.AsEnumerable().Select(x => new ItemMaster { Id = Convert.ToInt32(x["Id"]), Name = Convert.ToString(x["Name"]) ?? "", Price = Convert.ToDecimal(x["Price"]) }).ToList();
           
            if (items.Count == 0) 
                items.Add(new ItemMaster());

            return items;
        }
    }
}

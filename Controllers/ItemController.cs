using ALLINONEPROJECTWITHOUTJS.Data;
using ALLINONEPROJECTWITHOUTJS.Models;
using ALLINONEPROJECTWITHOUTJS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ALLINONEPROJECTWITHOUTJS.Controllers
{
    public class ItemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly IItemService _itemService;
        public ItemController(AppDbContext context,IConfiguration configuration,IItemService itemService)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("ConnectionString");
            _itemService = itemService;
        }

        public IActionResult Item()
        {
            var items = _itemService.GetAllItems();
            var im = new ItemMasterViewModel
            {
                itemMasters = items
            };
            return View(im);
        }
        [HttpPost]
        public IActionResult SaveItem(ItemMasterViewModel item)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_insertItem", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ItemName", item.ItemMaster.Name);
            cmd.Parameters.AddWithValue("@Price", item.ItemMaster.Price);
            con.Open();
            cmd.ExecuteNonQuery();
            ViewBag.Message = "Saved Successfully!";
            return RedirectToAction("Item");
        }
        [HttpPost]
        public IActionResult EditItem(ItemMasterViewModel im)
        {
            // Get the item from database
            var item = _context.ItemMasters.FirstOrDefault(x => x.Id == im.ItemMaster.Id);

            var model = new ItemMasterViewModel
            {
                itemMasters = _context.ItemMasters.ToList(), // list for the table
                ItemMaster = item // this will be used to fill top inputs
            };

            return View("Item", model); // return same view
        }

        [HttpPost]
        public IActionResult UpdateItem(ItemMasterViewModel item)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_updateItem", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", item.ItemMaster.Id);
            cmd.Parameters.AddWithValue("@ItemName", item.ItemMaster.Name);
            cmd.Parameters.AddWithValue("@Price", item.ItemMaster.Price);
            con.Open();
            cmd.ExecuteNonQuery();
            ViewBag.Message = "Saved Successfully!";
            return RedirectToAction("Item");
        }
        [HttpPost]
        public IActionResult DeleteItem(int Id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_deleteItem", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", Id);
            con.Open();
            cmd.ExecuteNonQuery();
            ViewBag.Message = "Deleted Successfully!";
            return RedirectToAction("Item");
        }

        [HttpPost]
        public IActionResult AddToCart(int Id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_itemAddToCart", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ItemId", Id);
            cmd.Parameters.AddWithValue("@UserId", userId);
            con.Open();
            cmd.ExecuteNonQuery();
            ViewBag.Message = "Deleted Successfully!";
            return RedirectToAction("Item");
        }
    }

    public class ItemMasterViewModel
    {
        public ItemMaster ItemMaster { get; set; }
        public List<ItemMaster> itemMasters { get; set; }
    }
}

using ALLINONEPROJECTWITHOUTJS.Data;
using ALLINONEPROJECTWITHOUTJS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ALLINONEPROJECTWITHOUTJS.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        public CartController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }
        public IActionResult Cart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Logout", "Account");

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_getCartItemsForUser", con);            
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var sda = new SqlDataAdapter(cmd);
            var cartData = new DataTable();
            sda.Fill(cartData);
            var cartItems = cartData.AsEnumerable().Select(x => new ItemMaster { Id = Convert.ToInt32(x["Id"]), Name = Convert.ToString(x["Name"]) ?? "", Price = Convert.ToDecimal(x["Price"]) }).ToList();
            if (cartItems.Count == 0) cartItems.Add(new ItemMaster());
            return View(cartItems);
        }
        [HttpPost]
        public IActionResult RemoveFromCart(int Id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_itemRemoveFromCart", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", Id);
            con.Open();
            cmd.ExecuteNonQuery();
            ViewBag.Message = "Removed Successfully!";
            return RedirectToAction("Cart");
        }
    }
}

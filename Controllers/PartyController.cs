using ALLINONEPROJECTWITHOUTJS.Data;
using ALLINONEPROJECTWITHOUTJS.Models;
using ALLINONEPROJECTWITHOUTJS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using static ALLINONEPROJECTWITHOUTJS.Services.PartyService;

namespace ALLINONEPROJECTWITHOUTJS.Controllers
{
    public class PartyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly IPartyService _partyService;
        public PartyController(AppDbContext context, IConfiguration configuration, IPartyService partyService)
        {
            _context = context;
            _partyService = partyService;
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }
        public IActionResult Party()
        {
            var parties = _partyService.GetAllParties();
            var im = new PartyMasterViewModel
            {
                partyMasters = parties
            };
            return View(im);
        }
        [HttpPost]
        public IActionResult SaveParty(PartyMasterViewModel party)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_insertParty", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PartyName", party.PartyMaster.Name);
            cmd.Parameters.AddWithValue("@PartyType", party.PartyMaster.Type);
            con.Open();
            cmd.ExecuteNonQuery();
            ViewBag.Message = "Saved Successfully!";
            return RedirectToAction("Party");
        }
        [HttpPost]
        public IActionResult EditParty(PartyMasterViewModel pm)
        {
            // Get the party from database
            var party = _context.PartyMasters.FirstOrDefault(x => x.Id == pm.PartyMaster.Id);

            var model = new PartyMasterViewModel
            {
                partyMasters = _context.PartyMasters.ToList(), // list for the table
                PartyMaster = party // this will be used to fill top inputs
            };

            return View("Party", model); // return same view
        }
        [HttpPost]
        public IActionResult UpdateParty(PartyMasterViewModel party)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_updateParty", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", party.PartyMaster.Id);
            cmd.Parameters.AddWithValue("@PartyName", party.PartyMaster.Name);
            cmd.Parameters.AddWithValue("@PartyType", party.PartyMaster.Type);
            con.Open();
            cmd.ExecuteNonQuery();
            ViewBag.Message = "Saved Successfully!";
            return RedirectToAction("Party");
        }
        [HttpPost]
        public IActionResult DeleteParty(int Id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_deleteParty", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", Id);
            con.Open();
            cmd.ExecuteNonQuery();
            ViewBag.Message = "Deleted Successfully!";
            return RedirectToAction("Party");
        }
    }

    public class PartyMasterViewModel
    {
        public PartyMaster PartyMaster { get; set; }
        public List<PartyMaster> partyMasters { get; set; }
    }
}

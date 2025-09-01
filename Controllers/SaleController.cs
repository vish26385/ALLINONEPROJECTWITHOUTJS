using ALLINONEPROJECTWITHOUTJS.Data;
using ALLINONEPROJECTWITHOUTJS.Models;
using ALLINONEPROJECTWITHOUTJS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ALLINONEPROJECTWITHOUTJS.Controllers
{
    public class SaleController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly IPartyService _partyService;
        private readonly IItemService _itemService;
        public SaleController(AppDbContext context, IConfiguration configuration, IItemService itemService, IPartyService partyService)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("ConnectionString");
            _itemService = itemService;
            _partyService = partyService;
        }

        public IActionResult Sale()
        {
            ////****Type Of IOrderedQueryable<SalesMaster>****////
            //var query = from s in _context.SalesMaster
            //                .Include(s => s.PartyMaster)
            //                .Include(s => s.salesDetails)
            //                    .ThenInclude(d => d.ItemMaster)
            //            orderby s.Id descending
            //            select s;

            ////****Type Of IEnumerable<SalesMaster>****////
            //var query = (from s in _context.SalesMaster
            //             join p in _context.PartyMasters
            //                 on s.PartyId equals p.Id
            //             join d in _context.SalesDetails
            //                 on s.Id equals d.SaleMasterId
            //             join i in _context.ItemMasters
            //                 on d.ItemId equals i.Id
            //             orderby s.Id descending
            //             select new
            //             {
            //                 SalesMaster = s,
            //                 PartyMaster = p,
            //                 SalesDetail = d,
            //                 ItemMaster = i
            //             })
            //            .AsEnumerable() // switch to LINQ-to-Objects for grouping
            //            .GroupBy(x => x.SalesMaster.Id)
            //            .Select(g =>
            //            {
            //                var sm = g.First().SalesMaster;
            //                sm.PartyMaster = g.First().PartyMaster;
            //                sm.salesDetails = g.Select(d =>
            //                {
            //                    d.SalesDetail.ItemMaster = d.ItemMaster;
            //                    return d.SalesDetail;
            //                }).ToList();
            //                return sm;
            //            });

            ////****Type Of IQueryable<a> means IQueryable<SalesMaster SalesMaster, PartyMaster PartyMaster, SalesDetail SalesDetail, ItemMaster ItemMaster>****////
            //var query = from s in _context.SalesMaster
            //            join p in _context.PartyMasters
            //                on s.PartyId equals p.Id
            //            join d in _context.SalesDetails
            //                on s.Id equals d.SaleMasterId
            //            join i in _context.ItemMasters
            //                on d.ItemId equals i.Id
            //            orderby s.Id descending
            //            select new
            //            {
            //                SalesMaster = s,
            //                PartyMaster = p,
            //                SalesDetail = d,
            //                ItemMaster = i
            //            };

            var sm = new SaleViewModel
            {
                itemMasters = _itemService.GetAllItems(),
                partyMasters = _partyService.GetAllParties(),

                //salesLists = query.ToList() //For above First Two

                //salesLists = query //For above Third
                //            .AsEnumerable() // switch to LINQ-to-Objects for grouping
                //            .GroupBy(x => x.SalesMaster.Id)
                //            .Select(g =>
                //            {
                //                var sm = g.First().SalesMaster;
                //                sm.PartyMaster = g.First().PartyMaster;
                //                sm.salesDetails = g.Select(d =>
                //                {
                //                    d.SalesDetail.ItemMaster = d.ItemMaster;
                //                    return d.SalesDetail;
                //                }).ToList();
                //                return sm;
                //            })
                //            .ToList()

                salesLists = _context.SalesMaster
                            .Include(s => s.PartyMaster)
                            .Include(s => s.salesDetails)
                            .ThenInclude(d => d.ItemMaster)
                            .OrderByDescending(s => s.Id)
                            .ToList()
            };
            sm.salesMaster = new SalesMaster();
            sm.salesMaster.SaleDate = DateTime.Today;
            sm.salesMaster.DueDate = DateTime.Today;
            if (sm.salesMaster.salesDetails == null || sm.salesMaster.salesDetails.Count == 0)
            {
                sm.salesMaster.salesDetails.Add(new SalesDetail());
            }
            return View(sm);
        }

        [HttpPost]
        public IActionResult Sale(SaleViewModel model)
        {
            model.itemMasters = _itemService.GetAllItems();
            model.partyMasters = _partyService.GetAllParties();
            model.salesLists = _context.SalesMaster
                .Include(s => s.PartyMaster)
                .Include(s => s.salesDetails)
                .ThenInclude(d => d.ItemMaster)
                .OrderByDescending(s => s.Id)
                .ToList();

            if (model.salesMaster != null && model.salesMaster.SaleDate != DateTime.MinValue)
            {
                switch (model.salesMaster.CalcSource)
                {
                    case "DueDays":
                        if (model.salesMaster.DueDays < 0)
                            model.salesMaster.DueDays = 0;

                        model.salesMaster.DueDate = model.salesMaster.SaleDate.AddDays(model.salesMaster.DueDays);

                        // Validation: due date must be >= sale date
                        if (model.salesMaster.DueDate < model.salesMaster.SaleDate)
                        {
                            model.salesMaster.DueDate = model.salesMaster.SaleDate;
                            model.salesMaster.DueDays = 0;
                            ModelState.AddModelError("salesMaster.DueDate", "Due Date cannot be earlier than Sale Date.");
                        }

                        ModelState.Remove("salesMaster.DueDate");
                        break;

                    case "DueDate":
                        // Validation: due date must be >= sale date
                        if (model.salesMaster.DueDate < model.salesMaster.SaleDate)
                        {
                            model.salesMaster.DueDate = model.salesMaster.SaleDate;
                            model.salesMaster.DueDays = 0;
                            ModelState.AddModelError("salesMaster.DueDate", "Due Date cannot be earlier than Sale Date.");
                        }
                        else
                        {
                            var days = (model.salesMaster.DueDate.Date - model.salesMaster.SaleDate.Date).Days;
                            model.salesMaster.DueDays = Math.Max(0, days);
                        }

                        ModelState.Remove("salesMaster.DueDays");
                        break;

                    case "SaleDate":
                        if (model.salesMaster.DueDays > 0)
                        {
                            model.salesMaster.DueDate = model.salesMaster.SaleDate.AddDays(model.salesMaster.DueDays);

                            if (model.salesMaster.DueDate < model.salesMaster.SaleDate)
                            {
                                model.salesMaster.DueDate = model.salesMaster.SaleDate;
                                model.salesMaster.DueDays = 0;
                                ModelState.AddModelError("salesMaster.DueDate", "Due Date cannot be earlier than Sale Date.");
                            }

                            ModelState.Remove("salesMaster.DueDate");
                        }
                        else if (model.salesMaster.DueDate != DateTime.MinValue)
                        {
                            if (model.salesMaster.DueDate < model.salesMaster.SaleDate)
                            {
                                model.salesMaster.DueDate = model.salesMaster.SaleDate;
                                model.salesMaster.DueDays = 0;
                                ModelState.AddModelError("salesMaster.DueDate", "Due Date cannot be earlier than Sale Date.");
                            }
                            else
                            {
                                var dd = (model.salesMaster.DueDate.Date - model.salesMaster.SaleDate.Date).Days;
                                model.salesMaster.DueDays = Math.Max(0, dd);
                            }

                            ModelState.Remove("salesMaster.DueDays");
                        }
                        break;

                    default:
                        if (model.salesMaster.DueDays > 0 && model.salesMaster.DueDate == DateTime.MinValue)
                        {
                            model.salesMaster.DueDate = model.salesMaster.SaleDate.AddDays(model.salesMaster.DueDays);

                            if (model.salesMaster.DueDate < model.salesMaster.SaleDate)
                            {
                                model.salesMaster.DueDate = model.salesMaster.SaleDate;
                                model.salesMaster.DueDays = 0;
                                ModelState.AddModelError("salesMaster.DueDate", "Due Date cannot be earlier than Sale Date.");
                            }

                            ModelState.Remove("salesMaster.DueDate");
                        }
                        else if (model.salesMaster.DueDate != DateTime.MinValue && model.salesMaster.DueDays == 0)
                        {
                            if (model.salesMaster.DueDate < model.salesMaster.SaleDate)
                            {
                                model.salesMaster.DueDate = model.salesMaster.SaleDate;
                                model.salesMaster.DueDays = 0;
                                ModelState.AddModelError("salesMaster.DueDate", "Due Date cannot be earlier than Sale Date.");
                            }
                            else
                            {
                                var dd = (model.salesMaster.DueDate.Date - model.salesMaster.SaleDate.Date).Days;
                                model.salesMaster.DueDays = Math.Max(0, dd);
                            }

                            ModelState.Remove("salesMaster.DueDays");
                        }
                        break;
                }
            }

            foreach (var detail in model.salesMaster.salesDetails)
            {
                if (detail.ItemId > 0)
                {
                    var selectedItem = model.itemMasters.FirstOrDefault(x => x.Id == detail.ItemId);
                    if (selectedItem != null)
                    {
                        detail.Rate = selectedItem.Price;
                        detail.Amount = detail.Qty * selectedItem.Price;
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult AddNewRow(SaleViewModel model)
        {
            model.itemMasters = _itemService.GetAllItems();
            model.partyMasters = _partyService.GetAllParties();
            model.salesLists = _context.SalesMaster
                .Include(s => s.PartyMaster)
                .Include(s => s.salesDetails)
                .ThenInclude(d => d.ItemMaster)
                .OrderByDescending(s => s.Id)
                .ToList();
            model.salesMaster.salesDetails.Add(new SalesDetail());
            return View("Sale", model);
        }

        [HttpPost]
        public IActionResult DeleteRow(SaleViewModel model)
        {
            model.itemMasters = _itemService.GetAllItems();
            model.partyMasters = _partyService.GetAllParties();
            model.salesLists = _context.SalesMaster
                .Include(s => s.PartyMaster)
                .Include(s => s.salesDetails)
                .ThenInclude(d => d.ItemMaster)
                .OrderByDescending(s => s.Id)
                .ToList();
            if (model.salesMaster.salesDetails.Count > 1)
            {
                model.salesMaster.salesDetails.RemoveAt(model.salesMaster.salesDetails.Count - 1);
            }
            return View("Sale", model);
        }

        [HttpPost]
        public IActionResult SaveSale(SaleViewModel model)
        {
            try
            {
                int SaleMasterId = 0;
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (var trn = con.BeginTransaction())
                    {
                        using var cmd = new SqlCommand("sp_insertSalesMaster", con, trn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SaleDate", model.salesMaster.SaleDate);
                        cmd.Parameters.AddWithValue("@DueDays", model.salesMaster.DueDays);
                        cmd.Parameters.AddWithValue("@DueDate", model.salesMaster.DueDate);
                        cmd.Parameters.AddWithValue("@PartyId", model.salesMaster.PartyId);
                        cmd.Parameters.Add("@SaleMasterId", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        SaleMasterId = (int)cmd.Parameters["@SaleMasterId"].Value;

                        foreach (var item in model.salesMaster.salesDetails)
                        {
                            using var cmdDetail = new SqlCommand("sp_insertSalesDetail", con, trn);
                            cmdDetail.CommandType = System.Data.CommandType.StoredProcedure;
                            cmdDetail.Parameters.AddWithValue("@SaleMasterId", SaleMasterId);
                            cmdDetail.Parameters.AddWithValue("@ItemId", item.ItemId);
                            cmdDetail.Parameters.AddWithValue("@Qty", item.Qty);
                            cmdDetail.ExecuteNonQuery();
                        }

                        trn.Commit();
                    }
                }
                return RedirectToAction("Sale");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public IActionResult EditSale(int id)
        {
            var master = _context.SalesMaster.Include(d => d.salesDetails).FirstOrDefault(x => x.Id == id);
            if (master == null)
                return RedirectToAction("Sale");

            var model = new SaleViewModel
            {
                salesMaster = master,
                itemMasters = _itemService.GetAllItems(),
                partyMasters = _partyService.GetAllParties(),
                salesLists = _context.SalesMaster
                .Include(s => s.PartyMaster)
                .Include(s => s.salesDetails)
                .ThenInclude(d => d.ItemMaster)
                .OrderByDescending(s => s.Id)
                .ToList()
            };

            foreach (var detail in model.salesMaster.salesDetails)
            {
                if (detail.ItemId > 0)
                {
                    var selectedItem = model.itemMasters.FirstOrDefault(x => x.Id == detail.ItemId);
                    if (selectedItem != null)
                    {
                        detail.Rate = selectedItem.Price;
                        detail.Amount = detail.Qty * selectedItem.Price;
                    }
                }
            }

            return View("Sale", model);
        }

        [HttpPost]
        public IActionResult UpdateSale(SaleViewModel model)
        {
            if (model.salesMaster == null || model.salesMaster.Id <= 0)
                return RedirectToAction("Sale");

            var existing = _context.SalesMaster.Include(d => d.salesDetails).FirstOrDefault(x => x.Id == model.salesMaster.Id);

            if (existing == null)
                return RedirectToAction("Sale");

            existing.SaleDate = model.salesMaster.SaleDate;
            existing.DueDays = model.salesMaster.DueDays;
            existing.DueDate = model.salesMaster.DueDate;
            existing.PartyId = model.salesMaster.PartyId;

            _context.SalesDetails.RemoveRange(existing.salesDetails);
            _context.SaveChanges();

            if (model.salesMaster.salesDetails != null)
            {
                foreach (var detail in model.salesMaster.salesDetails)
                {
                    if (detail.ItemId > 0)
                    {
                        _context.SalesDetails.Add(new SalesDetail { SaleMasterId = existing.Id, Qty = detail.Qty, ItemId = detail.ItemId });
                    }
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Sale");
        }

        [HttpPost]
        public IActionResult DeleteSale(int id)
        {
            var master = _context.SalesMaster.Include(d => d.salesDetails).FirstOrDefault(x => x.Id == id);
            if (master == null)
                return RedirectToAction("Sale");

            _context.SalesDetails.RemoveRange(master.salesDetails);          
            _context.SalesMaster.Remove(master);
            _context.SaveChanges();

            return RedirectToAction("Sale");
        }
    }

    public class SaleViewModel
    {
        public SalesMaster salesMaster { get; set; } = new SalesMaster();
        public List<SalesMaster> salesLists { get; set; } = new List<SalesMaster>();
        public List<ItemMaster> itemMasters { get; set; } = new List<ItemMaster>();
        public List<PartyMaster> partyMasters { get; set; } = new List<PartyMaster>();
    }
}

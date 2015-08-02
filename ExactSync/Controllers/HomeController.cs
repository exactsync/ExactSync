using ExactSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PagedList;

namespace ExactSync.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index(int? page)
        {
            List<SyncHistoryModel> models = null;

            using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
            {
                var userId = User.Identity.GetUserId();

                models = dbContext.SyncHistories
                    .Where(d => d.AspNetUID == userId)
                    .OrderByDescending(d => d.DateTimeUtc)
                    .ToList();
            }

            if (models != null && models.Count > 0)
            {
                ViewData["TotalSync"] = models.Count;
                ViewData["TotalSuccess"] = models.Where(d => d.Status == true).ToList().Count;
                ViewData["TotalFail"] = models.Where(d => d.Status == false && d.TotalFiles > 0).ToList().Count;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            return View("Index", models.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Detail(int id)
        {
            List<SyncHistoryDetailModel> models = null;

            using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
            {
                models = dbContext.SyncHistoryDetails
                    .Where(d => d.SyncHistoryId == id)
                    .OrderByDescending(d => d.Status)
                    .ToList();
            }

            return View("Detail", models);
        }
    }
}
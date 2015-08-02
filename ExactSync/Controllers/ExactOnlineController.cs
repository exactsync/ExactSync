using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using ExactSync.Helpers;
using ExactSync.Models;
using ExactOnline.Client.Models;
using ExactOnline.Client.Sdk.Controllers;
using ExactSync.Services;

namespace ExactSync.Controllers
{
    [Authorize]
    public class ExactOnlineController : Controller
    {
        ExactOnlineService exactOnline = new ExactOnlineService();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disconnect(FormCollection collection)
        {
            var id = collection["id"];

            if (id != null)
            {
                using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
                {
                    ExactOnlineAccountModel model = dbContext.ExactOnlineAccounts.Find(Convert.ToInt32(id));
                    dbContext.Entry(model).State = EntityState.Deleted;
                    await dbContext.SaveChangesAsync();
                }
            }

            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Connect()
        {
            return Redirect(exactOnline.AuthUrl().ToString());
        }

        public async Task<ActionResult> Authorize(string code, string error)
        {
            bool hasError = false;

            if (!String.IsNullOrEmpty(error))
            {
                hasError = true;
                ViewData["ErrorMessage"] = error;
            }

            if (String.IsNullOrEmpty(code))
            {
                hasError = true;
                ViewData["ErrorMessage"] = Resources.Constant.ERR_RESP_CODE;
            }

            if (!hasError)
            {
                OAuth2ResponseExact response = await exactOnline.AuthAsync(code);

                if (response != null)
                {
                    using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
                    {
                        var userId = User.Identity.GetUserId();
                        var _model = dbContext.ExactOnlineAccounts.Where(d => d.AspNetUID == userId).FirstOrDefault();

                        // If account not exists create new account
                        ExactOnlineAccountModel model = (_model != null) ? (ExactOnlineAccountModel)_model : new ExactOnlineAccountModel();

                        model.AccessTokenExpirationUtc = DateTime.UtcNow.Add(new TimeSpan(0, 0, (response.ExpiresIn - 120)));
                        model.AccessToken = response.AccessToken;
                        model.TokenType = response.TokenType;
                        model.ExpiresIn = response.ExpiresIn;
                        model.RefreshToken = response.RefreshToken;
                        model.AspNetUID = userId;

                        // Update or create state
                        dbContext.Entry(model).State = (_model != null) ? EntityState.Modified : EntityState.Added;
                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            return View("Authorize");
        }

        public ActionResult Index()
        {
            ExactOnlineAccountModel model = null;

            using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
            {
                var userId = User.Identity.GetUserId();
                model = dbContext.ExactOnlineAccounts.Where(d => d.AspNetUID == userId).FirstOrDefault();
            }

            return View("Index", model);
        }
    }
}
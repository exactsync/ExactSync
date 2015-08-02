using Dropbox.Api;
using ExactSync.Models;
using ExactSync.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Resources;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using ExactSync.Services;

namespace ExactSync.Controllers
{
    [Authorize]
    public class DropboxController : Controller
    {
        DropboxService dropbox = new DropboxService();

        [AllowAnonymous]
        public ActionResult Webhook(string challenge)
        {
            return Content(challenge);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Webhook()
        {
            // Make sure this is a valid request from Dropbox
            var signatureHeader = Request.Headers.GetValues("X-Dropbox-Signature");
            if (signatureHeader == null || !signatureHeader.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Get the signature value
            string signature = signatureHeader.FirstOrDefault();
            string jsonData = null;

            using (StreamReader reader = new StreamReader(Request.InputStream))
            {
                jsonData = await reader.ReadToEndAsync();
            }

            // Dropbox delta push notification log
            // await AuditLogService.LogAsync(Level.Info, EventType.DropboxAPI, EventAction.DeltaNotification, jsonData);

            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(dropbox.AppSecret)))
            {
                byte[] hashMessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(jsonData));
                string hexDigest = BitConverter.ToString(hashMessage).Replace("-", String.Empty);

                if (signature != hexDigest.ToLower())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            // Run sync service
            foreach (Object uid in dropbox.ProcessDeltaNotification(jsonData))
            {
                await new ExactSyncService().UID(uid.ToString()).Run();
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disconnect(FormCollection collection)
        {
            var id = collection["id"];

            if (id != null)
            {
                using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
                {
                    DropboxAccountModel model = dbContext.DropboxAccounts.Find(Convert.ToInt32(id));
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
            Session["CSRF"] = dropbox.AntiForgeryToken;
            return Redirect(dropbox.AuthUrl().ToString());
        }

        public async Task<ActionResult> Authorize(string code, string state)
        {
            bool hasError = false;

            if (Session["CSRF"] == null || !Session["CSRF"].Equals(state))
            {
                hasError = true;
                ViewData["ErrorMessage"] = Resources.Constant.ERR_CSRF;
            }

            if (String.IsNullOrEmpty(code))
            {
                hasError = true;
                ViewData["ErrorMessage"] = Resources.Constant.ERR_RESP_CODE;
            }

            if (!hasError)
            {
                OAuth2Response response = await dropbox.AuthAsync(code);

                if (response != null)
                {
                    using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
                    {
                        var userId = User.Identity.GetUserId();
                        var _model = dbContext.DropboxAccounts.Where(d => d.AspNetUID == userId).FirstOrDefault();

                        // If account not exists create new account
                        DropboxAccountModel model = (_model != null) ? (DropboxAccountModel)_model : new DropboxAccountModel();
                        model.UID = response.Uid;
                        model.AccessToken = response.AccessToken;
                        model.TokenType = response.TokenType;
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
            DropboxAccountModel model = null;

            using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
            {
                var userId = User.Identity.GetUserId();
                model = dbContext.DropboxAccounts.Where(d => d.AspNetUID == userId).FirstOrDefault();
            }

            return View("Index", model);
        }

    }
}

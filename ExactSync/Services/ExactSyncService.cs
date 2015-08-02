using Dropbox.Api;
using Dropbox.Api.Files;
using ExactOnline.Client.Models;
using ExactOnline.Client.Sdk.Controllers;
using ExactOnline.Client.Sdk.Exceptions;
using ExactSync.Helpers;
using ExactSync.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ExactSync.Services
{
    public class ExactSyncService
    {
        public string UserId { get; private set; }
        public DropboxAccountModel DropboxModel { get; private set; }
        public ExactOnlineAccountModel ExactOnlineModel { get; private set; }

        public ExactSyncService()
        {
        }

        public ExactSyncService(DropboxAccountModel dropboxModel = null, ExactOnlineAccountModel exactOnlineModel = null)
        {
            this.DropboxModel = dropboxModel;
            this.ExactOnlineModel = exactOnlineModel;
        }

        public ExactSyncService UID(string userId)
        {
            this.UserId = userId;
            return this;
        }

        public async Task Run()
        {
            while (HttpContext.Current.Application["synclock"] != null)
            {
                Thread.Sleep(1000); // wait 1 sec
            }

            await this.PrepareSync();

            if (this.DropboxModel != null && this.ExactOnlineModel != null)
            {
                await this.ExecuteSync();
            }

            this.PostSync();
        }

        public async Task PrepareSync()
        {
            using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
            {
                this.DropboxModel = dbContext.DropboxAccounts.Where(d => d.UID == this.UserId).FirstOrDefault();

                if (this.DropboxModel != null)
                {
                    string accountId = DropboxModel.AspNetUID;
                    this.ExactOnlineModel = dbContext.ExactOnlineAccounts.Where(d => d.AspNetUID == accountId).FirstOrDefault();

                    if (this.ExactOnlineModel != null)
                    {
                        try
                        {
                            TimeSpan time = this.ExactOnlineModel.AccessTokenExpirationUtc.Subtract(DateTime.UtcNow);
                            if (time.Minutes < 1)
                            {
                                OAuth2ResponseExact response = await new ExactOnlineService().RefreshTokenAsync(this.ExactOnlineModel.RefreshToken, this.ExactOnlineModel.AccessToken);
                                if (response != null)
                                {
                                    this.ExactOnlineModel.AccessTokenExpirationUtc = DateTime.UtcNow.Add(new TimeSpan(0, 0, (response.ExpiresIn - 120)));
                                    this.ExactOnlineModel.AccessToken = response.AccessToken;
                                    this.ExactOnlineModel.TokenType = response.TokenType;
                                    this.ExactOnlineModel.ExpiresIn = response.ExpiresIn;
                                    this.ExactOnlineModel.RefreshToken = response.RefreshToken;

                                    dbContext.Entry(this.ExactOnlineModel).State = EntityState.Modified;
                                    await dbContext.SaveChangesAsync();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AuditLogService.Log(Level.Error, EventType.Exception, EventAction.PrepareSyncAction, String.Format("UID: {0} - {1}", this.UserId, ex.Message));
                        }
                    }
                    else
                    {
                        await AuditLogService.LogAsync(Level.Error, EventType.Exception, EventAction.PrepareSyncAction, String.Format("UID: {0} - ExactOnline Integration Settings Not Found", this.UserId));
                    }
                }
                else
                {
                    await AuditLogService.LogAsync(Level.Error, EventType.Exception, EventAction.PrepareSyncAction, String.Format("UID: {0} - Dropbox Integration Settings Not Found", this.UserId));
                }
            }
        }

        public async Task ExecuteSync()
        {
            this.Lock(true);

            bool isAllSyncCompleted = true;

            DeltaModel deltaModel = await DropboxHelper.DeltaQuery(DropboxModel.AccessToken, DropboxModel.Cursor);

            List<EntryModel> entryModels = this.ValidateLocalEntryStates(deltaModel);

            if (entryModels != null && entryModels.Count > 0)
            {
                using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
                {
                    // Init sync history record
                    SyncHistoryModel syncHistoryModel = new SyncHistoryModel();
                    syncHistoryModel.AspNetUID = DropboxModel.AspNetUID;
                    syncHistoryModel.DateTimeUtc = DateTime.UtcNow;
                    syncHistoryModel.TotalFiles = 0;
                    syncHistoryModel.Status = false;

                    dbContext.Entry(syncHistoryModel).State = EntityState.Added;
                    await dbContext.SaveChangesAsync();

                    // Loop all delta records
                    foreach (EntryModel entry in entryModels)
                    {
                        // Attach document
                        if (!entry.is_dir && !entry.is_del)
                        {
                            bool isAttached = false;

                            SyncHistoryDetailModel detailModel = new SyncHistoryDetailModel();
                            detailModel.SyncHistoryId = syncHistoryModel.Id;
                            detailModel.FileName = this.GetFileName(entry.path);
                            detailModel.MimeType = entry.mime_type;
                            detailModel.Size = entry.size;
                            detailModel.DateTimeModified = entry.modified;
                            detailModel.Rev = entry.rev;
                            detailModel.Action = "Attach";

                            try
                            {
                                isAttached = await CreateOrUpdateDocument(entry.path);
                            }
                            catch (ExactOnline.Client.Sdk.Exceptions.UnauthorizedException)
                            {
                                detailModel.Remark = "Unauthorized intensive synchronisation processes on Exact Online server";
                            }
                            catch (Exception ex)
                            {
                                detailModel.Remark = ex.Message;
                            }

                            detailModel.Status = isAttached;

                            dbContext.Entry(detailModel).State = EntityState.Added;
                            await dbContext.SaveChangesAsync();

                            if (!isAttached)
                            {
                                isAllSyncCompleted = isAttached;
                                // Logging
                                await AuditLogService.LogAsync(Level.Error, EventType.SyncAction, EventAction.DocumentNew, String.Format("UID: {0} - Document: {1}, Action: Attach Fail, Remark: {2}", ExactOnlineModel.AspNetUID, this.GetFileName(entry.path), detailModel.Remark));
                            }
                        }

                        // Delete document
                        if (entry.is_del)
                        {
                            bool isDeleted = false;

                            SyncHistoryDetailModel detailModel = new SyncHistoryDetailModel();
                            detailModel.SyncHistoryId = syncHistoryModel.Id;
                            detailModel.FileName = this.GetFileName(entry.path);
                            detailModel.MimeType = "-";
                            detailModel.Size = "-";
                            detailModel.DateTimeModified = "-";
                            detailModel.Action = "Delete";

                            try
                            {
                                isDeleted = DeleteDocument(entry.path);
                            }
                            catch (ExactOnline.Client.Sdk.Exceptions.UnauthorizedException)
                            {
                                detailModel.Remark = "Unauthorized intensive synchronisation processes on Exact Online server";
                            }
                            catch (Exception ex)
                            {
                                detailModel.Remark = ex.Message;
                            }

                            detailModel.Status = isDeleted;
                            dbContext.Entry(detailModel).State = EntityState.Added;
                            await dbContext.SaveChangesAsync();

                            if (!isDeleted)
                            {
                                isAllSyncCompleted = isDeleted;
                                // Logging
                                await AuditLogService.LogAsync(Level.Error, EventType.SyncAction, EventAction.DocumentDelete, String.Format("UID: {0} - Document: {1}, Action: Delete Fail, Remark: {2}", ExactOnlineModel.AspNetUID, this.GetFileName(entry.path), detailModel.Remark));
                            }
                        }
                    } // foreach

                    // Update sync history record
                    syncHistoryModel.TotalFiles = entryModels.Count;
                    syncHistoryModel.Status = isAllSyncCompleted;
                    dbContext.Entry(syncHistoryModel).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();

                    // Update dropbox local state
                    DropboxModel.Cursor = deltaModel.cursor;
                    dbContext.Entry(DropboxModel).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();
                } // using

                if (deltaModel.has_more)
                {
                    await this.ExecuteSync();
                }
            }
            this.Lock(false);
        }

        public void PostSync()
        {
            if (this.DropboxModel != null)
            {
                this.DropboxModel = null;
            }

            if (this.ExactOnlineModel != null)
            {
                this.ExactOnlineModel = null;
            }
        }

        public void Lock(bool flag)
        {
            HttpContext.Current.Application.Lock();
            HttpContext.Current.Application["synclock"] = (flag) ? "sync" : null;
            HttpContext.Current.Application.UnLock();
        }

        public List<EntryModel> ValidateLocalEntryStates(DeltaModel deltaModel)
        {
            if (deltaModel == null)
            {
                return null;
            }

            if (deltaModel.EntryModels == null)
            {
                return null;
            }

            using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
            {
                foreach (EntryModel entry in deltaModel.EntryModels)
                {
                    string fileName = this.GetFileName(entry.path);

                    if (!entry.is_del)
                    {
                        var result = dbContext.DeltaEntryStates.Where(d => d.UserId == this.UserId
                                && d.FileName == fileName
                                && d.MimeType == entry.mime_type
                                && d.Size == entry.size
                                && d.Rev == entry.rev
                                && d.DateTimeModified == entry.modified).ToList();

                        if (result != null && result.Count > 0)
                        {
                            // Remove duplicated
                            deltaModel.EntryModels.Remove(entry);
                        }
                        else
                        {
                            DeltaEntryStateModel stateModel = new DeltaEntryStateModel();
                            stateModel.UserId = this.UserId;
                            stateModel.FileName = fileName;
                            stateModel.MimeType = entry.mime_type;
                            stateModel.Size = entry.size;
                            stateModel.Rev = entry.rev;
                            stateModel.DateTimeModified = entry.modified;

                            dbContext.Entry(stateModel).State = EntityState.Added;
                            dbContext.SaveChanges();
                        }
                    }
                    else
                    {
                        var result = dbContext.DeltaEntryStates.Where(d => d.UserId == this.UserId && d.FileName == fileName).ToList();

                        if (result != null && result.Count > 0)
                        {
                            foreach (DeltaEntryStateModel stateModel in result)
                            {
                                dbContext.Entry(stateModel).State = EntityState.Deleted;
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            // Remove duplicated
                            deltaModel.EntryModels.Remove(entry);
                        }
                    }
                }
            }

            return deltaModel.EntryModels;
        }

        public async Task<bool> CreateOrUpdateDocument(string path)
        {
            ExactOnlineClient exactOnlineClient = this.GetExactOnlineClient();
            var guid = this.GetDocumentId(path);

            return (Guid.Empty.CompareTo(guid) == 0)
                ? await this.NewDocument(path)
                : await this.UpdateDocument(guid, path);
        }

        public async Task<bool> NewDocument(string path)
        {
            ExactOnlineClient exactOnlineClient = this.GetExactOnlineClient();

            Document document = new Document();
            document.Subject = this.GetFileName(path);
            document.Type = 183; // Attachment Type
            document.Category = Guid.Parse("3b6d3833-b31b-423d-bc3c-39c62b8f2b12"); // General Category

            bool isCreated = exactOnlineClient.For<Document>().Insert(ref document);

            if (!isCreated)
            {
                throw new Exception("Document fail to create on Exact Online server");
            }

            DropboxClient dropboxClient = this.GetDropboxClient(this.DropboxModel.AccessToken);
            DownloadArg arg = new DownloadArg(path);

            DocumentAttachment attachment = new DocumentAttachment();
            attachment.Document = document.ID;
            attachment.FileName = this.GetFileName(path);
            attachment.Attachment = await this.GetDropboxService().DownloadContentAsByteArray(dropboxClient, arg);

            bool isAttached = exactOnlineClient.For<DocumentAttachment>().Insert(ref attachment);

            return isAttached;
        }

        public async Task<bool> UpdateDocument(Guid guid, string path)
        {
            ExactOnlineClient exactOnlineClient = this.GetExactOnlineClient();

            DropboxClient dropboxClient = this.GetDropboxClient(DropboxModel.AccessToken);
            DownloadArg arg = new DownloadArg(path);

            DocumentAttachment attachment = new DocumentAttachment();
            attachment.Document = guid;
            attachment.FileName = this.GetFileName(path);
            attachment.Attachment = await this.GetDropboxService().DownloadContentAsByteArray(dropboxClient, arg);

            bool isAttached = exactOnlineClient.For<DocumentAttachment>().Insert(ref attachment);

            return isAttached;
        }

        public bool DeleteDocument(string path)
        {
            ExactOnlineClient exactOnlineClient = this.GetExactOnlineClient();
            var guid = this.GetDocumentId(path, exactOnlineClient);

            if (Guid.Empty.CompareTo(guid) == 0)
            {
                throw new Exception("Document not found on Exact Online server");
            }

            var document = this.GetDocument(guid, exactOnlineClient);
            bool isDeleted = exactOnlineClient.For<Document>().Delete(document);

            return isDeleted;
        }

        public Document GetDocument(Guid id, ExactOnlineClient exactOnlinClient = null)
        {
            try
            {
                ExactOnlineClient client = exactOnlinClient ?? this.GetExactOnlineClient();
                return client.For<Document>().GetEntity(id);
            }
            catch (Exception ex)
            {
                AuditLogService.Log(Level.Error, EventType.Exception, EventAction.DocumentGet, ex.Message);
            }

            return null;
        }

        public Guid GetDocumentId(string path, ExactOnlineClient exactOnlinClient = null)
        {
            try
            {
                ExactOnlineClient client = exactOnlinClient ?? this.GetExactOnlineClient();
                return client.For<Document>().Select("ID").Where("Subject+eq+'" + this.GetFileName(path) + "'").Get().First().ID;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Sequence contains no elements"))
                {
                    AuditLogService.Log(Level.Error, EventType.Exception, EventAction.DocumentGet, ex.Message);
                    throw ex;
                }
            }

            return Guid.Empty;
        }

        public string GetFileName(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return path;
            }

            char[] delimiter = { '/' };
            string[] node = path.Split(delimiter);

            return node[node.Length - 1];
        }

        public DropboxService GetDropboxService()
        {
            return new DropboxService();
        }

        public DropboxClient GetDropboxClient(string accessToken)
        {
            return new DropboxClient(accessToken);
        }

        public ExactOnlineService GetExactOnlineService()
        {
            return new ExactOnlineService();
        }

        public ExactOnlineClient GetExactOnlineClient()
        {
            return new ExactOnlineClient(this.GetExactOnlineService().ApiEndPoint(Region.UK), this.ExactOnlineModel.AccessTokenManagerDelegate);
        }
    }
}
using ExactSync.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExactSync.Services
{
    public enum Level
    {
        Info,
        Debug,
        Warn,
        Error
    }

    public enum EventType
    {
        DropboxAPI,
        ExactOnlineAPI,
        SyncAction,
        Exception
    }

    public enum EventAction
    {
        DeltaNotification,
        RequestDelta,
        PrepareSyncAction,
        ExecuteSyncAction,
        DocumentGet,
        DocumentNew,
        DocumentUpdate,
        DocumentDelete
    }

    public static class AuditLogService
    {
        public static async Task LogAsync(Level level, EventType type, EventAction action, string data)
        {
            using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
            {
                AuditLogModel logModel = new AuditLogModel();
                logModel.Level = level.ToString();
                logModel.EventType = type.ToString();
                logModel.EventAction = action.ToString();
                logModel.Data = data;
                logModel.UTC = DateTime.UtcNow;

                dbContext.Entry(logModel).State = EntityState.Added;
                await dbContext.SaveChangesAsync();
            }
        }

        public static void Log(Level level, EventType type, EventAction action, string data)
        {
            using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
            {
                AuditLogModel logModel = new AuditLogModel();
                logModel.Level = level.ToString();
                logModel.EventType = type.ToString();
                logModel.EventAction = action.ToString();
                logModel.Data = data;
                logModel.UTC = DateTime.UtcNow;

                dbContext.Entry(logModel).State = EntityState.Added;
                dbContext.SaveChanges();
            }
        }
    }
}
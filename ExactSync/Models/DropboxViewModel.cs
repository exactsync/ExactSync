using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ExactSync.Models
{
    [Table("DropboxAccount")]
    public class DropboxAccountModel
    {
        public int Id { get; set; }
        public string AspNetUID { get; set; }
        [DisplayName("User ID")]
        public string UID { get; set; }
        [DisplayName("Access Token")]
        public string AccessToken { get; set; }
        [DisplayName("Token Type")]
        public string TokenType { get; set; }
        public string Cursor { get; set; }
    }

    [Table("DropboxDeltaEntryLocalState")]
    public class DeltaEntryStateModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string Size { get; set; }
        public string DateTimeModified { get; set; }
        public string Rev { get; set; }
    }

    public class DeltaModel
    {
        public bool has_more { get; set; }
        public string cursor { get; set; }
        public List<Object[]> entries { get; set; }
        public bool reset { get; set; }

        public List<EntryModel> EntryModels
        {
            get
            {
                List<EntryModel> models = new List<EntryModel>();
                if (entries != null && entries.Count > 0)
                {
                    foreach (Object[] obj in entries)
                    {
                        if (obj[1] != null)
                        {
                            EntryModel model = JsonConvert.DeserializeObject<EntryModel>(obj[1].ToString());
                            models.Add(model);
                        }
                        else
                        {
                            EntryModel model = new EntryModel();
                            model.path = obj[0].ToString();
                            model.is_del = true;
                            models.Add(model);
                        }
                    }
                }
                return models;
            }
        }
    }

    public class EntryModel
    {
        public string rev { get; set; }
        public bool thumb_exists { get; set; }
        public string path { get; set; }
        public bool is_dir { get; set; }
        public string client_mtime { get; set; }
        public string icon { get; set; }
        public bool read_only { get; set; }
        public string modifier { get; set; }
        public int bytes { get; set; }
        public string modified { get; set; }
        public string size { get; set; }
        public string root { get; set; }
        public string mime_type { get; set; }
        public int revision { get; set; }
        public bool is_del { get; set; }
    }
}
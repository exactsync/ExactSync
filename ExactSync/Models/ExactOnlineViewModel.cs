using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ExactSync.Models
{
    [Table("ExactOnlineAccount")]
    public class ExactOnlineAccountModel
    {
        public int Id { get; set; }
        public string AspNetUID { get; set; }
        [DisplayName("Access Token")]
        public string AccessToken { get; set; }
        [DisplayName("Token Type")]
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        [DisplayName("Access Token Expiration (UTC)")]
        public DateTime AccessTokenExpirationUtc { get; set; }

        // Delegate function call
        // ExactOnline.Client.Sdk implementation
        public string AccessTokenManagerDelegate()
        {
            return this.AccessToken;
        }
    }
}
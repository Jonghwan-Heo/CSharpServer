using Dapper.Contrib.Extensions;
using System;

namespace Database.GameModels
{
    [Serializable]
    [Dapper.Table("players")]
    public class PlayerModel
    {
        public long id { get; set; }    // global_meta_player_id
        public long server_id { get; set; }
        public string sns_id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public DateTime? agreed_terms_at { get; set; }
        public string login_token { get; set; }
        public DateTime? connected_at { set; get; }
        public string connected_ip { set; get; }
        public int exp { get; set; }
        public string language { get; set; }
        public int level { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime created_at { get; set; }
    }
}
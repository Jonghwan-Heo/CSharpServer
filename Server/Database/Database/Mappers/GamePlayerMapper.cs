using Dapper;
using Database.GameModels;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Mappers
{
    public class GamePlayerMapper
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DBSqlExecutor _executor;

        public GamePlayerMapper(DBSqlExecutor executor)
        {
            _executor = executor;
        }

        public PlayerModel Get(long id)
        {
            return GetAsync(id).Result;
        }

        public IEnumerable<PlayerModel> GetAllByIDs(IEnumerable<long> ids)
        {
            return GetAllByIDsAsync(ids).Result;
        }

        public IEnumerable<PlayerModel> GetAllByName(string names)
        {
            return GetAllByNameAsync(names).Result;
        }

        public PlayerModel GetByName(string name)
        {
            return GetByNameAsync(name).Result;
        }

        public PlayerModel GetBySNSID(string sns_id)
        {
            return GetBySNSIDAsync(sns_id).Result;
        }

        public void InsertTermsAgreed(PlayerModel value)
        {
            InsertTermsAgreedAsync(value).Wait();
        }

        public void UpdateAgreedTermsAt(PlayerModel value)
        {
            UpdateAgreedTermsAtAsync(value).Wait();
        }

        public void UpdateConnectedAt(PlayerModel value, string ip, string language)
        {
            UpdateConnectedAtAsync(value, ip, language).Wait();
        }

        public void UpdateDayContinued(PlayerModel value)
        {
            UpdateDayContinuedAsync(value).Wait();
        }

        public void UpdateExp(PlayerModel value, int exp)
        {
            UpdateExpAsync(value, exp).Wait();
        }

        public void UpdateLanguage(PlayerModel value, string language)
        {
            UpdateLanguageAsync(value, language).Wait();
        }

        public void UpdateLevel(PlayerModel value, int level, bool isLevelUp = false)
        {
            UpdateLevelAsync(value, level, isLevelUp).Wait();
        }

        public void UpdateName(PlayerModel value, string name)
        {
            UpdateNameAsync(value, name).Wait();
        }

        public void UpdateNameAndTermsAgreed(PlayerModel value, string name)
        {
            UpdateNameAndTermsAgreedAsync(value, name).Wait();
        }

        public void UpdateSNSID(PlayerModel value, string sns_id)
        {
            UpdateSNSIDAsync(value, sns_id).Wait();
        }

        private async Task<IEnumerable<PlayerModel>> GetAllByIDsAsync(IEnumerable<long> ids)
        {
            if (ids.Count() == 0)
                return new List<PlayerModel>();

            var p = new DynamicParameters();
            p.Add("@ids", ids);
            return await _executor.WithSessionAsync(x =>
            {
                return x.QueryAsync<PlayerModel>("SELECT * FROM players WHERE id IN @ids", p);
            });
        }

        private async Task<IEnumerable<PlayerModel>> GetAllByNameAsync(string names)
        {
            var p = new DynamicParameters();
            p.Add("@names", names);
            return await _executor.WithSessionAsync(x =>
            {
                return x.QueryAsync<PlayerModel>("SELECT * FROM players WHERE name IN (@names)", p);
            });
        }

        private async Task<PlayerModel> GetAsync(long id)
        {
            var p = new DynamicParameters();
            p.Add("@id", id);
            return await _executor.WithSessionAsync(x =>
            {
                return x.QueryFirstOrDefaultAsync<PlayerModel>("SELECT * FROM players WHERE id = @id", p);
            });
        }

        private async Task<PlayerModel> GetByNameAsync(string name)
        {
            var p = new DynamicParameters();
            p.Add("@name", name);
            return await _executor.WithSessionAsync(x =>
            {
                return x.QueryFirstOrDefaultAsync<PlayerModel>("SELECT * FROM players WHERE name LIKE @name", p);
            });
        }

        private async Task<PlayerModel> GetBySNSIDAsync(string sns_id)
        {
            var p = new DynamicParameters();
            p.Add("@sns_id", sns_id);
            return await _executor.WithSessionAsync(x =>
            {
                return x.QueryFirstOrDefaultAsync<PlayerModel>("SELECT * FROM players WHERE sns_id = @sns_id", p);
            });
        }

        private async Task InsertTermsAgreedAsync(PlayerModel value)
        {
            value.login_token = Guid.NewGuid().ToString();

            value.email = value.email;
            value.created_at = DateTime.UtcNow;
            value.updated_at = DateTime.UtcNow;
            value.agreed_terms_at = DateTime.UtcNow;
            value.id = await _executor.WithSessionAsync(async db =>
            {
                await db.ExecuteAsync(@"INSERT INTO players(id, sns_id, name, email, `state`, secret, `language`, login_token, updated_at, created_at, agreed_terms_at)
                    VALUES(@id, @sns_id, @name, @email, @state, @secret, @language, @login_token, @updated_at, @created_at, @agreed_terms_at)", value);

                return await db.ExecuteScalarAsync<long>("SELECT id FROM players WHERE sns_id = @sns_id", value);
            });
        }

        private async Task UpdateAgreedTermsAtAsync(PlayerModel value)
        {
            value.agreed_terms_at = DateTime.UtcNow;

            await _executor.WithSessionAsync(x =>
            {
                return x.ExecuteAsync(@"UPDATE players SET
agreed_terms_at = @agreed_terms_at
WHERE id = @id", value);
            });
        }

        private async Task UpdateConnectedAtAsync(PlayerModel value, string ip, string language)
        {
            value.language = language;

            value.connected_ip = ip;
            value.connected_at = DateTime.UtcNow;

            await _executor.WithSessionAsync(x =>
            {
                return x.ExecuteAsync(@"UPDATE players SET
connected_ip = @connected_ip,
connected_at = @connected_at,
language = @language WHERE id = @id", value);
            });
        }

        private async Task UpdateDayContinuedAsync(PlayerModel value)
        {
            await _executor.WithSessionAsync(x =>
            {
                return x.ExecuteAsync("UPDATE players SET last_connected_date = @last_connected_date, day_continued = @day_continued WHERE id = @id", value);
            });
        }

        private async Task UpdateExpAsync(PlayerModel value, int exp)
        {
            value.exp = exp;
            value.updated_at = DateTime.UtcNow;

            var p = new DynamicParameters();
            p.Add("@id", value.id);
            p.Add("@exp", value.exp);
            p.Add("@updated_at", value.updated_at);

            await _executor.WithSessionAsync(x =>
            {
                return x.ExecuteAsync("UPDATE players SET exp = @exp, updated_at = @updated_at WHERE id = @id", p);
            });
        }

        private async Task UpdateLanguageAsync(PlayerModel value, string language)
        {
            if (value.language == language)
                return;

            var p = new DynamicParameters();
            p.Add("@id", value.id);
            p.Add("@language", language);

            await _executor.WithSessionAsync(x =>
            {
                return x.ExecuteAsync("UPDATE players SET language = @language WHERE id = @id", p);
            });

            value.language = language;
        }

        private async Task UpdateLevelAsync(PlayerModel value, int level, bool isLevelUp = false)
        {
            value.level = level;
            value.updated_at = DateTime.UtcNow;
            value.exp = isLevelUp ? 0 : value.exp;

            var p = new DynamicParameters();
            p.Add("@id", value.id);
            p.Add("@level", value.level);
            p.Add("@exp", value.exp);
            p.Add("@updated_at", value.updated_at);

            await _executor.WithSessionAsync(x =>
            {
                return x.ExecuteAsync("UPDATE players SET level = @level, exp = @exp, updated_at = @updated_at WHERE id = @id", p);
            });
        }

        private async Task UpdateNameAndTermsAgreedAsync(PlayerModel value, string name)
        {
            value.name = name;
            value.agreed_terms_at = DateTime.UtcNow;

            await _executor.WithSessionAsync(x =>
            {
                return x.ExecuteAsync("UPDATE players SET name = @name, agreed_terms_at = @agreed_terms_at WHERE id = @id", value);
            });

            value.name = name;
        }

        private async Task UpdateNameAsync(PlayerModel value, string name)
        {
            value.name = name;

            await _executor.WithSessionAsync(x =>
            {
                return x.ExecuteAsync("UPDATE players SET name = @name WHERE id = @id", value);
            });
        }

        private async Task UpdateSNSIDAsync(PlayerModel value, string sns_id)
        {
            value.sns_id = sns_id;

            await _executor.WithSessionAsync(x =>
            {
                return x.ExecuteAsync(@"UPDATE players SET sns_id = @sns_id WHERE id = @id", value);
            });
        }
    }
}
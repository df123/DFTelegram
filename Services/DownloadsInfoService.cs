using DFTelegram.Helper;
using DFTelegram.Models;
using SqlSugar;

namespace DFTelegram.Services
{
    public class DownloadsInfoService
    {
        private readonly ISqlSugarClient _sqlSugarClient;
        public DownloadsInfoService(ISqlSugarClient sqlSugarClient)
        {
            this._sqlSugarClient = sqlSugarClient;
        }

        public async Task<bool> AddIgnoreSavePathTaskComplete(DownloadsInfo downloadsInfo)
        {
            int result = await _sqlSugarClient.Insertable<DownloadsInfo>(downloadsInfo)
                                .IgnoreColumns(m => new { m.SavePath, m.TaskComplete })
                                .ExecuteCommandAsync();
            if (result >= 1)
            {
                return true;
            }
            return false;
        }

        public async Task<int> Add(DownloadsInfo downloadsInfo)
        {
            int result = await _sqlSugarClient.Insertable(downloadsInfo).ExecuteCommandAsync();
            return result;
        }

        public async Task<int> DeleteByAccessHashID(DownloadsInfo downloadsInfo)
        {
            int result = await _sqlSugarClient.Deleteable<DownloadsInfo>()
                                .Where(m => m.ID == downloadsInfo.ID && m.AccessHash == downloadsInfo.AccessHash)
                                .IsLogic()
                                .ExecuteCommandAsync();
            return result;
        }

        public async Task<DownloadsInfo> GetVideoEarliest()
        {
            return await _sqlSugarClient.Queryable<DownloadsInfo>()
                   .Where(m => m.IsDelete == false && m.IsDownload == true)
                   .OrderBy(m => new { m.PID }).FirstAsync();
        }

        public async Task<DownloadsInfo> GetVideoReturn()
        {
            return await _sqlSugarClient.Queryable<DownloadsInfo>()
                   .Where(m => m.IsDelete == false && m.IsReturn == true && m.IsDownload == true)
                   .OrderBy(m => new { m.PID }).FirstAsync();
        }

        public async Task<long> GetDownloadsSize()
        {
            DateTime todayAtZero = DateTimeHelper.GetTodayAtZero();
            DateTime tomorrowAtZero = DateTimeHelper.GetTomorrowAtZero();
            long size = await _sqlSugarClient.Queryable<DownloadsInfo>()
                    .Where(m => m.IsDownload == true &&
                    m.TaskComplete >= todayAtZero &&
                    m.TaskComplete < tomorrowAtZero)
                    .SumAsync(m => m.Size);
            return size;
        }

        public async Task<DownloadsInfo[]> GetByAccessHashID(DownloadsInfo downloadsInfo)
        {
            DownloadsInfo[] isArray = await _sqlSugarClient.Queryable<DownloadsInfo>()
            .Where(m => m.AccessHash == downloadsInfo.AccessHash && m.ID == downloadsInfo.ID)
            .ToArrayAsync();
            return isArray;
        }

        public async Task<DownloadsInfo[]> GetByValueSHA1(DownloadsInfo downloadsInfo)
        {
            DownloadsInfo[] isArray = await _sqlSugarClient.Queryable<DownloadsInfo>()
            .Where(m => m.ValueSHA1 == downloadsInfo.ValueSHA1)
            .ToArrayAsync();
            return isArray;
        }

        public async Task<DownloadsInfo[]> GetNotDownloads()
        {
            DownloadsInfo[] isArray = await _sqlSugarClient.Queryable<DownloadsInfo>()
            .Where(m => m.IsDownload == false)
            .ToArrayAsync();
            return isArray;
        }

        public async Task<bool> IsNotDownloads(DownloadsInfo downloadsInfo)
        {
            DownloadsInfo[] array = await _sqlSugarClient.Queryable<DownloadsInfo>()
            .Where(m => m.AccessHash == downloadsInfo.AccessHash && m.ID == downloadsInfo.ID && m.IsDownload == false)
            .ToArrayAsync();
            if (array != null && array.Length > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<DownloadsInfo[]> GetDownloadsNotDelete()
        {
            DownloadsInfo[] array = await _sqlSugarClient.Queryable<DownloadsInfo>()
            .Where(m => m.IsDownload == true && m.IsDelete == false)
            .ToArrayAsync();
            return array;
        }

        public async Task<DownloadsInfo[]> GetByConditional(string conditionals)
        {
            DownloadsInfo[] isArray = await _sqlSugarClient.Queryable<DownloadsInfo>()
            .Where(_sqlSugarClient.Utilities.JsonToConditionalModels(conditionals))
            .ToArrayAsync();
            return isArray;
        }

        public async Task<DownloadsInfo[]> GetAll()
        {
            DownloadsInfo[] isArray = await _sqlSugarClient.Queryable<DownloadsInfo>().ToArrayAsync();
            return isArray;
        }

        public async Task<int> UpdateByAccessHashIDIgnoreFour(DownloadsInfo downloadsInfo)
        {
            int result = await _sqlSugarClient.Updateable<DownloadsInfo>(downloadsInfo)
                                .WhereColumns(m => new { m.AccessHash, m.ID })
                                .IgnoreColumns(m => new { m.IsReturn, m.Size, m.TaskCreate, m.IsDelete })
                                .ExecuteCommandAsync();
            return result;
        }



    }
}
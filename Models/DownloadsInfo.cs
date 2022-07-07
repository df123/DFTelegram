using System;
using SqlSugar;

namespace DFTelegram.Models
{
    public class DownloadsInfo
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long PID { get; set; }
        public long AccessHash { get; set; }
        public long ID { get; set; }
        public long Size { get; set; }
        public bool IsDownload { get; set; }
        public bool IsReturn { get; set; }
        public DateTime TaskCreate { get; set; }
        [SugarColumn(IsNullable = true)]
        public DateTime? TaskComplete { get; set; }
        #nullable disable
        public string SavePath { get; set; }
        public string ValueSHA1{get;set;}
        #nullable restore
        public bool IsDelete{get;set;}
    }
}
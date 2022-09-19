namespace DFTelegram.BackgroupTaskService;

using System.Threading;
using System.Threading.Tasks;
using DFTelegram.BackgroupTaskService.QueueService;
using DFTelegram.Helper;
using DFTelegram.Models;
using DFTelegram.Services;
using SqlSugar;
using TL;
public class ListenTelegramService : BackgroundService
{
    private ILogger<ListenTelegramService> _logger;
    private readonly WTelegram.Client _client;
    private readonly IQueueBase<Photo> _photoQueueBase;
    private readonly IQueueBase<Document> _mediaQueueBase;
    private readonly DownloadsInfoService _downloadsInfoService;
    public ListenTelegramService(ILogger<ListenTelegramService> logger,
    WTelegram.Client client,
    IQueueBase<Photo> photoQueueBase,
    IQueueBase<Document> mediaQueueBase,
    DownloadsInfoService downloadsInfoService)
    {
        this._logger = logger;
        this._client = client;
        this._photoQueueBase = photoQueueBase;
        this._mediaQueueBase = mediaQueueBase;
        this._downloadsInfoService = downloadsInfoService;
        WTelegram.Helpers.Log = (lvl, str) => _logger.Log((LogLevel)lvl, str);
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("开始Telegram消息监听");
        TL.User user = await _client.LoginUserIfNeeded();
        _client.OnUpdate += ClientUpdate;
        Task downloadMediaTask = DownloadMedia(stoppingToken);
        Task downloadPhotoTask = DownloadPhoto(stoppingToken);
        await downloadPhotoTask;
        await downloadMediaTask;
    }

    async Task ClientUpdate(IObject arg)
    {
        if (arg is not Updates { updates: var updates } upd) return;
        foreach (var update in updates)
        {
            if (update is not UpdateNewMessage { message: Message message })
            {
                continue;
            }

            if (message.media is MessageMediaDocument { document: Document document })
            {
                int slash = document.mime_type.IndexOf('/');
                if (slash < 0)
                {
                    continue;
                }
                string typeName = document.mime_type[..(slash)];
                if (typeName.ToLower() != "video" && typeName.ToLower() != "image")
                {
                    continue;
                }
                bool canAdd = await AddDownloadInfo(new DownloadsInfo()
                {
                    AccessHash = document.access_hash,
                    ID = document.id,
                    Size = document.size,
                });
                if (canAdd && _mediaQueueBase.GetConcurrentQueueCount() < 16)
                {
                    _mediaQueueBase.AddItem(document);
                }

            }
            else if (message.media is MessageMediaPhoto { photo: Photo photo })
            {
                bool canAdd = await AddDownloadInfo(new DownloadsInfo()
                {
                    AccessHash = photo.access_hash,
                    ID = photo.id,
                    Size = photo.LargestPhotoSize.FileSize,
                });
                if (canAdd && _photoQueueBase.GetConcurrentQueueCount() < 16)
                {
                    _photoQueueBase.AddItem(photo);
                }

            }
        }
    }

    public async Task DownloadPhoto(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string savePathPrefix = AppsettingsHelper.app("RunConfig", "SavePhotoPathPrefix");
                await IsUpperLimit();
                Photo photo = await _photoQueueBase.GetItemAsync(stoppingToken);
                await IsSpaceUpperLimit(photo.LargestPhotoSize.FileSize);
                if (photo == null)
                {
                    continue;
                }
                string fileName = Path.Combine(savePathPrefix, $"{photo.id}.jpg");
                _logger.LogInformation($"图片下载{fileName},access:{photo.access_hash},id:{photo.id}");
                using var fileStream = File.Create(fileName);
                var type = await _client.DownloadFileAsync(photo, fileStream);
                string valueSHA1 = HashHelper.CalculationHash(fileStream);
                fileStream.Close();
                await UpdateDownloadInfo(new DownloadsInfo()
                {
                    AccessHash = photo.access_hash,
                    ID = photo.id,
                    IsDownload = true,
                    TaskComplete = DateTime.Now,
                    SavePath = fileName,
                    ValueSHA1 = valueSHA1
                });
                _logger.LogInformation($"图片下载完成{fileName},access:{photo.access_hash},id:{photo.id}");
            }
            catch (Exception e)
            {
                _logger.LogError($"下载图片出错:{e.Message}");
            }

        }
    }

    public async Task DownloadMedia(CancellationToken stoppingToken)
    {
        #nullable disable
        Document document = null;
        #nullable restore
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string savePathPrefix = AppsettingsHelper.app("RunConfig", "SaveVideoPathPrefix");
                await IsUpperLimit();
                document = await _mediaQueueBase.GetItemAsync(stoppingToken);
                await IsSpaceUpperLimit(document.size);
                if (document == null)
                {
                    continue;
                }
                int slash = document.mime_type.IndexOf('/');
                string fileName = Path.Combine(savePathPrefix, $"{document.id}.{document.mime_type[(slash + 1)..]}");
                string fileNameTemp = $"{fileName}.temp";
                _logger.LogInformation($"下载媒体{fileName},access:{document.access_hash},id:{document.id}");
                using var fileStream = File.Create(fileNameTemp);
                await _client.DownloadFileAsync(document, fileStream);
                string valueSHA1 = HashHelper.CalculationHash(fileStream);
                fileStream.Close();
                File.Move(fileNameTemp, fileName, true);
                await UpdateDownloadInfo(new DownloadsInfo()
                {
                    AccessHash = document.access_hash,
                    ID = document.id,
                    IsDownload = true,
                    TaskComplete = DateTime.Now,
                    SavePath = fileName,
                    ValueSHA1 = valueSHA1
                });
                _logger.LogInformation($"下载媒体完成{fileName},access:{document.access_hash},id:{document.id}");
            }
            catch (Exception e)
            {
                if (document == null)
                {
                    _logger.LogError($"下载媒体出错:{e.Message}:::::{e.StackTrace}");
                }
                else
                {
                    _logger.LogError($"access:{document.access_hash},id:{document.id}:::下载媒体出错:{e.Message}:::::{e.StackTrace}");
                }
            }

        }
    }

    public async Task<bool> AddDownloadInfo(DownloadsInfo downloadsInfo)
    {
        downloadsInfo.IsDownload = false;
        downloadsInfo.IsReturn = false;
        downloadsInfo.TaskCreate = DateTime.Now;

        DownloadsInfo[] isArray = await _downloadsInfoService.GetByAccessHashID(downloadsInfo);
        if (isArray.Length > 0)
        {
            _logger.LogInformation($"AccessHash:{downloadsInfo.AccessHash},ID:{downloadsInfo.ID},已经存在;");
            return await _downloadsInfoService.IsNotDownloads(downloadsInfo);
        }

        bool result = await _downloadsInfoService.AddIgnoreSavePathTaskComplete(downloadsInfo);
        _logger.LogInformation($"AccessHash:{downloadsInfo.AccessHash},ID:{downloadsInfo.ID},保存成功;");
        return result;
    }

    public async Task UpdateDownloadInfo(DownloadsInfo downloadsInfo)
    {
        DownloadsInfo[] isArray = await _downloadsInfoService.GetByValueSHA1(downloadsInfo);
        if (isArray.Length > 0)
        {
            await DeleteDownloadInfo(downloadsInfo);
        }

        int result = await _downloadsInfoService.UpdateByAccessHashIDIgnoreFour(downloadsInfo);
        _logger.LogInformation($"AccessHash:{downloadsInfo.AccessHash},ID:{downloadsInfo.ID},更新成功;");
    }

    public async Task DeleteDownloadInfo(DownloadsInfo downloadsInfo)
    {
        _logger.LogInformation($"开始删除重复。ID:{downloadsInfo.ID},AccessHash:{downloadsInfo.AccessHash},Hash:{downloadsInfo.ValueSHA1}");
        try
        {
            File.Delete(downloadsInfo.SavePath);
            await _downloadsInfoService.DeleteByAccessHashID(downloadsInfo);

            _logger.LogInformation($"结束删除成功。ID:{downloadsInfo.ID},AccessHash:{downloadsInfo.AccessHash},Hash:{downloadsInfo.ValueSHA1}");
        }
        catch (System.Exception e)
        {
            _logger.LogInformation($"结束删除失败,失败信息{e.Message}。ID:{downloadsInfo.ID},AccessHash:{downloadsInfo.AccessHash},Hash:{downloadsInfo.ValueSHA1}");
        }
    }

    public async Task<double> CalculationDownloadsSize()
    {
        long size = await _downloadsInfoService.GetDownloadsSize();
        return StorageUnitConversionHelper.ByteToMB((double)(size));
    }

    public async Task IsUpperLimit()
    {
        long bandwidth = 0;
        long.TryParse(AppsettingsHelper.app("RunConfig", "Bandwidth"), out bandwidth);
        double sizes = await CalculationDownloadsSize();
        _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}已下载{sizes}MB");
        if (sizes > bandwidth)
        {
            _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}下载流量达到上限,暂停下载");
            Thread.Sleep(DateTimeHelper.GetUntilTomorrowTimeSpan());
            _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}下载流量，重新开始");
        }
    }

    public async Task IsSpaceUpperLimit(long sizes)
    {
        double availableFreeSpace = 0;
        double.TryParse(AppsettingsHelper.app("RunConfig", "AvailableFreeSpace"), out availableFreeSpace);
        _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}可用空间{SpaceHelper.GetRootAvailableMB()}MB");
        while (SpaceHelper.GetRootAvailableMB() - StorageUnitConversionHelper.ByteToMB(sizes) < availableFreeSpace)
        {
            if (bool.Parse(AppsettingsHelper.app(new string[] {
                "RunConfig", "IntervalTime","SpaceTime","Enable"})))
            {
                _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}可用空间到达下限,暂停下载");
                Thread.Sleep(int.Parse(AppsettingsHelper.app(new string[] {
                "RunConfig", "IntervalTime","SpaceTime","Duration"})));
            }
            await DeleteFile();
        }
        _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}可用空间足够,开始下载");
    }

    public void ViewProgress(long transmitted, long totalSize)
    {
        if (totalSize != 0)
        {
            double percent = transmitted / totalSize;
            _logger.LogInformation($"已经下载{percent}%");
        }
        else
        {
            _logger.LogInformation($"已经下载{transmitted}字节");
        }

    }

    private async Task DeleteFile()
    {
        #nullable disable
        DownloadsInfo downloadsInfo = null;
        #nullable restore
        switch (AppsettingsHelper.app(new string[] { "RunConfig", "DeleteFileModle" }))
        {
            case "direct":
                downloadsInfo = await _downloadsInfoService.GetVideoEarliest();
                break;
            default:
                downloadsInfo = await _downloadsInfoService.GetVideoReturn();
                break;
        }
        if (downloadsInfo != null)
        {
            SpaceHelper.DeleteFile(downloadsInfo.SavePath);
            await _downloadsInfoService.DeleteByAccessHashID(downloadsInfo);
            _logger.LogInformation($"删除ID:{downloadsInfo.ID},AccessHash:{downloadsInfo.AccessHash},Hash:{downloadsInfo.ValueSHA1},Sizes:{downloadsInfo.Size}");
        }
        else
        {
            Thread.Sleep(10000);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("关闭Telegram消息监听");
        return Task.CompletedTask;
    }
}
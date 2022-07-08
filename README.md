DFTelegram
=========
使用WTelegramClient实现的，从Telegram消息中下载图片和视频的下载器。

# 使用方法

配置appsettings.json。使用dotnet publish发布生成，然后运行

    dotnet publish

访问http://127.0.0.1:15000/swagger，在TL/SetVerificationCode提交验证码。


# appsettings.json文件参数
``` json
"Telegram": {
    "session_pathname":"./WTelegram.session",//登录成功以后的会话保存位置
    "api_id": "api_id",//Telegram的api_id
    "api_hash": "api_hash",//Telegram的api_hash
    "phone_number": "phone_number"//你的电话号码
  },
  "RunConfig":{
    "Bandwidth":30720,//下载流量限制（单位MB）
    "AvailableFreeSpace":2048,//剩余空间限制（单位MB）
    "SavePhotoPathPrefix": "./",//下载的图片保存位置
    "SaveVideoPathPrefix": "./",//下载的视频保存位置
    "SQLitePath":"./",//数据库保存位置
    "Proxy":{//代理设置
      "EnableProxy":true,//是否启用代理
      "ProxyHost":"127.0.0.1",
      "ProxyPort":10079
    },
    "DeleteFileModle":"direct",//当空间达到最低值时，是否直接删除最早下载的文件
    "ReturnDownloadUrlPrefix":"",//合成下载链接的前缀
    "IntervalTime": {
      "SpaceTime": {//达到存储空间时，检查间隔时间（单位秒）
        "Enable": false,
        "Duration": 5000
      }
    }
  }
}
```
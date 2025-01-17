﻿using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace n0tFlix.Addons.YoutubeDL
{
    public class YoutubeDlEntryPoint : IServerEntryPoint
    {
        private IFileSystem FileSystem { get; set; }
        private ILogger Logger { get; set; }
        private IHttpClientFactory httpClientFactory { get; set; }
        private IServerApplicationPaths ServerApplicationPaths { get; set; }
        private static ISessionManager SessionManager { get; set; }
        private static IJsonSerializer JsonSerializer { get; set; }

        // ReSharper disable once TooManyDependencies
        public YoutubeDlEntryPoint(ILogger<YoutubeDlEntryPoint> logger, IServerApplicationPaths paths, IHttpClientFactory httpClientFactory)
        {
            Logger = logger;
            ServerApplicationPaths = paths;
            this.httpClientFactory = httpClientFactory;
        }

        private void Ses_PlaybackStopped(object sender, MediaBrowser.Controller.Library.PlaybackStopEventArgs e)
        {
            e.MediaInfo.Path = "";
        }

        public void Dispose()
        {
        }

        public async Task RunAsync()
        {
            string youtube_dl_path = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!File.Exists(Path.Combine(Plugin.Instance.DataFolderPath, "youtube-dl.exe")))
                {
                    Logger.LogDebug("Downloading youtube-dl.exe");
                    Stream youtubeDL = await httpClientFactory.CreateClient().GetStreamAsync(@"https://yt-dl.org/downloads/latest/youtube-dl.exe");
                    using (var fs = new FileStream(Path.Combine(Plugin.Instance.DataFolderPath, "youtube-dl.exe"), FileMode.CreateNew))
                    {
                        await youtubeDL.CopyToAsync(fs);
                    }
                }
                youtube_dl_path = Path.Combine(Plugin.Instance.DataFolderPath, "youtube-dl.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (!File.Exists(Path.Combine(Plugin.Instance.DataFolderPath, "youtube-dl")))
                {
                    Logger.LogDebug("Downloading youtube-dl");
                    Stream youtubeDL = await httpClientFactory.CreateClient().GetStreamAsync(@"https://yt-dl.org/downloads/latest/youtube-dl");
                    using (var fs = new FileStream(Path.Combine(Plugin.Instance.DataFolderPath, "youtube-dl"), FileMode.CreateNew))
                    {
                        await youtubeDL.CopyToAsync(fs);
                    }
                    //dont belive this should give any output?
                    youtube_dl_path = Path.Combine(Plugin.Instance.DataFolderPath, "youtube-dl");
                }
            }
            else
            {
                Logger.LogError("I NEED A MAC BEFORE I CAN IMPLEMENT SUPPORT FOR IT PLEASE COME WITH A DONATION IF YOU WISH FOR THIS");
                Logger.LogError("I NEED A MAC BEFORE I CAN IMPLEMENT SUPPORT FOR IT PLEASE COME WITH A DONATION IF YOU WISH FOR THIS");
                Logger.LogError("I NEED A MAC BEFORE I CAN IMPLEMENT SUPPORT FOR IT PLEASE COME WITH A DONATION IF YOU WISH FOR THIS");
                Logger.LogError("I NEED A MAC BEFORE I CAN IMPLEMENT SUPPORT FOR IT PLEASE COME WITH A DONATION IF YOU WISH FOR THIS");
            }
        }
    }
}
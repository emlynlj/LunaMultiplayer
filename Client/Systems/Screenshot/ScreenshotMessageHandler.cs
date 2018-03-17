﻿using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Screenshot;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.Screenshot
{
    public class ScreenshotMessageHandler : SubSystem<ScreenshotSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ScreenshotBaseMsgData msgData)) return;

            switch (msgData.ScreenshotMessageType)
            {
                case ScreenshotMessageType.ScreenshotData:
                    var screenshotMsg = (ScreenshotDataMsgData)msgData;
                    var image = CreateImage(screenshotMsg.Screenshot.Width, screenshotMsg.Screenshot.Height, screenshotMsg.Screenshot.Data, screenshotMsg.Screenshot.NumBytes);

                    if (System.DownloadedImages.TryGetValue(screenshotMsg.Screenshot.FolderName, out var folderImages))
                        folderImages.AddOrUpdate(screenshotMsg.Screenshot.DateTaken, image, (key, existingVal) => image);
                    break;
                case ScreenshotMessageType.FoldersReply:
                    var foldersMsg = (ScreenshotFoldersReplyMsgData)msgData;
                    for (var i = 0; i < foldersMsg.NumFolders; i++)
                    {
                        System.DownloadedImages.TryAdd(foldersMsg.Folders[i], new ConcurrentDictionary<long, Texture2D>());
                        System.MiniatureImages.TryAdd(foldersMsg.Folders[i], new ConcurrentDictionary<long, Texture2D>());
                    }
                    break;
                case ScreenshotMessageType.ListReply:
                    var listMsg = (ScreenshotListReplyMsgData)msgData;
                    if (System.MiniatureImages.TryGetValue(listMsg.FolderName, out var folderMiniatureImages))
                    {
                        for (var i = 0; i < listMsg.NumScreenshots; i++)
                        {
                            var miniImage = CreateImage(listMsg.Screenshots[i].Width, listMsg.Screenshots[i].Height, listMsg.Screenshots[i].Data, listMsg.Screenshots[i].NumBytes);
                            folderMiniatureImages.AddOrUpdate(listMsg.Screenshots[i].DateTaken, miniImage, (key, existingVal) => miniImage);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Texture2D CreateImage(int width, int height, byte[] data, int numBytes)
        {
            var image = new Texture2D(width, height);
            var imgData = new byte[numBytes];

            Array.Copy(data, imgData, numBytes);
            image.LoadImage(data);
            return image;
        }
    }
}
﻿using SongCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;
using Logger = TournamentAssistantShared.Logger;

namespace TournamentAssistant
{
    public class SongDownloader
    {
        private static string beatSaverDownloadUrl = "https://beatsaver.com/api/download/hash/";

        public static void DownloadSong(string hash, bool refreshWhenDownloaded = true, Action<bool> songDownloaded = null)
        {
            SharedCoroutineStarter.instance.StartCoroutine(DownloadSong_internal(hash, refreshWhenDownloaded, songDownloaded));
        }

        private static IEnumerator DownloadSong_internal(string hash, bool refreshWhenDownloaded = true, Action<bool> songDownloaded = null)
        {
            UnityWebRequest www = UnityWebRequest.Get($"{beatSaverDownloadUrl}{hash}");
            bool timeout = false;
            float time = 0f;

            UnityWebRequestAsyncOperation asyncRequest = www.SendWebRequest();

            while (!asyncRequest.isDone || asyncRequest.progress < 1f)
            {
                yield return null;

                time += Time.deltaTime;

                if (time >= 15f && asyncRequest.progress == 0f)
                {
                    www.Abort();
                    timeout = true;
                }
            }

            if (www.isNetworkError || www.isHttpError || timeout)
            {
                Logger.Error($"Error downloading song {hash}: {www.error}");
                songDownloaded?.Invoke(false);
            }
            else
            {
                string zipPath = "";
                string customSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;
                string customSongPath = "";

                byte[] data = www.downloadHandler.data;

                try
                {
                    customSongPath = customSongsPath + "/" + hash + "/";
                    zipPath = customSongPath + hash + ".zip";
                    if (!Directory.Exists(customSongPath))
                    {
                        Directory.CreateDirectory(customSongPath);
                    }
                    File.WriteAllBytes(zipPath, data);
                }
                catch (Exception e)
                {
                    Logger.Error($"Error writing zip: {e}");
                    songDownloaded?.Invoke(false);
                    yield break;
                }

                try
                {
                    ZipFile.ExtractToDirectory(zipPath, customSongPath);
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to extract ZIP! Exception: {e}");
                    songDownloaded?.Invoke(false);
                    yield break;
                }

                try
                {
                    File.Delete(zipPath);
                }
                catch (IOException e)
                {
                    Logger.Warning($"Unable to delete zip! Exception: {e}");
                    yield break;
                }

                Logger.Success($"Downloaded!");

                if (refreshWhenDownloaded)
                {
                    Action<Loader, Dictionary<string, CustomPreviewBeatmapLevel>> songsLoaded = null;
                    songsLoaded = (_, __) =>
                        {
                            Loader.SongsLoadedEvent -= songsLoaded;
                            songDownloaded?.Invoke(true);
                        };
                    Loader.SongsLoadedEvent += songsLoaded;
                    Loader.Instance.RefreshSongs(false);
                }
                else songDownloaded?.Invoke(true);
            }
        }
    }
}

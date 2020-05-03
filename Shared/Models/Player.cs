﻿using System;
using BattleSaberShared.Models.Packets;

namespace BattleSaberShared.Models
{
    [Serializable]
    public class Player : User
    {
        public enum PlayState
        {
            Waiting,
            InGame,
        }

        public enum DownloadState
        {
            None,
            Downloading,
            Downloaded,
            DownloadError
        }

        [Serializable]
        public struct Point
        {
            public int x;
            public int y;
        }

        public Team Team { get; set; } 
        public PlayState CurrentPlayState { get; set; }
        public DownloadState CurrentDownloadState { get; set; }
        public int CurrentScore { get; set; }
        public SongList SongList { get; set; }

        //Stream sync
        public Point StreamScreenCoordinates;
        public long StreamDelayMs;
        public long StreamSyncStartMs;
    }
}

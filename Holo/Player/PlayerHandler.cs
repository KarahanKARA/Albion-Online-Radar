using System;
using System.Collections.Generic;
using System.Linq;

namespace AlbionRadar.Player
{
    public static class PlayerHandler
    {
        public static int LocalPlayerId { get; set; } = -1;

        private static readonly Dictionary<int, PlayerInfo> Players = new Dictionary<int, PlayerInfo>();
        public static readonly object PlayerLock = new object();

        private static float LocalPlayerX;
        private static float LocalPlayerY;
        public static string MapID { get; set; }
        public static string ZoneType { get; set; } = "BLUE";

        private static float _previousLocalX;
        private static float _previousLocalY;
        private static DateTime _lastLocalMovementTime = DateTime.MinValue;

        private static readonly Dictionary<int, MobInfo> Mobs = new Dictionary<int, MobInfo>();
        public static readonly object MobLock = new object();

        public static void Reset()
        {
            lock (PlayerLock)
            {
                Players.Clear();
                LocalPlayerX = 0;
                LocalPlayerY = 0;
                MapID = string.Empty;
            }

            lock (MobLock)
            {
                Mobs.Clear();
                MainForm.Log("Mob listesi temizlendi - Debug Test");
            }
        }

        public static void AddPlayer(float x, float y, string name, string guild, string alliance, int id, bool isPKEnabled)
        {
            lock (PlayerLock)
            {
                if (Players.TryGetValue(id, out var existingPlayer))
                {
                    existingPlayer.X = x;
                    existingPlayer.Y = y;
                    existingPlayer.LastUpdateTime = DateTime.Now;
                    existingPlayer.IsPKEnabled = isPKEnabled;
                }
                else
                {
                    Players[id] = new PlayerInfo
                    {
                        X = x,
                        Y = y,
                        Name = name,
                        Guild = guild,
                        Alliance = alliance,
                        LastUpdateTime = DateTime.Now,
                        IsPKEnabled = isPKEnabled
                    };

                    MainForm.Log($"New Player Added: {name} ({id}) - PK: {(isPKEnabled ? "OPEN" : "CLOSED")}");
                }
            }
        }

        public static void RemovePlayer(int id)
        {
            lock (PlayerLock)
            {
                if (Players.ContainsKey(id))
                {
                    string playerName = Players[id].Name;
                    Players.Remove(id);
                    MainForm.Log($"Player Removed: {playerName} ({id})");
                }
            }
        }
        public static void UpdatePlayerPosition(int id, float x, float y)
        {
            if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y))
                return;

            if (Math.Abs(x) > 5000 || Math.Abs(y) > 5000)
                return;

            lock (PlayerLock)
            {
                if (Players.TryGetValue(id, out var player))
                {
                    bool isFirstUpdate = (player.X == 0 && player.Y == 0);

                    if (isFirstUpdate)
                    {
                        player.X = x;
                        player.Y = y;
                        player.LastUpdateTime = DateTime.Now;
                    }
                    else
                    {
                        float deltaX = Math.Abs(x - player.X);
                        float deltaY = Math.Abs(y - player.Y);

                        if (deltaX < 50 && deltaY < 50) 
                        {
                            player.X = x;
                            player.Y = y;
                            player.LastUpdateTime = DateTime.Now;
                        }
                        else
                        {
                        }
                    }
                }
            }
        }

        public static void UpdateLocalPlayerPosition(float x, float y)
        {
            _previousLocalX = LocalPlayerX;
            _previousLocalY = LocalPlayerY;

            LocalPlayerX = x;
            LocalPlayerY = y;

            _lastLocalMovementTime = DateTime.Now;

            MainForm.UpdatePlayerPos(x, y);
        }


        public static int GetNearbyEnemyCount()
        {
            lock (PlayerLock)
            {
                return Players.Values.Count(p => p.IsPKEnabled);
            }
        }

        public static (float X, float Y)? GetPlayerPosition(int id)
        {
            lock (PlayerLock)
            {
                if (Players.TryGetValue(id, out var player))
                {
                    return (player.X, player.Y);
                }
                return null;
            }
        }

        
        public static (float X, float Y) GetLocalPlayerPosition()
        {
            return (LocalPlayerX, LocalPlayerY);
        }

        public static float GetLocalPlayerPosX()
        {
            return LocalPlayerX;
        }

        public static (float deltaX, float deltaY)? GetLocalPlayerMovement()
        {
            if ((DateTime.Now - _lastLocalMovementTime).TotalMilliseconds > 1000)
                return null;

            return (LocalPlayerX - _previousLocalX, LocalPlayerY - _previousLocalY);
        }

        public static float GetLocalPlayerPosY()
        {
            return LocalPlayerY;
        }

    }

    public class PlayerInfo
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string Name { get; set; }
        public string Guild { get; set; }
        public string Alliance { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public bool HasValidPosition { get; set; }
        public bool IsPKEnabled { get; set; }
    }

    public class MobInfo
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string Type { get; set; }
        public bool IsDynamic { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using DotNetEnv;
using StackExchange.Redis;
using UnityEngine;

namespace Scener.Redis
{
    public class RedisClient : MonoBehaviour
    {
        public static RedisClient instance { get; private set; }

        private string host;
        private int port;

        private ConnectionMultiplexer _redisConnection;
        private IDatabase _redisDatabase;

        public bool IsConnected => _redisConnection != null && _redisConnection.IsConnected;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            await ConnectAsync();
        }

        private async void OnApplicationQuit()
        {
            await DisconnectAsync();
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
            {
                Debug.Log("Redis is already connected.");
                return;
            }

            try
            {
                string envFilePath = Path.Combine(Application.streamingAssetsPath, ".env");

                if (File.Exists(envFilePath))
                {
                    Env.Load(envFilePath);

                    host = Env.GetString("REDIS_HOST", "localhost");
                    port = Env.GetInt("REDIS_PORT", 6379);
                }

                Debug.Log($"Attempting to connect to Redis at {host}:{port}...");
                _redisConnection = await ConnectionMultiplexer.ConnectAsync($"{host}:{port}");
                _redisDatabase = _redisConnection.GetDatabase();

                if (IsConnected)
                {
                    Debug.Log("<color=green>Successfully connected to Redis!</color>");
                }
                else
                {
                    Debug.LogError("Redis connection failed.");
                }
            }
            catch (RedisConnectionException e)
            {
                Debug.LogError($"<color=red>Redis Connection Failed: {e.Message}</color>");
                _redisConnection = null;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_redisConnection != null)
            {
                Debug.Log("Disconnecting from Redis...");
                await _redisConnection.CloseAsync();
                _redisConnection.Dispose();
                _redisConnection = null;
            }
        }

        public async Task<bool> WriteSceneAsync(string clientId, string sceneJson)
        {
            if (!IsConnected)
            {
                Debug.LogError("Redis is not connected.");
                return false;
            }

            try
            {
                string key = $"scene:{clientId}";
                return await _redisDatabase.StringSetAsync(key, sceneJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to Redis: {e.Message}");
                return false;
            }
        }
    }
}

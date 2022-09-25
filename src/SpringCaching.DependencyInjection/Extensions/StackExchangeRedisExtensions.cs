using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SpringCaching.DependencyInjection
{
    internal static class StackExchangeRedisExtensions
    {
        private readonly static Dictionary<string, Delegate> s_fieldDelegateMap = new Dictionary<string, Delegate>();

        static StackExchangeRedisExtensions()
        {
            AddFieldDelegate(typeof(RedisCache), "_cache");
            AddFieldDelegate(typeof(RedisCache), "_instance");
        }

        private static Delegate CreateFieldDelegate(Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                throw new ArgumentException($"field : {fieldName} not found!");
            }
            ParameterExpression instance = Expression.Parameter(type);
            Expression body = Expression.Field(instance, fieldInfo);
            Type delegateType = typeof(Func<,>);
            delegateType = delegateType.MakeGenericType(type, fieldInfo.FieldType);
            return Expression.Lambda(delegateType, body, instance).Compile();
        }

        private static void AddFieldDelegate(Type type, string fieldName)
        {
            string key = type.FullName + "." + fieldName;
            Delegate action = CreateFieldDelegate(type, fieldName);
            s_fieldDelegateMap.Add(key, action);
        }

        private static T? GetFieldDelegate<T>(Type type, string fieldName) where T : Delegate
        {
            string key = type.FullName + "." + fieldName;
            s_fieldDelegateMap.TryGetValue(key, out var value);
            return value as T;
        }

        private static Func<T, TField>? GetFieldDelegate<T, TField>(string fieldName)
        {
            return GetFieldDelegate<Func<T, TField>>(typeof(T), fieldName);
        }

        public static IDatabase GetDatabase(this RedisCache redisCache)
        {
            IDatabase database = redisCache.GetFieldValue<IDatabase>("_cache");
            if (database == null)
            {
                redisCache.GetType().GetMethod("Connect", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(redisCache, Array.Empty<object>());
                database = redisCache.GetFieldValue<IDatabase>("_cache");
            }
            return database;
        }

        public static string GetInstanceName(this RedisCache redisCache)
        {
            string instanceName = redisCache.GetFieldValue<string>("_instance");
            return instanceName;
        }

        public static void DeleteKeyByPattern(this RedisCache redisCache, string keyPattern)
        {
            string instanceName = redisCache.GetInstanceName();
            keyPattern = instanceName + keyPattern;
            var database = redisCache.GetDatabase();
            var connection = database.Multiplexer;
            foreach (var endPoint in connection.GetEndPoints())
            {
                var server = connection.GetServer(endPoint);
                //if (!(server.IsConnected && !server.IsReplica))
                if (!server.IsConnected)
                {
                    continue;
                }
                var keys = server.Keys(pattern: keyPattern).ToArray();
                if (keys.Length > 0)
                {
                    database.KeyDelete(keys);
                }
                break;
            }
        }

        public static async Task DeleteKeyByPatternAsync(this RedisCache redisCache, string keyPattern)
        {
            string instanceName = redisCache.GetInstanceName();
            keyPattern = instanceName + keyPattern;
            var database = redisCache.GetDatabase();
            var connection = database.Multiplexer;
            foreach (var endPoint in connection.GetEndPoints())
            {
                var server = connection.GetServer(endPoint);
                //if (!(server.IsConnected && !server.IsReplica))
                if (!server.IsConnected)
                {
                    continue;
                }
#if NET6_0_OR_GREATER
                var keys = server.Keys(pattern: keyPattern).ToArray();
#else
                var keys = server.Keys(pattern: keyPattern).ToArray();
#endif
                if (keys.Length > 0)
                {
                    await database.KeyDeleteAsync(keys).ConfigureAwait(false);
                }
                break;
            }
        }

        private static TField GetFieldValue<TField>(this RedisCache redisCache, string fieldName)
        {
            return GetFieldDelegate<RedisCache, TField>(fieldName)!.Invoke(redisCache);
        }

    }
}

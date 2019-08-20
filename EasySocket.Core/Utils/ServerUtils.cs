using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace EasySocket.Core.Utils
{
    public static class ServerUtils
    {
        public static T GetConfigValue<T>(IConfigurationSection section, string key, T defaultValue)
        {
            string value = section[key];
            if (value == null)
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static T GetConfigValue<T>(IConfigurationSection section, string key)
        {
            string value = section[key];
            if (value == null)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}

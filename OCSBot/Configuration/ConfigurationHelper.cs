using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure;

namespace OCSBot.Configuration
{
    public class ConfigurationHelper
    {
        public static string GetString(string key)
        {
            return CloudConfigurationManager.GetSetting(key);
        }
        public static int GetInteger(string key)
        {
            int output = int.MinValue;
            if(int.TryParse(GetString(key),out output))
            {
                return output;
            }else
            {
                throw new Exception($"Unable to get Integer key:{key}");
            }
        }
        public static float GetFloat(string key)
        {
            float output = float.MinValue;
            if (float.TryParse(GetString(key), out output))
            {
                return output;
            }
            else
            {
                throw new Exception($"Unable to get Float key:{key}");
            }
        }
    }
}
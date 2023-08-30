using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Server.Core
{
    class JsonHelper
    {
        public static JObject GetJObject(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json) || json.StartsWith("["))
                    return null;

                return JObject.Parse(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static JArray GetJArray(string json)
        {
            try
            {
                return JArray.Parse(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T FromJson<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return default;
            }
        }

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jerry.CMS.Core.Helper
{
    public static class JsonHelper
    {
        public static string ObjectToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, new IsoDateTimeConverter(){DateTimeFormat = "yyyy-MM-dd HH:mm:ss"});
        }

        //public static string ObjectToJson(this object obj, JsonConvert[] converters)
        //{
        //    return JsonConvert.SerializeObject(obj, converters);
        //}

        public static T JsonToObject<T>(this string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }
        public static T JSONToObjectBySetting<T>(this string input, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(input, settings);
        }

    }
}

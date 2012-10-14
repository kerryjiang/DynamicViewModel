using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DynamicViewModel
{
    public static class JsonExtensions
    {
        public static bool TryCreateDynamic<T>(this string json, out T result)
            where T : DynamicViewModel, new()
        {
            result = new T();

            try
            {
                var jObject = JObject.Parse(json);
                if (jObject != null)
                {
                    result = jObject.ToDynamic(result);
                }

                return true;
            }
            catch
            {
                try
                {
                    var jArray = JArray.Parse(json);
                    if (jArray != null)
                    {
                        var items = jArray.ToDynamicArray(new List<DynamicViewModel>());
                        result.Set("Items", items);
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static dynamic ToDynamic(this JObject jObject, DynamicViewModel parent)
        {
            foreach (var kvPair in jObject)
            {
                var value = kvPair.Value;
                if (value is JObject)
                {
                    dynamic property = (value as JObject).ToDynamic(DynamicViewModelFactory.Create());
                    parent.Set(kvPair.Key, property);
                }
                else if (value is JArray)
                {
                    // It's an array - make the property a collection.
                    dynamic property = (value as JArray).ToDynamicArray(new List<DynamicViewModel>());
                    parent.Set(kvPair.Key, property);
                }
                else
                {
                    parent.Set(kvPair.Key, value);
                }
            }

            return parent;
        }

        public static dynamic ToDynamicArray(this JArray jArray, List<DynamicViewModel> parent)
        {
            foreach (var jToken in jArray)
            {
                if (jToken is JObject)
                {
                    dynamic property = (jToken as JObject).ToDynamic(DynamicViewModelFactory.Create());
                    parent.Add(property);
                }
                else if (jToken is JArray)
                {
                    dynamic property = (jToken as JArray).ToDynamicArray(new List<DynamicViewModel>());
                    parent.Add(property);
                }
            }

            return parent;
        }
    }
}

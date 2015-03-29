using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
namespace ThumbnailCreator
{
    static class JsonSerializer
    {
        public static object Deserialize(string json, Type type) 
        {
            return new JavaScriptSerializer().Deserialize(json, type);
        }
        public static string Serialize(object obj) 
        {
            return new JavaScriptSerializer().Serialize(obj);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Common
{
    public static class YmlHelper
    {
        #region 导入导出
        public static bool Load<T>(string path, ref T config)
        {
            try
            {
                using (TextReader reader = File.OpenText(path))
                {
                    //Deserializer deserializer = new Deserializer();
                    //var deserializer = new DeserializerBuilder()
                    //    .IgnoreUnmatchedProperties()
                    //    .Build();
                    config = De.Deserialize<T>(reader);
                }
                return true;
            }
            catch (Exception e)
            {
                //new Log4netService.LogRecord(e);
                if (config == null)
                    config = default;
                return false;
            }
        }

        public static void Save<T>(string path, T config)
        {
            Serializer serializer = new Serializer();
            StringWriter strWriter = new StringWriter();
            serializer.Serialize(strWriter, config);
            //serializer.Serialize(Console.Out, config);
            using (TextWriter writer = File.CreateText(path))
            {
                writer.Write(strWriter.ToString());
            }
        }

        #endregion
        public static IDeserializer De = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

    }


}

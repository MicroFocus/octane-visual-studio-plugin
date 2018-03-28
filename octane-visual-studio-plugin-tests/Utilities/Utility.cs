using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities
{
    public static class Utility
    {
        public static T Clone<T>(T source)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            string serializedObject;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, source);

                memoryStream.Position = 0;
                using (StreamReader sr = new StreamReader(memoryStream))
                {
                    serializedObject = sr.ReadToEnd();
                }
            }

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedObject)))
            {
                return (T)serializer.ReadObject(stream);
            }
        }

    }
}

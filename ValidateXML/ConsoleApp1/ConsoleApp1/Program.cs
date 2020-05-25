using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using ConsoleApp1.TemplateReg;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    class Program
    {
        public static Dictionary<string, string> registersData = new Dictionary<string, string>();

        static void Main(string[] args)
        {

            //==================================================================================
            var path = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("", path + "\\XMLFile1.xsd");
            XmlReader rd = XmlReader.Create(path + "\\XMLFile.xml");
            XDocument doc = XDocument.Load(rd);
            doc.Validate(schema, DocumentValidationHandler);

            //==================================================================================
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path + "\\XMLFile.xml");
                        
            //XmlNode root = xDoc.DocumentElement;
            //XmlNodeList dataList = xDoc.GetElementsByTagName("DevicesDownloadConfiguration");  //root.SelectNodes("DevicesDownloadConfiguration");


            ArrayOfDevicesDownloadConfiguration arrayTemplateData = null;
            XmlSerializer serializer = new XmlSerializer(typeof(ArrayOfDevicesDownloadConfiguration));

            StreamReader reader = new StreamReader(path + "\\XMLFile.xml");
            arrayTemplateData = (ArrayOfDevicesDownloadConfiguration)serializer.Deserialize(reader);
            reader.Close();

            foreach(var devices in arrayTemplateData.DevicesDownloadConfiguration)
            {
                string registers = JsonConvert.SerializeObject(devices.Variables);
                foreach(var deviceType in devices.SupportedDeviceTypes)
                {
                    registersData.Add(deviceType, registers);
                }
            }


            
            //==================================================================================
            /*XmlTextReader reader = new XmlTextReader(path + "\\XMLFile.xml");
            while (reader.Read())
            {
                Console.WriteLine(reader.Name + reader.Value);
            }*/
            //==================================================================================
        }

        private static void DocumentValidationHandler(object sender, ValidationEventArgs e)
        {
            System.Console.WriteLine(e.Message);
        }
    }
}

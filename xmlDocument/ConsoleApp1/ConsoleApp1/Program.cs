using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ConsoleApp1
{
    public class Program
    {

        static void Main(string[] args)
        {
            loadDocument();
        }

        private static void loadDocument()
        {
            string path = "D:\\MapMeterAddTranslator.xml";
            List<string> prueba = new List<string>
            (
                new string[] 
                {
                    "KVARH Register",
                    "KWH Register11111111111"
                }
             );

            XmlSerializer serializer = new XmlSerializer(typeof(MeterAdd));
            StreamReader reader = new StreamReader(path);
            MeterAdd meterAdd = (MeterAdd)serializer.Deserialize(reader);
            reader.Close();

            List<string> asd = meterAdd.map[19].variable.Split(';').ToList();
            //var qwe  = prueba.SequenceEqual(asd);

            IEnumerable<string> differenceQuery = asd.Except(prueba);
            foreach (string s in differenceQuery)
                Console.WriteLine(s);

        }
    }
}

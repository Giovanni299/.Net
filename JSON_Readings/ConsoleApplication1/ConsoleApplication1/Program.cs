using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    class Program
    {
        static List<readings> listReadings = new List<readings>();
        static DateTime startDate = new DateTime(2019, 04, 08, 0, 0, 0); ///Fecha de inicio de datos.
        static DateTime endDate = new DateTime(2019, 04, 09, 0, 0, 0); ///Fecha fin de datos.
        static string[] deviceID = new string[] {
                                                "9011"
                                                };
        //static string socketName = "0NXPQJG5HHESTSCSK8KA";
        static int[] ReadingDetail = new int[] { 3056309, 3056310 };
        static int canales = 1;
        static int interval = 15;

        ///********************************INFORMACIÓN PARA GENERAR HUECOS*********************
        static bool holes = false;
        static DateTime startDateHole = new DateTime(2019, 04, 08, 8, 0, 0); ///Fecha de inicio de datos.
        static int holeHour = 2;
        

        static void Main(string[] args)
        {
            //for (int devices = 0; devices < 5; devices++)
            //{
                DateTime fechaCalculada = startDate;
                TimeSpan ts = endDate - startDate;
                int totalIntervals = (int)(ts.TotalMinutes / interval);

                for (int i = 0; i < totalIntervals; i++)
                {
                    if (holes && fechaCalculada >= startDateHole && fechaCalculada < startDateHole.AddHours(holeHour))
                    {
                        fechaCalculada = fechaCalculada.AddMinutes(interval);
                        continue;
                    }

                    for (int j = 1; j <= canales; j++)
                    {
                        readings NewPatient = new readings()
                        {
                            ReadingDate = fechaCalculada.ToString("s"),
                            UtcDate = fechaCalculada.ToUniversalTime().ToString("s"),
                            ReadingDetail = 1,//ReadingDetail[j - 1],
                            //Demand = 100,
                            //ReadVal = 200,
                            //Usage = 100,
                            Value = fechaCalculada.Hour,
                            DSTApplied = false,
                            Duration = 0,
                            Flags = "",
                            Channel = j,
                            LogNumber = 1,
                            Second = 0,
                            Millisecond = 0,
                            SocketName = deviceID[0],
                            DeviceName = deviceID[0],
                            VariableName = 1,
                            Uom = 1,
                            ReadingType = 1,
                            ReadingOrigin = new ReadingsOrigin
                            {
                                Date = fechaCalculada.ToString("s"),
                                Workstation = "",
                                Type = 1
                            },
                            StringValue = "0.0",
                            Interval = interval
                        };

                        listReadings.Add(NewPatient);
                    }

                    fechaCalculada = fechaCalculada.AddMinutes(interval);
                }

                var jsonPatientList = JsonConvert.SerializeObject(listReadings);
                System.IO.File.WriteAllText(string.Format("D:\\Readings\\9011_1_{0}.json", DateTime.Now.Date.ToShortDateString().Replace('/', '_')), jsonPatientList);

           // }
            //XmlSerializer SerializerObj = new XmlSerializer(typeof(List<readings>));
            //TextWriter WriteFileStream = new StreamWriter(string.Format("D:\\Readings\\archivo_{0}.txt", DateTime.Now.Date.ToShortDateString().Replace('/', '_')));
            //SerializerObj.Serialize(WriteFileStream, listReadings);
            //WriteFileStream.Close();
        }

    }


}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<SocketsValidation> listSocket = new List<SocketsValidation>();
            Random ra = new Random();
            for (int i = 0; i < 4; i++)
            {
                SocketsValidation qwe = new SocketsValidation
                {
                    Id_Soc = i,
                    Is_Backup = ra.Next(0, 2)
                };

                listSocket.Add(qwe);
            }

            var asd = string.Join(";", listSocket.Select(x => string.Format("{0},{1}", x.Id_Soc, x.Is_Backup)) );


            Random r = new Random();
            string meters = "8601";
            var lines = meters.Split(',');
            string strFilePath = @"D:\Temp\\DevicesPRF.csv";
            StringBuilder sbOutput = new StringBuilder();

            sbOutput.AppendLine("Id_medidor;Fecha_lectura;Periodo_integracion;Dst_activo;Log;1;2;3;4");
            int interval = 60;
            DateTime startDate = new DateTime(2019, 02, 01, 16, 0 ,0);
            DateTime endDate = DateTime.Now;
            int numInterval = (int)((endDate -startDate).TotalMinutes) / interval;
            foreach(var meter in lines)
            {
                DateTime datePRF = startDate;
                for (int i=0; i< numInterval; i++)
                {
                    string data = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", meter, datePRF.ToString("dd/MM/yyyy HH:mm"), interval, 0, 1, r.Next(0, 99), r.Next(0, 99), r.Next(0, 99), r.Next(0, 99));
                    sbOutput.AppendLine(data);
                    datePRF = datePRF.AddMinutes(interval);

                }
            }

            // Create and write the csv file
            File.WriteAllText(strFilePath, sbOutput.ToString());

        }
    }
}

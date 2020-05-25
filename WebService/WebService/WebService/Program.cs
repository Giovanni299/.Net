using Primestone.ImportExport.Application.ImportExport.Main.IEE.Helpers;
using Primestone.ImportExport.Application.ImportExport.Main.PublisherService;
using Primestone.PrimeRead.Infrastructure.Cache.Impl.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebService.PublishService;

namespace WebService
{
    public class Program
    {
        /// <summary>
        /// The redis.
       /// </summary>
        static StackExchangeRedisHandler redis = new StackExchangeRedisHandler();

        /// <summary>
        /// The value tou.
        /// </summary>
        static SortedList<string, double> valueTOU = new SortedList<string, double>();

        static void Main(string[] args)
        {

            try
            {
                //Console.WriteLine("");
                //string path = @"C:\Users\giovanni.sanabria\Documents\ALUMBRADO\QA0005.txt";
                //ServiceReference1.BusinessSubsProcessClient qwe = new ServiceReference1.BusinessSubsProcessClient();
                //string[] lines = File.ReadAllLines(path);
                //qwe.Ping();
                //qwe.ProcessReadingsJSON(lines);
                //qwe.Close();



                //List<ReadingDTO> asd = new List<ReadingDTO>();
                //ReadingDTO qwe = new ReadingDTO()
                //{
                //    DeviceName = "123",
                //    Value = 12,
                //};

                //asd.Add(qwe);
                //PublishService.PublisherServiceClient publish = new PublishService.PublisherServiceClient();
                //publish.OnNewReadings(asd.ToArray(), 1);
                //publish.Close();

                redisbd();


            }
            catch (Exception e)
            {
                Console.WriteLine("");
            }
        }


        static void redisbd()
        {
            //valueTOU.Add("ON PEAK", 10);
            valueTOU.Add("OFF PEAK", 15.123156);
            valueTOU.Add("SUPER PEAK", 20.6546468);
            valueTOU.Add("TOTAL TOU", 45.4984984);

            TOUTotal touTotal = new TOUTotal()
            {
                DateTimeTOU = DateTime.Now,
                IDSocket = "QA0001",
                Message = "",
                ValueTOU = valueTOU,
                Vendor = "vendor"
            };

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"C:\temp\WriteLines.txt"))
            {
                //file.WriteLine(@"D:\WriteLines.txt");
                file.WriteLine("poseidon01:6379,password=PRIME");


                bool status = redis.Initialize("poseidon01:6379,password=PRIME", "1");
                if (redis.ExistsKey("TOUTotal"))
                {
                    file.WriteLine("ExistsKey TOUTotal");
                    redis.AddItemIntoCollection("TOUTotal", touTotal);
                }
                else
                {
                    List<object> listdataTOU = new List<object>();
                    listdataTOU.Add(touTotal);
                    redis.AddCollection("TOUTotal", listdataTOU);
                    file.WriteLine("AddCollection TOUTotal");
                }
                redis.IncrementCounter("TOUTotalCounter");

                var qwe = redis.GetCollection("TOUTotal", typeof(TOUTotal));
                var initial = redis.GetCollection("TOUInitial", typeof(TOUTotal));
                var counter = redis.Get("TOUTotalCounter", typeof(Int16));

                file.WriteLine(string.Format("GetCollection TOUTotal: {0}", qwe.Count()));
                file.WriteLine(string.Format("GetCollection TOUInitial: {0}", initial.Count()));
                if (counter == null)
                {
                    file.WriteLine(string.Format("TOUTotalCounter: {0}", 0));
                }
                else
                {
                    file.WriteLine(string.Format("TOUTotalCounter: {0}", (short)counter));
                }

                redis.DeleteCollection("TOUTotal");
                redis.DeleteCollection("TOUInitial");
                redis.Delete("TOUTotalCounter");

                qwe = redis.GetCollection("TOUTotal", typeof(TOUTotal));
                initial = redis.GetCollection("TOUInitial", typeof(TOUTotal));
                counter = redis.Get("TOUTotalCounter", typeof(Int16));
                file.WriteLine(string.Format("Delete GetCollection TOUTotal: {0}", qwe.Count()));
                file.WriteLine(string.Format("GetCollection TOUInitial: {0}", initial.Count()));
                if (counter == null)
                {
                    file.WriteLine(string.Format("Delete TOUTotalCounter: {0}", 0));
                }
                else
                {
                    file.WriteLine(string.Format("Delete TOUTotalCounter: {0}", (short)counter));
                }

                redis.Dispose();
            }
                
        }
    }
}

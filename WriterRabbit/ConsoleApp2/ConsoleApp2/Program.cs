namespace ConsoleApp2
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using Primestone.PrimeBus.Contracts;
    using Primestone.PrimeRead.Domain.Core.Entities.Ennumerations;
    using Primestone.PrimeRead.Infrastructure.Cache.Impl.Redis;
    using Primestone.PrimeRead.Infrastructure.CrossCutting.Core.IoC;
    using Primestone.PrimeRead.Infrastructure.Serialization.Contracts;
    using Primestone.PrimeRead.Services.Legacy;
    using Newtonsoft.Json;
    using System.Linq;
    using Primestone.PrimeRead.Infrastructure.Events;
    using Primestone.PrimeRead.Infrastructure.Events.Contracts;
    using Primestone.PrimeBus.Contracts.Importer;

    class Program
    {
        /// <summary>
        /// The redis
        /// </summary>
        private static StackExchangeRedisHandler redis = new StackExchangeRedisHandler();

        static void Main(string[] args)
        {
            //writeRedis();
            writeRabbit();
            //writeEvent();
        }

        private static void writeEvent()
        {
            ISerializer serializer = IoCFactory.Resolve<ISerializer>();
            IEventPublisher eventPublisher = IoCFactory.Resolve<IEventPublisher>();

            ImportLogDTO importLogDTO = new ImportLogDTO
            {
                Status = 1,
                DeviceId = "PRUEBA123",
                FileName = "PRUEBA.xml",
                FileType = "PRE",
                ImportDate = DateTime.Now,
                Message = "PRUEBA MESSAGE",
                TotalSeconds = 2,
                StartDate = DateTime.Now.AddMinutes(-2),
                EndDate = DateTime.Now
            };

            ImportEventDTO importEventDTO = new ImportEventDTO
            {
                importLog = importLogDTO
            };

            Event ev = new Event
            {
                Source = "UnPackerAdded",
                EventType = "LoadProfile",
                TriggeredAt = DateTime.Now,
                Data = serializer.Serialize(importEventDTO)
            };

            ////Escribe el evento en la cola.
            eventPublisher.PublishEvent(ev);
        }

        private static void writeRedis()
        {
            List<DataImporterDTO> objCollectionDataImporterDTO = new List<DataImporterDTO>();
            List<object> updateReading = new List<object>();
            if (!redis.IsConnected())
            {
                bool success = redis.Initialize(ConfigurationManager.AppSettings["cacheConnectionString"], ConfigurationManager.AppSettings["cacheDatabase"]);
            }

            //DataImporterDTO resultPC = new DataImporterDTO
            //{
            //    Device = "qwe",
            //    TypeRead = (int)PSVarType.ResidentialEvent
            //};

            //objCollectionDataImporterDTO.Add(resultPC);
            //string output = JsonConvert.SerializeObject(resultPC);
            //redis.AddCollection("PruebaXML", objCollectionDataImporterDTO);



            string keyName = "XCIR000001827 [20181009154435@6656].xml";
            DateTime keyDate = new DateTime(2018,10,10,9,27,32);
            int posExt = keyName.IndexOf('[');
            posExt = posExt > 0 ? posExt : keyName.IndexOf('.');
            keyName = string.Format("{0}_{1}", keyName.Remove(posExt).Trim(), keyDate.ToString("yyMMddhhmmss"));
            bool asdf = redis.ExistsKey(keyName);
            if (redis.ExistsKey(keyName))
            {
                var gfdhg= 1;
            }

            List<DataImporterDTO> listImporters = redis.GetCollection<DataImporterDTO>("PruebaXML");
            var objectRead = listImporters.FirstOrDefault(x => x.Device.Equals("qwes") && x.TypeRead.Equals(5));
            if (!(objectRead is null))
            {
                listImporters.Remove(objectRead);
                redis.RemoveItemFromCollection("PruebaXML", objectRead);
                objectRead.Status = 0;
                redis.AddItemIntoCollection("PruebaXML", objectRead);
                listImporters.Add(objectRead);
            }

            if (!(listImporters.Any(x => x.Status is null)))
            {
                var asd = 1;
                redis.Delete("PruebaXML");
            }
        }

        private static void writeRabbit()
        {
            ISerializer serializer = IoCFactory.Resolve<ISerializer>();
            DateTime dataDate = new DateTime(2018, 09, 16, 02, 37, 0);

            ReadingDTO read = new ReadingDTO()
            {
                Channel = 0,
                ReadingDate = dataDate,
                Value = 150,
                ReadingType = 4,
                Interval = 45,
                Demand = 43,
                DSTApplied = false,
                Duration = 0.0,
                Flags = string.Empty,
                //ReadingDetailId = 0, //936808,
                LogNumber = 1,
                Millisecond = 0,
                ReadingEndDate = new DateTime(),
                ReadVal = 45,
                Second = 0,
                SocketName = "ION8600",
                DeviceName = "6363",
                Usage = 42,
                UtcDate = dataDate.AddHours(5),
                UtcEndDate = dataDate.AddHours(5),
                VariableName = "UnpackerAMI14",
                VariableCustomName = "UnpackerAMI14",
                Uom = PSUoM.A,
                StringValue = "0.0",
                VariableCode = "4",
                Tenant = null,
                ReadingOrigin = new DataOriginDTO()
                {
                    Date = new DateTime(2017, 3, 2),
                    Workstation = string.Empty,
                    Type = (int)PSDataOriginType.CallPrimeReadEnterprise,
                    FileName = "FilePrueba.xml"
                }
            };

            byte[] result;
            ReadingDTO[] readings = new ReadingDTO[1];
            CallerInterface interf = new CallerInterface();

            //read.ReadingType = 1;
            //readings[0] = read;
            //result = serializer.Serialize<ReadingDTO[]>(readings);
            //interf.OnNewReadings(result, (int)PSVarType.MassMemmory);

            //read.ReadingType = 4;
            //readings[0] = read;
            //result = serializer.Serialize<ReadingDTO[]>(readings);
            //interf.OnNewReadings(result, (int)PSVarType.ResidentialRegister);

            read.ReadingType = 3;
            readings[0] = read;
            result = serializer.Serialize<ReadingDTO[]>(readings);
            interf.OnNewReadings(result, (int)PSVarType.Event);
        }
    }
}

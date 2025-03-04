﻿namespace PrimeStone.ReadingsGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Primestone.ReadingsGenerator.Entities;
    using Primestone.ReadingsGenerator.Helpers;
    using DevExpress.Spreadsheet;
    using Newtonsoft.Json;
    using Primestone.PrimeBus.Contracts;
    using Primestone.PrimeRead.Domain.Core.Entities.Ennumerations;
    using System.Timers;
    using Primestone.PrimeRead.Services.Legacy;
    using Primestone.PrimeRead.Infrastructure.CrossCutting.Core.IoC;
    using Primestone.PrimeRead.Infrastructure.Serialization.Contracts;

    public partial class PrincipalForm : Form
    {
        private string appPath = System.IO.Directory.GetCurrentDirectory();
        private string resoursesPath = @"..\..\Resourses";
        private static Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Dictionary<string, string> devices = new Dictionary<string, string>();
        int systemSize = 8175;
        Dictionary<int, ElectricSystem> systems = new Dictionary<int, ElectricSystem>();
        StringBuilder deviceFile = new StringBuilder();
        Scilab m_oSCilab = null;
        Dictionary<string, List<ReadingDTO>> readingsDTOLP = new Dictionary<string, List<ReadingDTO>>();
        Dictionary<string, List<ReadingDTO>> readingsDTORG = new Dictionary<string, List<ReadingDTO>>();
        Dictionary<string, List<ReadingDTO>> readingsDTOEV = new Dictionary<string, List<ReadingDTO>>();
        Dictionary<string, List<ReadingDTO>> readingsDTOLastRG = new Dictionary<string, List<ReadingDTO>>();
        Dictionary<string, List<ReadingDTO>> readingsDTOLastEV = new Dictionary<string, List<ReadingDTO>>();
        Dictionary<string, int> eventprobabilities = new Dictionary<string, int>();
        Dictionary<string, Dictionary<DateTime, List<Reading>>> readings = new Dictionary<string, Dictionary<DateTime, List<Reading>>>();
        private static System.Timers.Timer aTimer;
        DateTime dateOrigin;
        DateTime sinceDate;

        public PrincipalForm()
        {
            InitializeComponent();
            dtpInitialDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpFinalDate.Value = DateTime.Now.Date;
            textBox1.Text = string.Format("{0}\\PrimeSimulator\\Output", appPath);
            ReadTemplate();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReadTemplate()
        {
            var a = resoursesPath;
            string fileName = @"..\..\Resourses\TEMPLATE16CH";
            StreamReader reading = File.OpenText(fileName);
            string str;
            while ((str = reading.ReadLine()) != null)
            {
                deviceFile.AppendLine(str);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int totalDevices = (int)numSystems.Value * systemSize;
            devices.Clear();
            List<string> devs = new List<string>();
            if (File.Exists("Devices.txt"))
            {
                string text = File.ReadAllText("Devices.txt");
                devs = JsonConvert.DeserializeObject<List<string>>(text);
                devs.ForEach(t => devices.Add(t, t));
            }
            
            while (true)
            {
                
                string id = GetId(20);
                if (!devices.ContainsKey(id) && devices.Keys.Count < totalDevices)
                {
                    devices.Add(id, id);
                }

                if (devices.Keys.Count == totalDevices)
                {
                    break;
                }
            }

            string json = JsonConvert.SerializeObject(devices.Keys.ToList());
            File.WriteAllText("Devices.txt", json);
            Splitsystems();
            foreach (var key in systems.Keys)
            {
                var systemPath = string.Format("{0}\\PrimeSimulator\\Output\\System_{1}", appPath, key);
                Directory.CreateDirectory(systemPath);
                Directory.CreateDirectory(systemPath + "\\Devices");
                Directory.CreateDirectory(systemPath + "\\Readings");
                json = JsonConvert.SerializeObject(systems[key]);
                System.IO.File.WriteAllText(string.Format("{0}\\system.txt", systemPath), json);
                foreach (var device in systems[key].Devices)
                {
                    string deviceText = deviceFile.ToString();
                    System.IO.File.WriteAllText(string.Format("{0}\\Devices\\{1}", systemPath, device), deviceText.Replace("TEMPLATE", device));
                }
            }

            watch.Stop();
            var elapsedMs = watch.Elapsed.TotalMinutes;
            MessageBox.Show(elapsedMs.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        private void Splitsystems()
        {
            int j = 0;
            int i = 0;
            foreach (var key in devices.Keys)
            {
                if (!systems.Keys.Contains(i))
                {
                    int typicalCurve = random.Next(1, 3);
                    double factor = random.NextDouble() * 3.0;
                    systems.Add(i, new ElectricSystem() { SystemNumber = i, Typicalcurve = typicalCurve, TypicalCurveFactor = factor });
                }

                j++;
                systems[i].Devices.Add(key.ToString());
                if (j == systemSize)
                {
                    j = 0;
                    i++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idLength"></param>
        /// <returns></returns>
        private string GetId(int idLength)
        {
            return new string(Enumerable.Repeat(chars, idLength).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox1.Text = folderBrowserDialog1.SelectedPath;
        }

        private void btnLoadSystem_Click(object sender, EventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            systems.Clear();
            devices.Clear();
            var directories = Directory.GetDirectories(textBox1.Text);

            foreach (var directory in directories)
            {
                string text = System.IO.File.ReadAllText(string.Format("{0}\\system.txt", directory));
                ElectricSystem electricSystem = JsonConvert.DeserializeObject<ElectricSystem>(text);
                systems.Add(electricSystem.SystemNumber, electricSystem);
            }

            var elapsedMs = watch.Elapsed.TotalMinutes;
            MessageBox.Show(elapsedMs.ToString());
        }

        private void ExportReadingsToINTFiles(object sender, EventArgs e)
        {
            if (m_oSCilab == null)
            {
                m_oSCilab = new Scilab(true);
            }

            var sciLabPath = resoursesPath;
            var configFilePath = string.Format("{0}\\primeSimSettings.csv", sciLabPath);
            var scriptPath = string.Format("{0}\\PrimeSimLiteSCI6.sce", sciLabPath);
            string exec = string.Format(@"exec('{0}');", scriptPath);
            var directories = Directory.GetDirectories(textBox1.Text);
            int i = 0;
            tbExecutingTotal.Text = directories.Count().ToString();
            tbExecutingNumber.Text = i.ToString();
            Application.DoEvents();
            DateTime currentDate = dtpInitialDate.Value.Date;
                foreach (var directory in directories)
                {
                    Application.DoEvents();
                    var polishLPFiles = Directory.GetFiles(directory, "PolishLP*.xls");
                    string systemName = string.Empty;
                    if (polishLPFiles.Count() > 0)
                    {
                        systemName = polishLPFiles[0].Split('\\')[polishLPFiles[0].Split('\\').Count() - 1].Split('.')[0];
                    }

                    updateScriptFile(scriptPath, sciLabPath, directory);
                            
                    while (currentDate < dtpFinalDate.Value.Date)
                    {
                        tbExecutingDay.Text = currentDate.ToString();
                       
            
                        string readingsFilepath = string.Format("{0}\\Readings\\LP_Readings_{1}_{2}.csv", directory, systemName, sinceDate.Date.ToString("yyyy-M-dd"));
                        if (!File.Exists(readingsFilepath))
                        {
                            tbExecutingSystem.Text = directory.Split('\\')[directory.Split('\\').Count() - 1];
                            Application.DoEvents();
                            string settingsFile = string.Format("{0}\\primeSimSettings.csv", directory);
                            if (File.Exists(settingsFile))
                            {
                                UpdateConfigFileDate(settingsFile, directory, string.Format("{0}\\LoadPatterns", sciLabPath), currentDate);
                                File.Copy(settingsFile, configFilePath, true);
                                var result = m_oSCilab.SendScilabJob(exec);
                                if (result == 0)
                                {
                                
                                }
                            }
                        }

                        if (File.Exists(readingsFilepath))
                        {
                            Translatefiles(directory);
                        }

                        i++;
                        tbExecutingNumber.Text = i.ToString();
                        currentDate = currentDate.AddDays(1);
                    }
                }

                
        }


        private void GetingReadings(string directory)
        {
            var readingFiles = Directory.GetFiles(directory, "LP*.*");
            foreach (var file in readingFiles)
            {
                TranslateFile(file);
                if (!Directory.Exists(directory + "\\Done"))
                {
                    Directory.CreateDirectory(directory + "\\Done");
                }

                string fileName = Path.GetFileName(file);
                File.Move(file, string.Format("{0}\\Done\\{1}", directory, fileName));
            }

            WriteINTFiles(string.Format("{0}\\Readings", directory));

        }

        private void Translatefiles(string directory)
        {
            var readingFiles = Directory.GetFiles(directory, "LP*.*");
            foreach (var file in readingFiles)
            {
                TranslateFile(file);
                if (!Directory.Exists(directory + "\\Done"))
                {
                    Directory.CreateDirectory(directory + "\\Done");
                }

                string fileName = Path.GetFileName(file);
                File.Move(file, string.Format("{0}\\Done\\{1}", directory, fileName));
            }

            WriteINTFiles(string.Format("{0}\\Readings", directory));
        }

        private void GetTotalReadingsDTO(string file, DateTime dateOrigin, DateTime init)
        {
            List<ReadingDTO> dtos = new List<ReadingDTO>();
            var reader = new StreamReader(File.OpenRead(file));
            int i = 0;
            string date = string.Empty;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                i++;
                if (i == 1)
                {
                    continue;
                }

                line = line.Replace(',', ';');
                var values = line.Split(';');
                string device = values[0].Trim();
                string variable = values[4].Trim();
                date = values[2].Trim().Replace('/', '-').Split(' ')[0];

                ReadingDTO reading = MapReadingDTO(line, dateOrigin);

                if (reading.ReadingDate <= dateOrigin && reading.ReadingDate > init)
                {
                    if (!readingsDTOLP.ContainsKey(reading.DeviceName))
                    {
                        readingsDTOLP.Add(reading.DeviceName, new List<ReadingDTO>());
                        readingsDTOLastEV.Add(reading.DeviceName, new List<ReadingDTO>());
                        readingsDTOLastRG.Add(reading.DeviceName, new List<ReadingDTO>());
                    }


                    readingsDTOLP[reading.DeviceName].Add(reading);
                }
            }

            reader.Dispose();
            GC.Collect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="prefix"></param>
        private void TranslateFile(string file)
        {
            var reader = new StreamReader(File.OpenRead(file));
            int i = 0;
            string date = string.Empty;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                i++;
                if (i == 1)
                {
                    continue;
                }

                line = line.Replace(',', ';');
                var values = line.Split(';');
                string device = values[0].Trim();
                string variable = values[4].Trim();
                date = values[2].Trim().Replace('/', '-').Split(' ')[0];

                Reading reading = MapReading(line);
                DateTime monthDate = new DateTime(reading.Date.Year, reading.Date.Month, 1);
                if (!readings.ContainsKey(reading.DeviceId))
                {
                    readings.Add(reading.DeviceId, new Dictionary<DateTime, List<Reading>>());
                }

                if (!readings[reading.DeviceId].ContainsKey(monthDate))
                {
                    readings[reading.DeviceId].Add(monthDate, new List<Reading>());
                }

                readings[reading.DeviceId][monthDate].Add(reading);
            }

            reader.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private ReadingDTO MapReadingDTO(string line, DateTime dateOrigin)
        {
            var values = line.Split(';');
            string device = values[0].Trim();
            string variable = values[4].Trim();
            DateTime date = DateTime.Now;
            DateTime.TryParseExact(values[2].Trim(), "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out date);
            int log = int.Parse(values[3].Trim());
            int channel = int.Parse(values[4].Trim());
            if (date.Month == 5 && channel > 10)
            {

            }
            double value = 0;
            double.TryParse(values[5].Trim().Replace(".", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator), out value);
            ReadingDTO newReading = new ReadingDTO()
            {
                DeviceName = device,
                DSTApplied = false,
                Flags = string.Empty,
                LogNumber = log,
                ReadingDate = date,
                ReadingOrigin = new DataOriginDTO() { Date = dateOrigin, Type = (int)PSDataOriginType.CallPrimeReadEnterprise, Workstation = "DESARROLLO-49" },
                ReadingType = (int)PSVarType.MassMemmory,
                ReadVal = value,
                Usage = value,
                Channel = channel
            };

            return newReading;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private Reading MapReading(string line)
        {
            var values = line.Split(';');
            string device = values[0].Trim();
            string variable = values[4].Trim();
            DateTime date = DateTime.Now;
            DateTime.TryParseExact(values[2].Trim(), "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out date);
            int log = int.Parse(values[3].Trim());
            int channel = int.Parse(values[4].Trim());
            if (date.Month == 5 && channel > 10)
            {

            }
            double value = 0;
            double.TryParse(values[5].Trim().Replace(".", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator), out value);
            Reading newReading = new Reading()
            {
                channel = channel,
                Date = date,
                DeviceId = device,
                Log = log,
                Value = value
            };

            return newReading;
        }

        /// <summary>
        /// 
        /// </summary>
        private void WritePRFFiles(string mainPath)
        {
            string letters = "abcdefghijklmnopqrstuvwxyz";
            int i = 0;
            int j = 0;
            foreach (var deviceId in readings.Keys)
            {
                i = 0;
                j = 0;
                foreach (var date in readings[deviceId].Keys)
                {
                    string folderPath = string.Format("{0}\\{1}", mainPath, date.ToString("yyyyMMdd"));
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string prefix = letters.ToCharArray()[i].ToString() + letters.ToCharArray()[j].ToString();
                    j++;
                    if (j == letters.ToCharArray().Count())
                    {
                        i++;
                        j = 0;
                    }

                    string fileName = string.Format("{0}\\{1}_{2}_{3}.csv", folderPath, prefix, deviceId, date.ToString("yyyyMM"));
                    bool first = true;
                    using (TextWriter writer = File.CreateText(fileName))
                    {
                        var orderedReadings = readings[deviceId][date].OrderBy(c => c.channel).ThenBy(n => n.Date).ToList();
                        foreach (var reading in orderedReadings)
                        {
                            if (first)
                            {
                                first = false;
                                writer.WriteLine("Id_medidor;Dst_activo;Fecha_lectura;Log;Canal;Valor");
                            }

                            string line = string.Format("{0};{1};{2};{3};{4};{5}", reading.DeviceId, 0, reading.Date.ToString("dd/MM/yyyy HH:mm"), reading.Log, reading.channel, reading.Value.ToString().Replace(",", "."));
                            writer.WriteLine(line); // "sb" is the StringBuilder
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void WriteINTFiles(string mainPath)
        {
            foreach (var deviceId in readings.Keys)
            {
                foreach (var date in readings[deviceId].Keys)
                {
                    string folderPath = string.Format("{0}\\{1}", mainPath, date.ToString("yyyyMMdd"));
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fileName = string.Format("{0}\\{1}.INT", folderPath, deviceId);
                    using (TextWriter writer = File.CreateText(fileName))
                    {
                        var orderedReadings = readings[deviceId][date].OrderBy(c => c.channel).ThenBy(n => n.Date).ToList();
                        var dates = orderedReadings.Select(x => x.Date).Distinct();
                        foreach (var readingDate in dates)
                        {
                            string line = readingDate.AddMinutes(60).ToString("HHmm");
                            var dateReadings = orderedReadings.Where(x => x.Date == readingDate).OrderBy(x => x.channel);
                            foreach (var reading in dateReadings)
                            {
                                line = string.Format("{0}   {1}", line, reading.Value);
                            }

                            writer.WriteLine(line); // "sb" is the StringBuilder
                        }
                    }
                }
            }
        }

        private void UpdateConfigFileDate(string file, string systemPath, string readingPath, DateTime readingsDate)
        {
            List<String> lines = new List<String>();
            if (File.Exists(file))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("Year"))
                        {
                            line = string.Format("Year;{0}", readingsDate.Year);
                        }
                        else if (line.StartsWith("Month"))
                        {
                            line = string.Format("Month;{0}", readingsDate.Month);
                        }
                        else if (line.StartsWith("Start day"))
                        {
                            line = string.Format("Start day;{0}", readingsDate.Day);
                        }
                        else if (line.StartsWith("End day"))
                        {
                            line = string.Format("End day;{0}", readingsDate.Day);
                        }
                        else if (line.StartsWith("System path"))
                        {
                            line = string.Format("System path;{0}", systemPath);
                        }
                        else if (line.StartsWith("Reading path"))
                        {
                            line = string.Format("Reading path;{0}", readingPath);
                        }
                        else if (line.StartsWith("Output path"))
                        {
                            line = string.Format("Output path;{0}", string.Format("{0}\\Readings", systemPath));
                        }

                        lines.Add(line);
                    }
                }

                using (StreamWriter writer = new StreamWriter(file, false))
                {
                    foreach (String line in lines)
                        writer.WriteLine(line);
                }
            }
        }

        private void updateScriptFile(string scriptPath, string sciLabPath, string systemPath)
        {
            List<String> lines = new List<String>();

            if (File.Exists(scriptPath))
            {
                using (StreamReader reader = new StreamReader(scriptPath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("filePath "))
                        {
                            line = string.Format("filePath  = '{0}';", sciLabPath);
                        }
                        else if (line.StartsWith("filePath2 "))
                        {
                            line = string.Format("filePath2  = '{0}';", string.Format("{0}\\PrimeSimulator", appPath));
                        }
                        else if (line.StartsWith("diary"))
                        {
                            line = string.Format("diary('{0}\\Trailfile.txt','new','prefix=YYYY-MM-DD hh:mm:ss');", systemPath);
                        }

                        lines.Add(line);
                    }
                }

                using (StreamWriter writer = new StreamWriter(scriptPath, false))
                {
                    foreach (String line in lines)
                        writer.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Updates the file.
        /// </summary>
        /// <param name="path">The path.</param>
        private void UpdateSettingsFile(string path, ElectricSystem electricSystem, string configFinalPath, string systemPath, string systemFileName, DateTime readingsDate)
        {
            string simulatorPath = string.Format("{0}\\PrimeSimulator", System.IO.Directory.GetCurrentDirectory());

            List<String> lines = new List<String>();

            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("Year"))
                        {
                            line = string.Format("Year;{0}", readingsDate.Year);
                        }
                        else if (line.StartsWith("Month"))
                        {
                            line = string.Format("Month;{0}", readingsDate.Month);
                        }
                        else if (line.StartsWith("Start day"))
                        {
                            line = string.Format("Start day;{0}", readingsDate.Day);
                        }
                        else if (line.StartsWith("End day"))
                        {
                            line = string.Format("End day;{0}", readingsDate.Day);
                        }
                        else if (line.StartsWith("System path"))
                        {
                            line = string.Format("System path;{0}", systemPath);
                        }
                        else if (line.StartsWith("System name"))
                        {
                            line = string.Format("System name;{0}", systemFileName);
                        }
                        else if (line.StartsWith("Reading path"))
                        {
                            line = string.Format("Reading path;{0}", string.Format("{0}\\LoadPatterns", simulatorPath));
                        }
                        else if (line.StartsWith("Output path"))
                        {
                            line = string.Format("Output path;{0}", string.Format("{0}Readings", systemPath));
                        }
                        else if (line.StartsWith("multiplier"))
                        {
                            line = string.Format("multiplier;{0}", (electricSystem.TypicalCurveFactor / 3.0).ToString().Replace(",", "."));
                        }

                        lines.Add(line);
                    }
                }

                using (StreamWriter writer = new StreamWriter(configFinalPath, false))
                {
                    foreach (String line in lines)
                        writer.WriteLine(line);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string systemPath = string.Format("{0}\\PrimeSimulator\\Systems\\{1}", System.IO.Directory.GetCurrentDirectory(), "PolishLP1.xls");
            // Load a workbook from a stream. 
            using (FileStream stream = new FileStream(systemPath, FileMode.Open))
            {
                spreadsheetControl1.LoadDocument(stream, DocumentFormat.Xls);
            }

            IWorkbook workbook = spreadsheetControl1.Document;
            var workSheet = workbook.Worksheets["Sockets"];
            for (int i = 0; i < 50; i++)
            {
                workSheet[i, 0].SetValue(i);
            }

            spreadsheetControl1.SaveDocument(systemPath);
        }

        private void btnCalculateSystems_Click(object sender, EventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string configFile = "primeSimSettings.csv";
            //string configFilePath = string.Format(@"{0}\PrimeSimulator\{1}", appPath, configFile);
            string configFilePath = string.Format(@"{0}\{1}", resoursesPath, configFile);
            foreach (var system in systems)
            {
                string systemFinalPath = string.Format("{0}\\PrimeSimulator\\Output\\System_{1}\\", appPath, system.Value.SystemNumber);
                string systemFileName = string.Format("PolishLP{0}.xls", system.Value.Typicalcurve);

                string configFinalPath = string.Format("{0}\\PrimeSimulator\\Output\\System_{1}\\primeSimSettings.csv", appPath, system.Value.SystemNumber);
                UpdateSystemFile(system.Value, systemFinalPath + systemFileName);
                UpdateSettingsFile(configFilePath, system.Value, configFinalPath, systemFinalPath, systemFileName, DateTime.Now);
            }

            var elapsedMs = watch.Elapsed.TotalMinutes;
            MessageBox.Show(elapsedMs.ToString());
        }

        private void UpdateSystemFile(ElectricSystem electricSystem, string finalPath)
        {
            string systemPath = string.Format("{0}\\Systems\\{1}", resoursesPath, "PolishLP1.xls");
            // Load a workbook from a stream. 
            using (FileStream stream = new FileStream(systemPath, FileMode.Open))
            {
                spreadsheetControl1.LoadDocument(stream, DocumentFormat.Xls);
            }

            IWorkbook workbook = spreadsheetControl1.Document;
            var workSheet = workbook.Worksheets["Sockets"];
            workSheet.Import(electricSystem.Devices.ToArray(), 0, 0, true);

            spreadsheetControl1.SaveDocument(finalPath);
            spreadsheetControl1.CreateNewDocument();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            DisableControls();
            GetReadingsDto();
            int since = Convert.ToInt32(numericUpDown2.Value);
            sinceDate = dateOrigin.AddDays(-1 * since);
            CreateEvents(dateOrigin);
            Createregs(dateOrigin);
            CallerInterface interf = new CallerInterface();
            var serializer = IoCFactory.Resolve<ISerializer>();
            foreach (var readss in readingsDTOLP)
            {
                byte[] dtosresult = serializer.Serialize<List<ReadingDTO>>(readss.Value);
                interf.OnNewReadings(dtosresult, (int)PSVarType.MassMemmory);
                try
                {
                    var t = readingsDTOEV[readss.Key];
                    dtosresult = serializer.Serialize<List<ReadingDTO>>(t);
                    interf.OnNewReadings(dtosresult, (int)PSVarType.Event);
                }
                catch
                {
                }

                try
                {
                    var t = readingsDTORG[readss.Key];
                    dtosresult = serializer.Serialize<List<ReadingDTO>>(t);
                    interf.OnNewReadings(dtosresult, (int)PSVarType.Register);
                }
                catch
                {
                }
            }

            using (TextWriter writer = File.CreateText(string.Format(@"{0}\LastEventsJson.txt", appPath)))
            {
                foreach (var readss in readingsDTOEV)
                {

                    string dtosresult = serializer.Serialize<List<ReadingDTO>>(readss.Value).ToString();
                    writer.WriteLine(dtosresult);

                }
            }

            using (TextWriter writer = File.CreateText(string.Format(@"{0}\LastRegsJson.txt", appPath)))
            {
                foreach (var readss in readingsDTOLastEV)
                {
                    string dtosresult = serializer.Serialize<List<ReadingDTO>>(readss.Value).ToString();
                    writer.WriteLine(dtosresult);
                }
            }

            int every = Convert.ToInt32(this.numericUpDown1.Value);
            TimerStart(every);
            GC.Collect();
        }

        public void TimerStart(int every)
        {
            aTimer = new System.Timers.Timer();
            aTimer.Interval = every*3600*1000;
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            GetReadingsDto();
            int since = Convert.ToInt32(numericUpDown2.Value);
            sinceDate = dateOrigin.AddDays(-1 * since);
            var reads = readingsDTOLP.Where(t => t.Value.Any(y => y.ReadingDate >= sinceDate && y.ReadingDate <= dateOrigin));
            CallerInterface interf = new CallerInterface();
            foreach (var readss in reads)
            {
                string readstr = JsonConvert.SerializeObject(readss.Value.ToArray(), Newtonsoft.Json.Formatting.None);
                interf.OnNewReadings(readstr, (int)PSVarType.MassMemmory);
            }
        }

        private void GetReadingsDto()
        {
            var sciLabPath = resoursesPath;
            var configFilePath = string.Format("{0}\\primeSimSettings.csv", sciLabPath);
            var scriptPath = string.Format("{0}\\PrimeSimLiteSCI6.sce", sciLabPath);
            string exec = string.Format(@"exec('{0}');", scriptPath);
            var directories = Directory.GetDirectories(textBox1.Text);
            tbExecutingTotal.Text = directories.Count().ToString();
            Application.DoEvents();
            dateOrigin = DateTime.Now;
            int since = Convert.ToInt32(numericUpDown2.Value);
            sinceDate = dateOrigin.AddDays(-1 * since);
            while (sinceDate <= dateOrigin)
            {
                Application.DoEvents();
                foreach (var directory in directories)
                {
                    var polishLPFiles = Directory.GetFiles(directory, "PolishLP*.xls");
                    string systemName = string.Empty;
                    if (polishLPFiles.Count() > 0)
                    {
                        systemName = polishLPFiles[0].Split('\\')[polishLPFiles[0].Split('\\').Count() - 1].Split('.')[0];
                    }

                    string readingsFilepath = string.Format("{0}\\Readings\\LP_Readings_{1}_{2}.csv", directory, systemName, sinceDate.Date.ToString("yyyy-M-dd"));
                    if (!File.Exists(readingsFilepath))
                    {
                        tbExecutingSystem.Text = directory.Split('\\')[directory.Split('\\').Count() - 1];
                        Application.DoEvents();
                        updateScriptFile(scriptPath, sciLabPath, directory);
                        string settingsFile = string.Format("{0}\\primeSimSettings.csv", directory);
                        if (File.Exists(settingsFile))
                        {
                            if (m_oSCilab == null)
                            {
                                m_oSCilab = new Scilab(true);
                            }

                            UpdateConfigFileDate(settingsFile, directory, string.Format("{0}\\LoadPatterns", sciLabPath), sinceDate.Date);
                            File.Copy(settingsFile, configFilePath, true);
                            var result = m_oSCilab.SendScilabJob(exec);
                            if (result != 0)
                            {
                                ///FAlle o reintente o continue
                            }
                        }

                    }

                    if (File.Exists(readingsFilepath))
                    {
                        GetTotalReadingsDTO(readingsFilepath, dateOrigin, sinceDate);
                    }
                }

                sinceDate = sinceDate.Date.AddDays(1);
            }

            GC.Collect();
        }

        private void DisableControls()
        {
            button1.Enabled = false;
            numSystems.Enabled = false;
            textBox1.Enabled = false;
            button2.Enabled = false;
            btnLoadSystem.Enabled = false;
            btnCalculateReadings.Enabled = false;
            button4.Enabled = false;
            spreadsheetControl1.Enabled = false;
            btnCalculateSystems.Enabled = false;
            dtpInitialDate.Enabled = false;
            dtpFinalDate.Enabled = false;
            tbExecutingDay.Enabled = false;
            tbExecutingSystem.Enabled = false;
            tbExecutingNumber.Enabled = false;
            tbExecutingTotal.Enabled = false;
            button3.Enabled = false;
            button6.Enabled = false;
            numericUpDown1.Enabled = false;
            numericUpDown2.Enabled = false;
        }

        private void EnableControls()
        {
            button1.Enabled = true;
            numSystems.Enabled = true;
            textBox1.Enabled = true;
            button2.Enabled = true;
            btnLoadSystem.Enabled = true;
            btnCalculateReadings.Enabled = true;
            button4.Enabled = true;
            spreadsheetControl1.Enabled = true;
            btnCalculateSystems.Enabled = true;
            dtpInitialDate.Enabled = true;
            dtpFinalDate.Enabled = true;
            tbExecutingDay.Enabled = true;
            tbExecutingSystem.Enabled = true;
            tbExecutingNumber.Enabled = true;
            tbExecutingTotal.Enabled = true;
            button3.Enabled = true;
            button6.Enabled = true;
            numericUpDown1.Enabled = true;
            numericUpDown2.Enabled = true;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            aTimer.Stop();
            aTimer.Dispose();
            EnableControls();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DisableControls();
            GetReadingsDto();
            int since = Convert.ToInt32(numericUpDown2.Value);
            sinceDate = dateOrigin.AddDays(-1 * since);
            CreateEvents(dateOrigin);
            Createregs(dateOrigin);
            var serializer = IoCFactory.Resolve<ISerializer>();
            foreach (var readss in readingsDTOLP)
            {
                byte[] dtosresult = serializer.Serialize<List<ReadingDTO>>(readss.Value);
                FileStream fs = File.Create(string.Format(@"\\DESARROLLO-49\info Dianeth\ReadingsJson_{0}.txt", readss.Key), dtosresult.Length, FileOptions.None);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(dtosresult);
                bw.Close();
                fs.Close();
            }

            GC.Collect();
            foreach (var readss in readingsDTORG)
            {
                byte[] dtosresult = serializer.Serialize<List<ReadingDTO>>(readss.Value);
                string s = Encoding.UTF8.GetString(dtosresult, 0, dtosresult.Length);
                FileStream fs = File.Create(string.Format(@"\\DESARROLLO-49\info Dianeth\ReadingsJsonRG_{0}.txt", readss.Key), dtosresult.Length, FileOptions.None);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(dtosresult);
                bw.Close();
                fs.Close();
            }

            GC.Collect();
            foreach (var readss in readingsDTOEV)
            {
                byte[] dtosresult = serializer.Serialize<List<ReadingDTO>>(readss.Value);
                string s = Encoding.UTF8.GetString(dtosresult, 0, dtosresult.Length);
                FileStream fs = File.Create(string.Format(@"\\DESARROLLO-49\info Dianeth\ReadingsJsonEV_{0}.txt", readss.Key), dtosresult.Length, FileOptions.None);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(dtosresult);
                bw.Close();
                fs.Close();
            }

            GC.Collect();
            //using (TextWriter writer = File.CreateText(string.Format(@"{0}\LastEventsJson.txt", appPath)))
            //{
            //    foreach (var readss in readingsDTOEV)
            //    {
            //        byte[] dtosresult = serializer.Serialize<List<ReadingDTO>>(readss.Value);
            //        string s = Encoding.UTF8.GetString(dtosresult, 0, dtosresult.Length);
            //        writer.WriteLine(s);
            //    }
            //}

            //using (TextWriter writer = File.CreateText(string.Format(@"{0}\LastRegsJson.txt", appPath)))
            //{
            //    foreach (var readss in readingsDTOLastEV)
            //    {
            //        byte[] dtosresult = serializer.Serialize<List<ReadingDTO>>(readss.Value);
            //        string s = Encoding.UTF8.GetString(dtosresult, 0, dtosresult.Length);
            //        writer.WriteLine(s);
            //    }
            //}

            int every = Convert.ToInt32(this.numericUpDown1.Value);
            TimerStart(every);
            GC.Collect();
        }

        private void CreateEvents(DateTime originDate)
        {
            var reader = new StreamReader(File.OpenRead(string.Format(@"..\..\Resourses\Evs{0}.txt", 1)));
            while (!reader.EndOfStream)
            {
                string[] line = reader.ReadLine().Split(',');
                eventprobabilities.Add(line[0], Convert.ToInt32(line[1]));
            }

            reader.Dispose();
            if (File.Exists(string.Format(@"{0}\LastEventsJson.txt", appPath)))
            {
                var reader2 = new StreamReader(File.OpenRead(string.Format(@"{0}\LastEventsJson.txt", appPath)));
                while (!reader2.EndOfStream)
                {
                    string line = reader2.ReadLine();
                    ReadingDTO[] readsdtos = JsonConvert.DeserializeObject<ReadingDTO[]>(line);
                    foreach (var ev in readsdtos)
                    {
                        readingsDTOLastEV[ev.DeviceName].Add(ev);
                    }
                }
            }

            if (File.Exists(string.Format(@"{0}\LastRegsJson.txt", appPath)))
            {
                var reader3 = new StreamReader(File.OpenRead(string.Format(@"{0}\LastRegsJson.txt", appPath)));
                while (!reader3.EndOfStream)
                {
                    string line = reader3.ReadLine();
                    ReadingDTO[] readsdtos = JsonConvert.DeserializeObject<ReadingDTO[]>(line);
                    foreach (var rg in readsdtos)
                    {
                        readingsDTOLastRG[rg.DeviceName].Add(rg);
                    }
                }
            }

            foreach (var reds in readingsDTOLP)
            {
                List<ReadingDTO> lasevs = readingsDTOLastEV[reds.Key];
                var readinggg = reds.Value.GroupBy(rr => rr.ReadingDate, (d, rrr) => new { Date = d, Readsss = rrr.ToList() });
                if (lasevs.Count() > 0)
                {
                    readinggg = reds.Value.Where(rr => rr.ReadingDate > lasevs.Max(r => r.ReadingEndDate)).GroupBy(rr => rr.ReadingDate, (d, rrr) => new { Date = d, Readsss = rrr.ToList() });
                    if (!readingsDTOEV.ContainsKey(reds.Key))
                    {
                        readingsDTOEV.Add(reds.Key, new List<ReadingDTO>());
                    }

                    readingsDTOEV[reds.Key].AddRange(lasevs);
                }
                
                foreach (var readingg in readinggg)
                {
                    var hh = readingg.Readsss.Where(r => r.Flags.Contains("IZ") || (r.ReadVal == 0 && r.Channel >= 5 && r.Channel <= 7));
                    if (!readingg.Readsss.Any(r => r.Flags.Contains("IZ") || (r.ReadVal == 0 && r.Channel >= 5 && r.Channel <= 7)))
                    foreach (var aa in eventprobabilities)
                    {
                        int rand = new Random().Next(0, 100);
                        bool result = false;
                        if (aa.Value > rand && aa.Value != 0)
                        {
                            result = EvAnalize("OUTAGE", string.Empty, readingg.Readsss, aa.Key, aa.Value, rand, 60);
                            if (!result)
                            {
                                result = EvAnalize("CUT", "A", readingg.Readsss, aa.Key, aa.Value, rand, 60) || EvAnalize("CUT", "B", readingg.Readsss, aa.Key, aa.Value, rand, 60) || EvAnalize("CUT", "C", readingg.Readsss, aa.Key, aa.Value, rand, 60);
                                if (!result)
                                {
                                    result = EvAnalize("SAG", "A", readingg.Readsss, aa.Key, aa.Value, rand) || EvAnalize("SAG", "B", readingg.Readsss, aa.Key, aa.Value, rand) || EvAnalize("SAG", "C", readingg.Readsss, aa.Key, aa.Value, rand);
                                    if (!result)
                                    {
                                        result = EvAnalize("SWELL", "A", readingg.Readsss, aa.Key, aa.Value, rand) || EvAnalize("SWELL", "B", readingg.Readsss, aa.Key, aa.Value, rand) || EvAnalize("SWELL", "C", readingg.Readsss, aa.Key, aa.Value, rand);
                                        if (!result)
                                        {
                                            var st = readingg.Readsss.FirstOrDefault().ReadingDate.AddMinutes(new Random().Next(0, 15));
                                            AddEvent(dateOrigin, 1, st, st, 0, readingg.Readsss.FirstOrDefault().DeviceName, aa.Key);
                                            break;
                                        }
                                    }
                                }
                            }

                            if (result)
                                break;
                        }
                    }
                }
                
            }

            readingsDTOLastEV = readingsDTOEV;
            GC.Collect();
        }

        private void Createregs(DateTime originDate)
        {
            //var reader = new StreamReader(File.OpenRead(string.Format(@"..\..\Resourses\Evs{0}", value)));
            //while (!reader.EndOfStream)
            //{
            //    string[] line = reader.ReadLine().Split(',');
            //    eventprobabilities.Add(line[0], Convert.ToInt32(line[1]));
            //}

            //reader.Dispose();

            foreach (var reds in readingsDTOLP)
            {
                var readingsbd = reds.Value.GroupBy(rr => rr.ReadingDate.Date, (d, rrr) => new { Date = d, readbych = rrr.GroupBy(s => s.Channel, (ch, sum) => new { Ch = ch, Sum = sum.Sum(c => c.ReadVal), Prom = sum.Sum(c => c.ReadVal) / 24, Stamp = sum.Max(c => c.ReadingDate), Data = sum}) });
                var readingsbh = reds.Value.GroupBy(rr => rr.ReadingDate.ToString("DDHH"), (d, rrr) => new { Date = d, readbych = rrr.GroupBy(s => s.Channel, (ch, sum) => new { Ch = ch, Sum = sum.Sum(c => c.ReadVal), Prom = sum.Sum(c => c.ReadVal) / 24, Stamp = sum.Max(c => c.ReadingDate), Data = sum }) });
                List<ReadingDTO> lastres = new List<ReadingDTO>();
                List<ReadingDTO> a = readingsDTOLastRG[reds.Key];
                var aa = a.Where(r => r.VariableName.Contains("_acum"));
                var readingsbch = reds.Value.GroupBy(rr => rr.Channel, (c, r) => new { Ch = c, Lastreading = r.Where(rrrr => rrrr.ReadingDate <= originDate).OrderBy(rrrr => rrrr.ReadingDate).Last(), sum = r.Where(rf => rf.ReadingDate <= originDate).Sum(rf => rf.ReadVal), max = r.Max(rf => rf.ReadVal), min = r.Min(rf => rf.ReadVal), R = r });
                if (aa.Count() == 16)
                {
                    readingsbch = reds.Value.GroupBy(rr => rr.Channel, (c, r) => new { Ch = c, Lastreading = r.Where(rrrr => rrrr.ReadingDate <= originDate).OrderBy(rrrr => rrrr.ReadingDate).Last(), sum = r.Where(rf => rf.ReadingDate > aa.Where(e => e.VariableName == string.Format("Ch{0}_acum", c)).FirstOrDefault().ReadingDate && rf.ReadingDate <= originDate).Sum(rf => rf.ReadVal), max = r.Max(rf => rf.ReadVal), min = r.Min(rf => rf.ReadVal), R = r });
                }

                foreach (var readingbd in readingsbd)
                {
                    foreach (var yy in readingbd.readbych)
                    {
                        AddReg(dateOrigin, yy.Prom, yy.Stamp, reds.Key, string.Format("Ch{0}_prom24h", yy.Ch));
                        AddReg(dateOrigin, yy.Sum, yy.Stamp, reds.Key, string.Format("Ch{0}_acum24h", yy.Ch));
                        double max24 = yy.Data.Max(d => d.ReadVal);
                        AddReg(dateOrigin, max24, yy.Data.Where(d => d.ReadVal == max24).FirstOrDefault().ReadingDate, reds.Key, string.Format("Ch{0}_max24h", yy.Ch));
                        double min24 = yy.Data.Min(d => d.ReadVal);
                        AddReg(dateOrigin, min24, yy.Data.Where(d => d.ReadVal == min24).FirstOrDefault().ReadingDate, reds.Key, string.Format("Ch{0}_min24h", yy.Ch));
                    }
                }

                foreach (var readingbh in readingsbh)
                {
                    foreach (var yy in readingbh.readbych)
                    {
                        AddReg(dateOrigin, yy.Prom, yy.Stamp, reds.Key, string.Format("Ch{0}_promh", yy.Ch));
                        AddReg(dateOrigin, yy.Sum, yy.Stamp, reds.Key, string.Format("Ch{0}_acumh", yy.Ch));
                        double maxh = yy.Data.Max(d => d.ReadVal);
                        AddReg(dateOrigin, maxh, yy.Data.Where(d => d.ReadVal == maxh).FirstOrDefault().ReadingDate, reds.Key, string.Format("Ch{0}_maxh", yy.Ch));
                        double minh = yy.Data.Min(d => d.ReadVal);
                        AddReg(dateOrigin, minh, yy.Data.Where(d => d.ReadVal == minh).FirstOrDefault().ReadingDate, reds.Key, string.Format("Ch{0}_minh", yy.Ch));
                    }
                }

                foreach (var tt in readingsbch) 
                {
                    AddReg(dateOrigin, tt.min, tt.R.Where(t => t.ReadVal == tt.min).FirstOrDefault().ReadingDate, reds.Key, string.Format("Ch{0}_min", tt.Ch));
                    AddReg(dateOrigin, tt.max, tt.R.Where(t => t.ReadVal == tt.max).FirstOrDefault().ReadingDate, reds.Key, string.Format("Ch{0}_max", tt.Ch));
                    AddReg(dateOrigin, tt.sum, originDate, reds.Key, string.Format("Ch{0}_acum", tt.Ch));
                    AddReg(dateOrigin, tt.Lastreading.ReadVal, originDate, reds.Key, string.Format("Ch{0}_inst", tt.Ch));
                    if (a.Any(r => r.VariableName == string.Format("Ch{0}_min", tt.Ch) && r.ReadVal > tt.min))
                    {
                        lastres.Add(createlastreg(dateOrigin, tt.min, tt.R.Where(t => t.ReadVal == tt.min).FirstOrDefault().ReadingDate, reds.Key, string.Format("Ch{0}_min", tt.Ch)));
                    }
                    else 
                    {
                        lastres.Add(a.Where(r => r.VariableName == string.Format("Ch{0}_min", tt.Ch)).FirstOrDefault());
                    }

                    if (a.Any(r => r.VariableName == string.Format("Ch{0}_max", tt.Ch) && r.ReadVal < tt.max))
                    {
                        lastres.Add(createlastreg(dateOrigin, tt.max, tt.R.Where(t => t.ReadVal == tt.max).FirstOrDefault().ReadingDate, reds.Key, string.Format("Ch{0}_max", tt.Ch)));
                    }
                    else
                    {
                        lastres.Add(a.Where(r => r.VariableName == string.Format("Ch{0}_max", tt.Ch)).FirstOrDefault());
                    }

                    lastres.Add(createlastreg(dateOrigin, tt.sum, dateOrigin, reds.Key, string.Format("Ch{0}_acum", tt.Ch)));
                }

                readingsDTOLastRG[reds.Key] = lastres;
            }

            GC.Collect();
        }

        private ReadingDTO createlastreg(DateTime originDate, double value, DateTime date, string deviceName, string variableName)
        {
            ReadingDTO reg = new ReadingDTO();
            reg.ReadingType = (int)PSVarType.Register;
            reg.ReadVal = value;
            reg.ReadingDate = date;
            reg.LogNumber = 1;
            reg.DeviceName = deviceName;
            reg.VariableName = variableName;
            reg.ReadingOrigin = new DataOriginDTO() { Date = originDate, Type = (int)PSDataOriginType.CallPrimeReadEnterprise, Workstation = "DESARROLLO-49" };
            return reg;
        }

        private bool EvAnalize(string type, string phase, List<ReadingDTO> readingg, string aaK, int aaV, int rand, int factdur = 1) 
        {
            bool result = false;

            rand = rand + new Random().Next(0, 2);

            if (aaK.Contains(string.Format("{0}_PH{1}", type, phase)) && aaV >= rand)
            {
                int rand3 = new Random().Next(0, 15);
                double rand4 = new Random().Next(30, 40) / 100;
                var reading = readingg.FirstOrDefault();
                double duration = new Random().NextDouble() * 240 * factdur;

                DateTime initDate = reading.ReadingDate.AddMinutes(rand3);
                DateTime endDate = initDate.AddSeconds(duration);
                AddEvent(dateOrigin, 1, initDate, endDate, duration, reading.DeviceName, string.Format("MTR_{0}_PH{1}", type, phase));
                var a = readingsDTOLastEV[reading.DeviceName];
                ReadingDTO eventmax = a == null ? null : a.Where(rdto => rdto.VariableName == string.Format("MTR_MAXIMUM_DURATION_{0}_PH{1}", type, phase)).FirstOrDefault();
                if (eventmax == null || duration > eventmax.ReadVal)
                {
                    AddEvent(dateOrigin, duration, dateOrigin, dateOrigin, 0, reading.DeviceName, string.Format("MTR_MAXIMUM_DURATION_{0}_PH{1}", type, phase));
                }
                else
                {
                    AddEvent(eventmax.DeviceName, eventmax);
                }

                ReadingDTO eventmin = readingsDTOLastEV[reading.DeviceName].Where(rdto => rdto.VariableName == string.Format("MTR_MINIMUM_DURATION_{0}_PH{1}", type, phase)).FirstOrDefault();
                if (eventmin == null || duration < eventmin.ReadVal)
                {
                    AddEvent(dateOrigin, duration, dateOrigin, dateOrigin, 0, reading.DeviceName, string.Format("MTR_MINIMUM_DURATION_{0}_PH{1}", type, phase));
                }
                else
                {
                    AddEvent(eventmin.DeviceName, eventmin);
                }

                ReadingDTO eventocurr = readingsDTOLastEV[reading.DeviceName].Where(rdto => rdto.VariableName == string.Format("MTR_OCURRENCES_{0}_PH{1}", type, phase)).FirstOrDefault();
                if (eventocurr == null)
                {
                    AddEvent(dateOrigin, 1, dateOrigin, dateOrigin, 0, reading.DeviceName, string.Format("MTR_OCURRENCES_{0}_PH{1}", type, phase));
                }
                else
                {
                    eventocurr.ReadVal++;
                    AddEvent(eventocurr.DeviceName, eventocurr);
                }

                ReadingDTO eventtot = readingsDTOLastEV[reading.DeviceName].Where(rdto => rdto.VariableName == string.Format("MTR_TOTAL_DURATION_{0}_PH{1}", type, phase)).FirstOrDefault();
                if (eventtot == null)
                {
                    AddEvent(dateOrigin, duration, dateOrigin, dateOrigin, 0, reading.DeviceName, string.Format("MTR_TOTAL_DURATION_{0}_PH{1}", type, phase));
                }
                else
                {
                    eventtot.ReadVal = eventtot.ReadVal + duration;
                    AddEvent(eventtot.DeviceName, eventtot);
                }

                phase = string.IsNullOrEmpty(phase) ? "A" : phase;
                string phase2 = phase == "B" ? "A" : "B";
                string phase3 = phase == "C" ? "A" : "C";
                int chV1 = phase == "A" ? 5 : phase == "B" ? 6 : 7;
                int chI1 = phase == "A" ? 8 : phase == "B" ? 9 : 10;
                int chV2 = phase == "B" ? 5 : 6;
                int chV3 = phase == "C" ? 5 : 7;
                int chI2 = phase == "B" ? 8 : 9;
                int chI3 = phase == "C" ? 8 : 10;

                if (type == "CUT")
                {
                    AddReg(dateOrigin, 0, initDate, reading.DeviceName, string.Format("V{0} Inst", phase));
                    AddReg(dateOrigin, 0, initDate, reading.DeviceName, string.Format("I{0} Inst", phase));
                    foreach (var tt in readingsDTOLP[reading.DeviceName].Where(r => (r.Channel == chV1 || r.Channel == chI1 || r.Channel == (chV1 + 6) || r.Channel == (chV1 + 9)) && r.ReadingDate > initDate && r.ReadingDate < endDate))
                    {
                        tt.ReadVal = 0;
                    }
                }
                else if (type == "SAG")
                {
                    double valV = readingg.Where(r => r.Channel == chV2).FirstOrDefault().ReadVal;
                    AddReg(dateOrigin, valV + (valV * rand4), initDate, reading.DeviceName, string.Format("V{0} Inst", phase));
                    AddReg(dateOrigin, readingg.Where(r => r.Channel == chV1).FirstOrDefault().ReadVal, initDate, reading.DeviceName, string.Format("I{0} Inst", phase));
                }
                else if (type == "SWELL")
                {
                    double valV = readingg.Where(r => r.Channel == chV2).FirstOrDefault().ReadVal;
                    AddReg(dateOrigin, valV - (valV * rand4), initDate, reading.DeviceName, string.Format("V{0} Inst", phase));
                    AddReg(dateOrigin, readingg.Where(r => r.Channel == chV1).FirstOrDefault().ReadVal, initDate, reading.DeviceName, string.Format("I{0} Inst", phase));
                }

                if (!type.Contains("OUTAGE"))
                {
                    AddReg(dateOrigin, readingg.Where(r => r.Channel == chV2).FirstOrDefault().ReadVal, initDate, reading.DeviceName, string.Format("V{0} Inst", phase2));
                    AddReg(dateOrigin, readingg.Where(r => r.Channel == chV3).FirstOrDefault().ReadVal, initDate, reading.DeviceName, string.Format("V{0} Inst", phase3));
                    AddReg(dateOrigin, readingg.Where(r => r.Channel == chI2).FirstOrDefault().ReadVal, initDate, reading.DeviceName, string.Format("I{0} Inst", phase2));
                    AddReg(dateOrigin, readingg.Where(r => r.Channel == chI3).FirstOrDefault().ReadVal, initDate, reading.DeviceName, string.Format("I{0} Inst", phase3));
                }
                else 
                {
                    AddReg(dateOrigin, 0, initDate, reading.DeviceName, string.Format("V{0} Inst", phase));
                    AddReg(dateOrigin, 0, initDate, reading.DeviceName, string.Format("V{0} Inst", phase2));
                    AddReg(dateOrigin, 0, initDate, reading.DeviceName, string.Format("V{0} Inst", phase3));
                    AddReg(dateOrigin, 0, initDate, reading.DeviceName, string.Format("I{0} Inst", phase));
                    AddReg(dateOrigin, 0, initDate, reading.DeviceName, string.Format("I{0} Inst", phase2));
                    AddReg(dateOrigin, 0, initDate, reading.DeviceName, string.Format("I{0} Inst", phase3));
                    foreach (var tt in readingsDTOLP[reading.DeviceName].Where(r => r.ReadingDate > initDate && r.ReadingDate < endDate))
                    {
                        tt.ReadVal = 0;
                        tt.Flags = "IZ";
                    }
                }

                result = true;
            }

            return result;
        }

        private void AddEvent(DateTime originDate, double value, DateTime initDate, DateTime EndDate, double duration, string deviceName, string variableName)
        {
            ReadingDTO ev = new ReadingDTO();
            ev.ReadingType = (int)PSVarType.Event;
            ev.ReadVal = value;
            ev.ReadingDate = initDate;
            ev.ReadingEndDate = EndDate;
            ev.Duration = duration;
            ev.LogNumber = 1;
            ev.DeviceName = deviceName;
            ev.VariableName = variableName;
            ev.ReadingOrigin = new DataOriginDTO(){Date = originDate, Type = (int)PSDataOriginType.CallPrimeReadEnterprise, Workstation = "DESARROLLO-49"};
            ev.Flags = string.Empty;
            ev.DSTApplied = false;
            if (!readingsDTOEV.ContainsKey(deviceName))
            {
                readingsDTOEV.Add(deviceName, new List<ReadingDTO>());
            }

            readingsDTOEV[deviceName].Add(ev);
        }

        private void AddEvent(string deviceName, ReadingDTO ev)
        {
            if (!readingsDTOEV.ContainsKey(deviceName))
            {
                readingsDTOEV.Add(deviceName, new List<ReadingDTO>());
            }

            readingsDTOEV[deviceName].Add(ev);
        }

        private void AddReg(DateTime originDate, double value, DateTime date, string deviceName, string variableName)
        {
            ReadingDTO reg = new ReadingDTO();
            reg.ReadingType = (int)PSVarType.Register;
            reg.ReadVal = value;
            reg.ReadingDate = date;
            reg.LogNumber = 1;
            reg.DeviceName = deviceName;
            reg.VariableName = variableName;
            reg.ReadingOrigin = new DataOriginDTO(){Date = originDate, Type = (int)PSDataOriginType.CallPrimeReadEnterprise, Workstation = "DESARROLLO-49"};
            if (!readingsDTORG.ContainsKey(deviceName))
            {
                readingsDTORG.Add(deviceName, new List<ReadingDTO>());
            }

            readingsDTORG[deviceName].Add(reg);
        }

        private void AddReg(ReadingDTO reg, string deviceName)
        {
            if (!readingsDTORG.ContainsKey(deviceName))
            {
                readingsDTORG.Add(deviceName, new List<ReadingDTO>());
            }
            
            readingsDTORG[deviceName].Add(reg);
        }
    }
}

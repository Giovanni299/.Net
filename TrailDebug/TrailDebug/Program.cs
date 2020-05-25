using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TrailDebug
{
    class Program
    {
        /// <summary>
        /// Puerto de escucha.
        /// </summary>
        static int port;

        /// <summary>
        /// The data list
        /// </summary>
        static List<String> dataList;

        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            port = 6543;
            if (LoadDocument())
            {
                Listen();
            }
        }

        /// <summary>
        /// Loads the document.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private static bool LoadDocument()
        {
            string trailFile = @"C:\Nueva carpeta\text.txt";
            string line = string.Empty;
            string newLine = string.Empty;
            int row = 3;
            dataList = new List<string>();

            if (File.Exists(trailFile))
            {
                // Read the file and display it line by line.  
                System.IO.StreamReader file = new System.IO.StreamReader(trailFile);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("<--"))
                    {
                        if (line.Contains('['))
                        {
                            int start = line.IndexOf('[') + 1;
                            int end = line.IndexOf(']');
                            newLine = line.Substring(start, end - start);
                        }
                        else
                        {
                            if (line.Contains("<---"))
                            {
                                row = 4;                                
                            }

                            newLine = line.Substring(line.IndexOf("<--") + row).Trim();
                        }
                        
                        if (newLine.Split(' ').Any())
                        {
                            newLine = newLine.Split(' ')[0];
                        }

                        dataList.Add(newLine);
                        //Console.WriteLine(newLine);
                    }
                }

                file.Close();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Escucha lo que esta llegando por el puerto.
        /// </summary>
        static void Listen()
        {
            TcpListener server = null;
            try
            {
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                foreach(var local in localIPs)
                {
                    Console.WriteLine("Local IP " + local);
                }


                IPAddress localAddr = localIPs[1];
                server = new TcpListener(localAddr, port);
                server.Start();
                
                Console.WriteLine(" SERVICE IS STARTED ");
                Console.WriteLine(string.Format("LocalEndpoint Connected: {0}", server.LocalEndpoint));
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Socket socketTCP = client.Client;
                    Console.WriteLine("Connection accepted from " + socketTCP.RemoteEndPoint);

                    for (int frame = 0; frame < dataList.Count; frame++)
                    {
                        string receivedText = string.Empty;
                        byte[] b = new byte[100];
                        int k = socketTCP.Receive(b);
                        for (int i = 0; i < k; i++)
                            receivedText += b[i].ToString("x2");
                        
                        if (receivedText == "0206001400")
                        {
                            Console.Write($"\n <-- {receivedText}  - String refuse.");
                            frame--;
                            continue;
                        }

                        //if (frame == 4)
                        //    continue;

                        Console.Write($"\r\n <-- {receivedText}");
                        Send(dataList[frame], socketTCP);
                    }

                    socketTCP.Close();
                    break;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                server.Stop();
            }
        }
        
        /// <summary>
        /// Envio de la cadena de informacion.
        /// </summary>
        /// <param name="data">Cadena de información.</param>
        /// <param name="socketTCP">Socket.</param>
        static void Send(string data1, Socket socketTCP)
        {
            string data = Hex2Ascii(data1);

            byte[] byteData = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                byteData[i] = Convert.ToByte(data[i]);
            }
            
            socketTCP.Send(byteData);
            Console.Write($"\n --> {data1}");
        }

        /// <summary>
        /// Convierte de Hexastring a Asciistring.
        /// </summary>
        /// <param name="cadena">Cadena de entrada.</param>
        /// <returns>String en Hexa.</returns>
        static string Hex2Ascii(string cadena)
        {
            string result = string.Empty, oneByte = string.Empty;
            short i = 0;
            if (cadena.Length == 1)
            {
                cadena = "0" + cadena;
            }

            for (i = 0; ; i++)
            {
                if (cadena.Length < 2)
                {
                    break;
                }

                oneByte = cadena.Substring(0, 2);
                cadena = cadena.Remove(0, 2);
                result += Convert.ToChar(Convert.ToByte(oneByte, 16));
                if (cadena.Length == 0)
                {
                    break;
                }
            }

            return result;
        }
    }
}

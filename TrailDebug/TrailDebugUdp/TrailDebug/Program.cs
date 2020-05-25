using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        /// Variable que indica si el servicio esta en ejecución.
        /// </summary>
        static bool Running;

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
            Running = true;
            port = 6543;
            if (LoadDocument())
            {
                if (args.Length > 0 && args[0] == "udp")
                {
                    ListenUdp(); 
                }
                else
                {
                    ListenTcp();
                }
            }
        }

        /// <summary>
        /// Loads the document.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private static bool LoadDocument()
        {
            string trailFile = @"D:\Atención de Casos\Protocolos\2018\Edmi\28052 - Acción de relé falla EDMIMK10D\Resp - Falta Info\IMP#28052_20181203\PR\Abrir Rele\CALL_RealTrail.txt";
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
                        Console.WriteLine(newLine);
                    }
                }

                file.Close();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Escucha lo que está llegando por el puerto en UDP.
        /// </summary>
        static void ListenUdp()
        {
            UdpClient listener = new UdpClient(port);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine(" SERVICE IS STARTED IN UDP MODE ");
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast....");
                    for (int frame = 0; frame < dataList.Count; frame++)
                    {
                        byte[] b = listener.Receive(ref groupEP);
                        Console.WriteLine($"Recieved UDP broadcast from {groupEP}...");
                        for (int i = 0; i < b.Length; i++)
                        {
                            Console.Write(Convert.ToChar(b[i]));
                        }
                        Console.WriteLine(); 
                        Console.WriteLine($"Sending UDP response...");
                        SendUdp(dataList[frame], ref listener, ref groupEP);
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"UDP Exception: {ex.Message}");
            }
            finally
            {
                listener.Close(); 
            }
        }

        /// <summary>
        /// Escucha lo que esta llegando por el puerto en TCP.
        /// </summary>
        static void ListenTcp()
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
                
                Console.WriteLine(" SERVICE IS STARTED IN TCP MODE ");
                Console.WriteLine(string.Format("LocalEndpoint Connected: {0}", server.LocalEndpoint));
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Socket socketTCP = client.Client;
                    Console.WriteLine("Connection accepted from " + socketTCP.RemoteEndPoint);

                    for (int frame = 0; frame < dataList.Count; frame++)
                    {
                        byte[] b = new byte[100];
                        int k = socketTCP.Receive(b);
                        Console.WriteLine("Recieved...");
                        for (int i = 0; i < k; i++)
                            Console.Write(Convert.ToChar(b[i]));
                        
                        Send(dataList[frame], socketTCP);
                    }

                    socketTCP.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("TCP Socket Exception: {0}", e);
            }
            finally
            {
                server.Stop();
                dataList.Clear();
            }
        }

        /// <summary>
        /// Envio de la cadena de informacion por medio de Udp.
        /// </summary>
        /// <param name="data">Cadena de información.</param>
        /// <param name="client">Servidor de Udp.</param>
        /// <param name="groupEP">Ip endpoint para enviar información.</param>
        static void SendUdp(string data, ref UdpClient client, ref IPEndPoint groupEP)
        {
            data = Hex2Ascii(data);

            byte[] byteData = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                byteData[i] = Convert.ToByte(data[i]);
            }
            client.Send(byteData, byteData.Length, groupEP);
        }

        /// <summary>
        /// Envio de la cadena de informacion.
        /// </summary>
        /// <param name="data">Cadena de información.</param>
        /// <param name="socketTCP">Socket.</param>
        static void Send(string data, Socket socketTCP)
        {
            data = Hex2Ascii(data);

            byte[] byteData = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                byteData[i] = Convert.ToByte(data[i]);
            }
            
            socketTCP.Send(byteData);
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

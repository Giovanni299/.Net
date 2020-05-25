using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using Newtonsoft.Json;
using Primestone.PrimeBus.Contracts;
using Primestone.PrimeBus.Contracts.Exceptions;
using Primestone.PrimeBus.InfrastructureService;
using Primestone.PrimeRead.Infrastructure.CrossCutting.Core.IoC;
using Primestone.PrimeRead.Infrastructure.Serialization.Contracts;

namespace UnpackerServicesTester
{
    public partial class Form1 : Form
    {
        List<ReadingDTO> readings = new List<ReadingDTO>();
        List<ChainLink> chainList = new List<ChainLink>();
        ISerializer serializer = IoCFactory.Resolve<ISerializer>();
        const string EXECUTION_MESSAGE = "Total Time on Execution : {0} Seconds";
        const string TCP_CONFIG = "NetTcpBinding";
        const string HTTP_CONFIG = "BasicHttpBinding";

        // SUBSCRIBER
        const string TCP_SUBSCRIBER_CONFIG = "NetTcpBindingConfiguration";
        const string HTTP_SUBSCRIBER_CONFIG = "httpBindingConfiguration";
             
        // CHAIN LINK
        const string HTTP_CHAINLINK_CONFIG = "BasicHttpBinding_IServiceLink";
        const string TCP_CHAINLINK_CONFIG = "NetTcpBinding_IServiceLink";
        
        // PUBLISHER
        const string HTTP_PUBLISHER_CONFIG = "BasicHttpBinding_IPublisherService";
        const string TCP_PUBLISHER_CONFIG = "NetTcpBinding_IPublisherService";

        int readingType;

        // TIME MEASURE
        Stopwatch stopWatch = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
            cmbBindings.SelectedIndex = 0;
            cmbType.SelectedIndex = 0;
        }
        
        private void showLoadingReadingsMessage(string msg)
        {
            loadResult.AppendText(string.Format("\r\n {0} : {1}", DateTime.Now, msg));
            loadResult.ScrollToCaret();
        }
        private void showEndpointMEssage(string msg)
        {
            txtResultEndpkint.AppendText(string.Format("\r\n {0} : {1}",DateTime.Now, msg));
            txtResultEndpkint.ScrollToCaret();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if(txtPath.Text == string.Empty) return;
                string path = txtPath.Text; 
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    readings = JsonConvert.DeserializeObject<List<ReadingDTO>>(json);
                    btnSend.Enabled = readings.Any();
                    showLoadingReadingsMessage("Loading Completed");
                    showLoadingReadingsMessage(string.Format("Loaded Readings: {0}", readings.Count));
                    if (readings.Any())
                    {
                        readingType = readings.FirstOrDefault().ReadingType;
                        showLoadingReadingsMessage(string.Format("Reading Type: {0}", readingType));
                    }
                }
            }
            catch (Exception ex)
            {
                showLoadingReadingsMessage(ex.Message);
            }
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private SupportedBindings GetSelectedBinding()
        {
            return cmbBindings.Text == "NetTcpBinding" ? SupportedBindings.NetTcpBinding : SupportedBindings.BasicHttpBinding;
        }
       
        private void Consume()
        {
            string config = string.Empty;
            SupportedBindings binding = GetSelectedBinding();

            try
            {
                byte[] result = serializer.Serialize<ReadingDTO[]>(readings.ToArray());
                switch (cmbType.Text)
                {
                    case "Publisher":
                        config = HTTP_PUBLISHER_CONFIG;
                        if (cmbBindings.Text == TCP_CONFIG)
                        {
                            config = TCP_PUBLISHER_CONFIG;
                        }
                        stopWatch.Start();
                        var publisher = ChainServiceHelper.CreateServiceClient<IPublisherService>(binding, config, txtEndpoint.Text);
                        publisher.OnNewReadingsSerialized(result, readingType);
                        stopWatch.Stop();
                        break;
                    case "Subscriber":
                        config = HTTP_SUBSCRIBER_CONFIG;
                        if (cmbBindings.Text == TCP_CONFIG)
                        {
                            config = TCP_SUBSCRIBER_CONFIG;
                        }
                        stopWatch.Start();
                        var subscriber = ChainServiceHelper.CreateServiceClient<IBusinessSubsProcess>(binding, config, txtEndpoint.Text);
                        subscriber.ProcessReadings(result);
                        stopWatch.Stop();
                        break;
                    case "ChainLink":
                        config = HTTP_CHAINLINK_CONFIG;
                        if (cmbBindings.Text == TCP_CONFIG)
                        {
                            config = TCP_CHAINLINK_CONFIG;
                        }
                        
                        if (txtNextLink.Text != string.Empty)
                        {
                            chainList = new List<ChainLink>();
                            chainList.Add(new ChainLink(binding, txtNextLink.Text, config, 1));
                            chainList.Add(new ChainLink(binding, txtNextLink.Text, config, 2));
                        }
                        stopWatch.Start();
                        var chainLink = ChainServiceHelper.CreateServiceClient<IServiceLink>(binding, config, txtEndpoint.Text);
                        chainLink.HandleRequestSerialized(chainList, result);
                        stopWatch.Stop();
                        break;
                }
                
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            stopWatch.Reset();
            try
            {
                
                if (txtEndpoint.Text == string.Empty) return;
               

                showEndpointMEssage("Starting...");
                Consume();
                showEndpointMEssage(string.Format(EXECUTION_MESSAGE, stopWatch.Elapsed.Seconds));
                showEndpointMEssage("Service Launched Successfully.");
            }
            catch (FaultException<BusinessException> bx)
            {
                stopWatch.Stop();
                showEndpointMEssage(string.Format(EXECUTION_MESSAGE, stopWatch.Elapsed.Seconds));
                showEndpointMEssage(string.Format("Error connecting to service : {0}", bx.Message));
                
            }
            catch (Exception ex)
            {
                
                stopWatch.Stop();
                showEndpointMEssage(string.Format(EXECUTION_MESSAGE, stopWatch.Elapsed.Seconds));
                showEndpointMEssage(string.Format("Error connecting to service : {0}", ex.Message));
            }
            finally
            {
                showEndpointMEssage("Service Consuming Ended.");
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            loadResult.Text = "";
            txtResultEndpkint.Text = "";
        }
    }
}

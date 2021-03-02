using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CommStatus
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        const int maxPingPerIP = 5;
        public event PropertyChangedEventHandler PropertyChanged;
        ObservableCollection<IPAddress> _iPAddresses;
        ObservableCollection<IPAddress> IPAddresses
        {
            get => _iPAddresses;
            set
            {
                if (_iPAddresses != value)
                    _iPAddresses = value;
                OnPropertyChanged(nameof(IPAddresses));
            }
        }

        public async Task PingMachineAsync()
        {
            //instance of ping
            Ping pingSender = new Ping();
            //instance of reply
            PingReply reply = null;
            //read json file
            string json = File.ReadAllText("myips.json", Encoding.UTF8);
            //deserialization of json elements
            var addressesCollection = JsonConvert.DeserializeObject<IEnumerable<IPObject>>(json).ToList(); 

            foreach (var address in addressesCollection)
            {
                for (int ping = 0; ping < maxPingPerIP; ping++)
                {
                    try
                    {
                        reply = await pingSender.SendPingAsync(address.IPAdress);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    progressbarAddresses.Value += progressbarAddresses.Maximum / addressesCollection.Count / maxPingPerIP;
                    progressbarAddressesPlaceholder.Text = $"{progressbarAddresses.Value}% (pinging {address.IPAdress} [{ping}])";
                }

                TextBlock statusTextBlock = new TextBlock()
                {
                    Text = $"{address.IPAdress} {reply.Status} ({reply.Address})",
                };

                if (reply.Status != IPStatus.Success)
                {
                    statusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    statusTextBlock.Text += $" - {address.ErrorDescription}";
                }
                else
                {
                    statusTextBlock.Foreground = new SolidColorBrush(Colors.LimeGreen);
                }
                replyHolder.Children.Add(statusTextBlock);
            }
            progressbarAddressesPlaceholder.Text = "Completed";
        }

        private async void CheckNetworkStatus(object sender, RoutedEventArgs e)
        {
            //clear values of elements
            progressbarAddresses.Value = 0;
            progressbarAddressesPlaceholder.Text = "";
            replyHolder.Children.Clear();
            (sender as Button).IsEnabled = false;
            //start pinging
            await PingMachineAsync();
            (sender as Button).IsEnabled = true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SolidLayer_Architecture.Pages
{
    public class MobileAccessModel : PageModel
    {
        public string ServerIP { get; private set; } = string.Empty;
        public int ServerPort { get; private set; } = 5235;
        public string QRCodeUrl { get; private set; } = string.Empty;
        
        public void OnGet()
        {
            // Get the server's IP address
            ServerIP = GetLocalIPAddress();
            
            // Generate a URL for the server
            string serverUrl = $"http://{ServerIP}:{ServerPort}";
            
            // Create a QR code URL using the Google Charts API
            QRCodeUrl = $"https://chart.googleapis.com/chart?cht=qr&chl={Uri.EscapeDataString(serverUrl)}&chs=300x300";
        }
        
        private string GetLocalIPAddress()
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var address in hostEntry.AddressList)
            {
                // Only consider IPv4 addresses on the local network (not localhost or virtual interfaces)
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    string ip = address.ToString();
                    if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || 
                        (ip.StartsWith("172.") && ip.Split('.').Length > 1 && 
                         int.TryParse(ip.Split('.')[1], out int second) && 
                         second >= 16 && second <= 31))
                    {
                        return ip;
                    }
                }
            }
            
            return "127.0.0.1"; // Fallback to localhost if no suitable IP is found
        }
    }
}

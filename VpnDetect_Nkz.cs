using BestHTTP;
using Life;
using Life.DB;
using Life.Network;
using Mirror;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace VpnDetect_Nkz
{
    public class VpnDetect_Nkz : Plugin
    {
        private string _webhookUrl;
        private bool _isBanFunctionEnabled;
        private List<string> _excludedCountryCodes;
        private MainConfig _mainConfig;

        private static readonly List<string> ExcludedISPKeywords = new List<string>
        {
            "NVIDIA",
            "Geforce",
            "GeForce Now",
            "NVIDIA Corporation",
            "NVIDIA Ltd",
            "NVIDIA Online",
            "GeForce Experience",
            "GFN",
            "Geforce Now",
            "G-Force Now",
            "NVIDIA Services",
            "NVIDIA Cloud Gaming",
            "GeForce Cloud",
            "GFNow",
            "Nvidia Gaming",
            "Geforce Now Cloud",
            "NVIDIA Geforce Experience",
            "Geforce Cloud",
            "NVIDIA Streaming",
            "Geforce Streaming",
            "NVIDIA Shield",
            "NVIDIA Shield TV",
            "GeForce Shield",
            "NVIDIA GeForce",
            "GeForce NVIDIA",
            "GFN NVIDIA"
        };
        public VpnDetect_Nkz(IGameAPI api) : base(api) { }

        public void Log(string message) => Console.WriteLine("[VpnDetect_Nkz] " + message);

        public override void OnPluginInit()
        {
            base.OnPluginInit();

            _mainConfig = LoadConfiguration(Path.Combine(pluginsPath, "VpnDetect_Nkz/config.json"));

            _webhookUrl = _mainConfig.WebhookUrl;
            _isBanFunctionEnabled = _mainConfig.IsBanFunctionEnabled;
            _excludedCountryCodes = _mainConfig.ExcludedCountryCodes;

            Log($"VpnDetect_Nkz est initialisé avec succès !");
            Log($"Discord : emile.cvl | Github : https://github.com/emilecvl/");
        }

        private static MainConfig LoadConfiguration(string configFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(configFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
            }
            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new MainConfig
                {
                    WebhookUrl = "",
                    IsBanFunctionEnabled = true,
                    ExcludedCountryCodes = new List<string> { "FR", "BE", "CA", "CH" },
                };
                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.None));
            }
            return JsonConvert.DeserializeObject<MainConfig>(System.IO.File.ReadAllText(configFilePath));
        }

        public override async void OnPlayerSpawnCharacter(Player player, NetworkConnection conn, Characters character)
        {
            base.OnPlayerSpawnCharacter(player, conn, character);

            await CheckPlayerVpnStatus(player);
        }

        public async Task CheckPlayerVpnStatus(Player player)
        {
            var ipAddress = player.conn.identity.connectionToClient.address;
            var ipInfo = await IpApiResponse.GetIpGeolocationAsync(ipAddress);

            if (string.IsNullOrEmpty(ipInfo.Isp))
            {
                Log($"Impossible de récupérer l'ISP pour {player.FullName} ({ipAddress}).");
                return;
            }

            bool isGeforceNow = ExcludedISPKeywords.Any(keyword => ipInfo.Isp.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            if (_excludedCountryCodes.Contains(ipInfo.CountryCode) || isGeforceNow)
            {
                Log(isGeforceNow
                    ? $"L'adresse IP {ipAddress} du joueur {player.FullName} provient de {ipInfo.Country} mais s'est connectée via GeForce Now."
                    : $"L'adresse IP {ipAddress} du joueur {player.FullName} provient de {ipInfo.Country}.");
                return;
            }

            if (_isBanFunctionEnabled) BanPlayer(player);

            Nova.server.SendMessageToAdmins($"<color=red>[VpnDetect_Nkz] Le joueur {player.FullName} a rejoint le serveur avec une IP provenant d'un pays non autorisé.\n[VpnDetect_Nkz] Pays : {ipInfo.Country}");
            PlaySoundForAdmins();
            SendDiscordWebhook(_webhookUrl, player.FullName, ipAddress, ipInfo.Country);
            Log($"L'adresse IP {ipAddress} du joueur {player.FullName} ne provient pas d'un pays autorisé sur le serveur. Pays : {ipInfo.Country}");
            Log("Pour toute modification, veuillez éditer le fichier de configuration et redémarrer le serveur.");
        }

        public static void PlaySoundForAdmins()
        {
            foreach (var player in Nova.server.Players)
            {
                if (player.IsAdmin) player.setup.TargetPlayClaironById(0.2f, Nova.server.config.roleplayConfig.ticketAlertSound);
            }
        }

        private static void BanPlayer(Player player)
        {
            player.account.banReason = "Connexion avec un VPN";
            player.account.bans++;
            player.account.banTimestamp = -1;
            player.Save();
            player.Disconnect();
        }

        public void SendDiscordWebhook(string webhookUrl, string playerName, string ipAdress, string Country)
        {
            var embed = new
            {
                title = "VpnDetect_Nkz : Info",
                color = 0x1d2d3c,
                fields = new[]
                {

                    new { name = "👤 Joueur :", value = playerName, inline = false },
                    new { name = "🌐 Adresse IP :", value = ipAdress, inline = true },
                    new { name = "🌍 Pays :", value = Country, inline = false }
                },
                footer = new { text = "VpnDetect_Nkz - emile.cvl" }
            };

            var payload = new { embeds = new[] { embed } };

            string jsonMessage = JsonConvert.SerializeObject(payload);
            HTTPRequest request = new HTTPRequest(new Uri(webhookUrl), HTTPMethods.Post, (req, res) => { });
            request.AddHeader("Content-Type", "application/json");
            request.RawData = System.Text.Encoding.UTF8.GetBytes(jsonMessage);
            if (!string.IsNullOrEmpty(webhookUrl)) request.Send();
            else
            {
                Log("Veuillez remplir le webhook dans la configuration.");
                Log("Pour toute modification, veuillez éditer le fichier de configuration et redémarrer le serveur.");
            }
        }
        public class IpApiResponse
        {
            public string Country { get; set; }
            public string CountryCode { get; set; }
            public string Isp { get; set; }

            public static async Task<IpApiResponse> GetIpGeolocationAsync(string ipAddress)
            {
                var url = $"http://ip-api.com/json/{ipAddress}";
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    return JsonConvert.DeserializeObject<IpApiResponse>(response);
                }
            }
        }
    }
}

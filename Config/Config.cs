using System.Collections.Generic;

public class VpnDetect.MainConfig
{
    public MainConfig()
    {
        ExcludedCountryCodes = new List<string>();
    }

    public string WebhookUrl { get; set; }
    public bool IsBanFunctionEnabled { get; set; }
    public List<string> ExcludedCountryCodes { get; set; }
}

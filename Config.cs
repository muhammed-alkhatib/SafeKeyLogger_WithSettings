// Config.cs - simple key=value text config loader/saver
using System;
using System.IO;
using System.Collections.Generic;

namespace SafeKeyLogger
{
    public class Config
    {
        public string FromEmail { get; set; } = "";
        public string FromPassword { get; set; } = "";
        public string SmtpHost { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string ToEmail { get; set; } = "";
        public bool EnableSsl { get; set; } = true;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(FromEmail) && !string.IsNullOrWhiteSpace(FromPassword) && !string.IsNullOrWhiteSpace(SmtpHost) && SmtpPort > 0 && !string.IsNullOrWhiteSpace(ToEmail);
        }

        public void Save(string path)
        {
            var lines = new List<string> {
                "FromEmail=" + FromEmail,
                "FromPassword=" + FromPassword,
                "SmtpHost=" + SmtpHost,
                "SmtpPort=" + SmtpPort,
                "ToEmail=" + ToEmail,
                "EnableSsl=" + EnableSsl
            };
            File.WriteAllLines(path, lines);
        }

        public static Config? Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var idx = line.IndexOf('=');
                    if (idx <= 0) continue;
                    var k = line.Substring(0, idx).Trim();
                    var v = line.Substring(idx + 1).Trim();
                    dict[k] = v;
                }
                var cfg = new Config();
                if (dict.TryGetValue("FromEmail", out var fe)) cfg.FromEmail = fe;
                if (dict.TryGetValue("FromPassword", out var fp)) cfg.FromPassword = fp;
                if (dict.TryGetValue("SmtpHost", out var sh)) cfg.SmtpHost = sh;
                if (dict.TryGetValue("SmtpPort", out var sp) && int.TryParse(sp, out var port)) cfg.SmtpPort = port;
                if (dict.TryGetValue("ToEmail", out var te)) cfg.ToEmail = te;
                if (dict.TryGetValue("EnableSsl", out var es) && bool.TryParse(es, out var b)) cfg.EnableSsl = b;
                return cfg;
            }
            catch { return null; }
        }
    }
}

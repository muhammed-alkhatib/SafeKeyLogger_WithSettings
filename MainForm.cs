// MainForm.cs - builds UI and opens SettingsForm
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace SafeKeyLogger
{
    public partial class MainForm : Form
    {
        private bool isRecording = false;
        private string logFolder;
        private string logFilePath;
        private Config config;

        private Button btnStartStop;
        private Button btnCapture;
        private Button btnSend;
        private Button btnSettings;
        private TextBox txtLog;
        private Label lblStatus;

        public MainForm()
        {
            // If running on older .NET remove SetHighDpiMode (handled in Program.cs target net6.0-windows)
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            SetupUI();

            logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SafeLogs");
            Directory.CreateDirectory(logFolder);

            // load config (if exists)
            config = Config.Load(Path.Combine(logFolder, "config.txt"));
        }

        private void SetupUI()
        {
            this.Text = "SafeKeyLogger - Educational";
            this.ClientSize = new Size(760, 500);

            btnStartStop = new Button() { Text = "Start", Location = new Point(20, 20), Size = new Size(100, 30) };
            btnCapture = new Button() { Text = "Capture Screen", Location = new Point(140, 20), Size = new Size(120, 30) };
            btnSend = new Button() { Text = "Send Logs", Location = new Point(280, 20), Size = new Size(100, 30) };
            btnSettings = new Button() { Text = "Settings", Location = new Point(400, 20), Size = new Size(100, 30) };
            lblStatus = new Label() { Text = "Status: Stopped", Location = new Point(520, 25), AutoSize = true };
            txtLog = new TextBox() { Multiline = true, Location = new Point(20, 70), Size = new Size(710, 400), ScrollBars = ScrollBars.Vertical };

            this.Controls.Add(btnStartStop);
            this.Controls.Add(btnCapture);
            this.Controls.Add(btnSend);
            this.Controls.Add(btnSettings);
            this.Controls.Add(lblStatus);
            this.Controls.Add(txtLog);

            btnStartStop.Click += BtnStartStop_Click;
            btnCapture.Click += BtnCapture_Click;
            btnSend.Click += BtnSend_Click;
            btnSettings.Click += BtnSettings_Click;
        }

        private void BtnStartStop_Click(object sender, EventArgs e)
        {
            isRecording = !isRecording;
            if (isRecording)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                logFilePath = Path.Combine(logFolder, $"keylog_{timestamp}.txt");
                txtLog.AppendText($"--- Recording started: {DateTime.Now} ---" + Environment.NewLine);
                btnStartStop.Text = "Stop";
                lblStatus.Text = "Status: Recording (app focused only)";
            }
            else
            {
                txtLog.AppendText($"--- Recording stopped: {DateTime.Now} ---" + Environment.NewLine);
                if (!string.IsNullOrEmpty(logFilePath))
                    File.AppendAllText(logFilePath, txtLog.Text);
                btnStartStop.Text = "Start";
                lblStatus.Text = "Status: Stopped";
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isRecording) return;
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{e.KeyCode}";
            txtLog.AppendText(line + Environment.NewLine);
            try
            {
                if (!string.IsNullOrEmpty(logFilePath))
                    File.AppendAllText(logFilePath, line + Environment.NewLine);
            }
            catch { }
        }

        private void BtnCapture_Click(object sender, EventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                string file = Path.Combine(logFolder, $"screenshot_{now:yyyyMMdd_HHmmss}.png");
                Rectangle bounds = Screen.GetBounds(Point.Empty);
                using (Bitmap bmp = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }
                    bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                }
                txtLog.AppendText($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tScreenshot saved: {file}" + Environment.NewLine);
                MessageBox.Show("Screenshot saved:\n" + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            // reload config before sending
            config = Config.Load(Path.Combine(logFolder, "config.txt"));

            if (string.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
            {
                MessageBox.Show("No log file to send.");
                return;
            }

            if (config == null || !config.IsValid())
            {
                MessageBox.Show("SMTP settings are missing or invalid. Open Settings and configure them first.");
                return;
            }

            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = new System.Net.Mail.MailAddress(config.FromEmail);
                mail.To.Add(config.ToEmail);
                mail.Subject = "Test Logs (Educational)";
                mail.Body = "Attached: app-focused key log and screenshots (with consent).";

                mail.Attachments.Add(new System.Net.Mail.Attachment(logFilePath));
                foreach (var f in Directory.GetFiles(logFolder, "screenshot_*.png"))
                    mail.Attachments.Add(new System.Net.Mail.Attachment(f));

                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(config.SmtpHost, config.SmtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(config.FromEmail, config.FromPassword),
                    EnableSsl = config.EnableSsl
                };
                smtp.Send(mail);
                MessageBox.Show("Email sent.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Email send error: " + ex.Message);
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            string cfgPath = Path.Combine(logFolder, "config.txt");
            using (var form = new SettingsForm(cfgPath))
            {
                var res = form.ShowDialog();
                if (res == DialogResult.OK)
                {
                    txtLog.AppendText($"Settings saved: {DateTime.Now}" + Environment.NewLine);
                }
            }
        }
    }
}

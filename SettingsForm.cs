// SettingsForm.cs - simple settings UI to edit SMTP config saved in config.txt
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace SafeKeyLogger
{
    public class SettingsForm : Form
    {
        private TextBox txtFromEmail, txtFromPass, txtSmtpHost, txtSmtpPort, txtToEmail;
        private CheckBox chkSsl;
        private Button btnSave, btnCancel;
        private string configPath;

        public SettingsForm(string configPath)
        {
            this.configPath = configPath;
            SetupUI();
            LoadConfig();
        }

        private void SetupUI()
        {
            this.Text = "Settings";
            this.ClientSize = new Size(420, 260);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var lbl1 = new Label() { Text = "From Email:", Location = new Point(10, 15), AutoSize = true };
            txtFromEmail = new TextBox() { Location = new Point(110, 12), Width = 290 };

            var lbl2 = new Label() { Text = "Password:", Location = new Point(10, 50), AutoSize = true };
            txtFromPass = new TextBox() { Location = new Point(110, 47), Width = 290, UseSystemPasswordChar = true };

            var lbl3 = new Label() { Text = "SMTP Host:", Location = new Point(10, 85), AutoSize = true };
            txtSmtpHost = new TextBox() { Location = new Point(110, 82), Width = 290 };

            var lbl4 = new Label() { Text = "SMTP Port:", Location = new Point(10, 120), AutoSize = true };
            txtSmtpPort = new TextBox() { Location = new Point(110, 117), Width = 100 };

            var lbl5 = new Label() { Text = "To Email:", Location = new Point(10, 155), AutoSize = true };
            txtToEmail = new TextBox() { Location = new Point(110, 152), Width = 290 };

            chkSsl = new CheckBox() { Text = "Enable SSL", Location = new Point(110, 185), AutoSize = true };

            btnSave = new Button() { Text = "Save", Location = new Point(230, 210), Size = new Size(80, 30) };
            btnCancel = new Button() { Text = "Cancel", Location = new Point(320, 210), Size = new Size(80, 30) };

            this.Controls.AddRange(new Control[] { lbl1, txtFromEmail, lbl2, txtFromPass, lbl3, txtSmtpHost, lbl4, txtSmtpPort, lbl5, txtToEmail, chkSsl, btnSave, btnCancel });

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        }

        private void LoadConfig()
        {
            var cfg = Config.Load(configPath);
            if (cfg == null) return;
            txtFromEmail.Text = cfg.FromEmail;
            txtFromPass.Text = cfg.FromPassword;
            txtSmtpHost.Text = cfg.SmtpHost;
            txtSmtpPort.Text = cfg.SmtpPort.ToString();
            txtToEmail.Text = cfg.ToEmail;
            chkSsl.Checked = cfg.EnableSsl;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFromEmail.Text) || string.IsNullOrWhiteSpace(txtFromPass.Text))
            {
                MessageBox.Show("From Email and Password are required.");
                return;
            }
            int port = 587;
            int.TryParse(txtSmtpPort.Text, out port);
            var cfg = new Config
            {
                FromEmail = txtFromEmail.Text.Trim(),
                FromPassword = txtFromPass.Text,
                SmtpHost = txtSmtpHost.Text.Trim(),
                SmtpPort = port,
                ToEmail = txtToEmail.Text.Trim(),
                EnableSsl = chkSsl.Checked
            };
            cfg.Save(configPath);
            this.DialogResult = DialogResult.OK;
        }
    }
}

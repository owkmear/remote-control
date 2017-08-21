using System;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace TcpClient
{
    public partial class MyAccount : Form
    {
        private string domain;
        public MyAccount()
        {
            InitializeComponent();
            button1.Enabled = false;
            Timer.Enabled = true;
            button1.DialogResult = DialogResult.OK;
            button2.DialogResult = DialogResult.Cancel;

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal principial = (WindowsPrincipal)Thread.CurrentPrincipal;
            WindowsIdentity identity = (WindowsIdentity)principial.Identity;

            domain = identity.Name.Remove(identity.Name.IndexOf("\\")).Trim();
            textBox2.Text = identity.Name.Substring(identity.Name.IndexOf("\\") + 1).Trim();
        }

        public string getDomain()
        {
            return domain;
        }
        public string getLogin()
        {
            return this.textBox2.Text;
        }
        public string getPassword()
        {
            return this.textBox3.Text;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (new System.Text.RegularExpressions
                .Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$")
                .IsMatch(TextIP.Text))
            {
                TextIP.ForeColor = System.Drawing.Color.Green;
                button1.Enabled = true;
            }
            else
            {
                TextIP.ForeColor = System.Drawing.Color.Red;
                button1.Enabled = false;
            }
        }
        public string getTextIP()
        {
            return this.TextIP.Text;
        }
        public void setTimer(bool _)
        {
            Timer.Enabled = _;
        }
        public int getPort()
        {
            return (int) Port.Value;
        }
    }
}

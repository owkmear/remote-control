using System;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace TcpClient
{
    public partial class Сonnect : Form
    {
        public Сonnect()
        {
            InitializeComponent();

            button1.DialogResult = DialogResult.OK;
            button2.DialogResult = DialogResult.Cancel;

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal principial = (WindowsPrincipal)Thread.CurrentPrincipal;
            WindowsIdentity identity = (WindowsIdentity)principial.Identity;

            textBox1.Text = identity.Name.Remove(identity.Name.IndexOf("\\")).Trim();
            textBox2.Text = identity.Name.Substring(identity.Name.IndexOf("\\") + 1).Trim();
        }

        public string getDomain()
        {
            return this.textBox1.Text;
        }
        public string getLogin()
        {
            return this.textBox2.Text;
        }
        public string getPassword()
        {
            return this.textBox3.Text;
        }
    }
}

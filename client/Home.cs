using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Security.Principal;
using System.Net.Security;
using System.Security.Authentication;


namespace TcpClient
{
    public partial class Home : Form
    {
        public List<string> MyList { get; set; }

        private static System.Net.Sockets.TcpClient client;
        private static NegotiateStream nStream;

        private Сonnect dlgConnect = new Сonnect();
        private MyAccaunt dlgMyAccaunt = new MyAccaunt();

        public Home()
        {
            InitializeComponent();
            button1.Enabled = false;
            textBox1.Enabled = false;
            WinConToolStripMenuItem.Enabled = false;
            listBox1.Items.Add($"[{DateTime.Now}] Пройдите Windows авторицацию");
            WinAutВToolStripMenuItem_Click(new object(), new EventArgs());
        }

        private void WriteMessage(string _message)
        {
            byte[] message = Encoding.Unicode.GetBytes(_message);
            nStream.Write(message, 0, message.Length);
            nStream.WriteTimeout = 1500;
            nStream.ReadTimeout = 1500;
        }
        private void ReadMessage()
        {
            int bytesRead = 0;
            while (true)
            {
                byte[] recievByte = new byte[client.ReceiveBufferSize];
                try { if ((bytesRead = nStream.Read(recievByte, 0, client.ReceiveBufferSize)) <= 0) break; }
                catch (Exception) { break; }

                foreach (string value in Encoding.Unicode.GetString(recievByte).Trim().Replace("\0", "").Split('\n'))
                {
                    if (value.Trim() == "") { continue; }
                    if (value.ToLower() == "exit") { client.Close(); break; }
                    listBox1.Items.Add(value.Trim());
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            textBox1.Enabled = false;
            try
            {
                string command = textBox1.Text;
                textBox1.Text = "";
                listBox1.Items.Add($"[{DateTime.Now}] : {command}");
                WriteMessage(command);
                ReadMessage();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            textBox1.Focus();
            button1.Enabled = true;
            textBox1.Enabled = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, null);
            }
        }

        private void WinAutВToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            dlgMyAccaunt.ShowDialog(this);
            if (dlgMyAccaunt.DialogResult != DialogResult.OK) { return; }

            client = new System.Net.Sockets.TcpClient();
            try { dlgMyAccaunt.setTimer(false); client.Connect(dlgMyAccaunt.getTextIP(), dlgMyAccaunt.getPort()); }
            catch (Exception ex) { listBox1.Items.Add($"[{DateTime.Now}] Ошбка: {ex.Message}"); return; }

            try
            {
                nStream = new NegotiateStream(client.GetStream());
                nStream.AuthenticateAsClient(new NetworkCredential(dlgMyAccaunt.getLogin(), dlgMyAccaunt.getPassword()),
                                                dlgMyAccaunt.getDomain(),
                                                ProtectionLevel.None,
                                                TokenImpersonationLevel.Impersonation);
                listBox1.Items.Add($"[{DateTime.Now}] Аутентификация пройдена [ip: {dlgMyAccaunt.getTextIP()}; port: {dlgMyAccaunt.getPort()}]");

                WinConToolStripMenuItem_Click(new object(), new EventArgs());

                WinConToolStripMenuItem.Enabled = true;
                WinAutВToolStripMenuItem.Enabled = false;
            }
            catch (InvalidCredentialException)
            {
                listBox1.Items.Add($"[{DateTime.Now}] Аутентификация на сервере не пройдена");
                button1.Enabled = false;
                textBox1.Enabled = false;
                WinConToolStripMenuItem.Enabled = false;
            }
        }

        private void WinConToolStripMenuItem_Click(object sender, EventArgs e)
        {

            dlgConnect.ShowDialog(this);
            if (dlgConnect.DialogResult != DialogResult.OK) { return; }

            try
            {
                WriteMessage(dlgConnect.getDomain());
                ReadMessage();
                WriteMessage(dlgConnect.getLogin());
                ReadMessage();
                WriteMessage(dlgConnect.getPassword());
                ReadMessage();

                button1.Enabled = true;
                textBox1.Enabled = true;
                WinConToolStripMenuItem.Enabled = false;
            }
            catch (Exception ex)
            {
                listBox1.Items.Add($"[{DateTime.Now}] Ошибка: {ex.Message}");
            }
        }
    }
}

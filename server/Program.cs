using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;


namespace TcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 1;
            TcpListener tcpServer = new TcpListener(IPAddress.Any, Setting());

            try
            {
                tcpServer.Start();
                Console.WriteLine("> system: Сервер успешно запущен.\n" +
                                  "> system: Ожидание новых подключений...\n");
                while (true)
                {
                    ThreadPool.QueueUserWorkItem(NewClient, tcpServer.AcceptTcpClient());
                    Console.WriteLine("> system: Подключение №" + count.ToString() + "!");
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("> system: Невозможно запустить сервер: " + ex.Message);
            }
            tcpServer.Stop();
            Console.ReadLine();
        }
        static int Setting()
        {
            int port = 8888;
            Console.Title = $"TcpServer v2.0 | Ipv4: {Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString()} | port: {port}";
            int minThreadsCount = 2;
            ThreadPool.SetMinThreads(minThreadsCount, minThreadsCount);
            int maxThreadsCount = Environment.ProcessorCount * 4;
            ThreadPool.SetMaxThreads(maxThreadsCount, maxThreadsCount);

            Console.WriteLine("Текущие настройки сервера:\n" +
                              $" - Порт для прослушивания входящих попыток подключения - {port}\n" +
                              $" - Минимальное количество рабочих потоков - {minThreadsCount}\n" +
                              $" - Минимальное количество потоков асинхронного ввода-вывода - {minThreadsCount}\n" +
                              $" - Максимальное количество рабочих потоков в пуле потоков - {maxThreadsCount}\n" +
                              $" - Максимальное количество потоков асинхронного ввода-вывода в пуле потоков - {maxThreadsCount}");
            return port;
        }
        static string ReadMessage(TcpClient client, NegotiateStream nStream, string userName)
        {
            byte[] byteMessage = new byte[client.ReceiveBufferSize];
            int bytesRead = nStream.Read(byteMessage, 0, client.ReceiveBufferSize);
            string message = Encoding.Unicode.GetString(byteMessage).Replace("\0", "").Trim();
            Console.WriteLine($"> {userName}: " + message);
            return message;
        }
        static void WriteMessage(NegotiateStream nStream, string _message)
        {
            byte[] message = Encoding.Unicode.GetBytes(_message);
            nStream.Write(message, 0, message.Length);
        }
        static void NewClient(object objClient)
        {
            TcpClient client = objClient as TcpClient;
            try
            {
                NegotiateStream nStream = new NegotiateStream(client.GetStream());

                nStream.AuthenticateAsServer((NetworkCredential)CredentialCache.DefaultCredentials,
                                               ProtectionLevel.None,
                                               TokenImpersonationLevel.Impersonation);

                WindowsIdentity winIdentity = (WindowsIdentity)nStream.RemoteIdentity;

                if (!winIdentity.IsAuthenticated) { Console.WriteLine($"> system: Клиент [{winIdentity.Name}\\\\{winIdentity.Token}] не аутентифицирован"); return; }
                Console.WriteLine($"> system: Клиент [{winIdentity.Name}\\\\{winIdentity.Token}] аутентифицирован");

                Process process = new Process();
                try
                {
                    process.StartInfo.WorkingDirectory = @"C:\Windows\System32";
                    process.StartInfo.FileName = @"cmd.exe";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    
                    // Domain
                    process.StartInfo.Domain = ReadMessage(client, nStream, winIdentity.Name + "\\" + winIdentity.Token);
                    WriteMessage(nStream, "\tDomain\t\t-\tOK\n");
                    
                    // Login
                    process.StartInfo.UserName = ReadMessage(client, nStream, winIdentity.Name + "\\" + winIdentity.Token);
                    WriteMessage(nStream, "\tLogin\t\t-\tOK\n");
                    
                    // Password
                    string password = ReadMessage(client, nStream, winIdentity.Name + "\\" + winIdentity.Token);
                    WriteMessage(nStream, "\tPassword\t-\tOK\n");

                    SecureString securePassword = new SecureString();
                    for (int j = 0; j < password.Length; j++)
                        securePassword.AppendChar(password[j]);

                    process.StartInfo.Password = securePassword;
                    process.Start();

                    WriteMessage(nStream, "Успешный запуск консоли на стороне сервера!");
                    Console.WriteLine("> system: Успешный запуск консоли. Пользователь: " + winIdentity.Name + "\\" + winIdentity.Token);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("> system: Ошибка запуска консоли:" + _ex.Message);
                    WriteMessage(nStream, "Ошибка запуска консоли на стороне сервера. Вы были отключены!");
                    client.Close();
                    return;
                }

                Thread thread = new Thread(function);
                Profile profile = new Profile();

                try
                {
                    profile.process = process;
                    profile.nStream = nStream;
                    thread.Start(profile);
                    while (true)
                    {
                        string Data = ReadMessage(client, nStream, winIdentity.Name + "\\" + winIdentity.Token);
                        process.StandardInput.WriteLine(Data);
                        process.StandardInput.Flush();

                        if (Data.Split(' ')[0] == "console_exit")
                        {
                            Console.WriteLine("> system: Введена команда console_exit");
                            if (process.HasExited)
                            {
                                WriteMessage(nStream, "\nconsole_exit\n");
                                break;
                            }
                        }
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("> system: Ошибка подключения клиента: " + _ex.Message);
                }
                process.WaitForExit();
                if (nStream != null) nStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("> system: Ошибка подключения клиента: " + ex.Message);
            }

            Console.WriteLine("> system: Соединение с пользователем принудительно разорвано!");
            client.Close();
        }

        private static void function(object obj)
        {
            string outputString;
            Profile profile = (Profile)obj;
            Process process = profile.process;
            NegotiateStream stream = profile.nStream;

            while (!(process.StandardOutput.EndOfStream))
            {
                if (process.HasExited)
                {
                    process.Close();
                    stream.Close();
                    break;
                }
                outputString = process.StandardOutput.ReadLine() + "\n";
                byte[] message = Encoding.Unicode.GetBytes(outputString);
                try
                {
                    stream.Write(message, 0, message.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("> system: Ошибка: " + ex.Message);
                    process.Close();
                    stream.Close();
                    break;
                }
            }
        }
        private class Profile
        {
            public NegotiateStream nStream;
            public Process process;
        }
    }
}
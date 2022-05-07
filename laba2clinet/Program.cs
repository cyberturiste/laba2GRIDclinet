using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace SocketTcpClient
{
    public static class Resolver
    {
        private static volatile bool _loaded;

        public static void RegisterDependencyResolver()
        {
            if (!_loaded)
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnResolve;
                _loaded = true;
            }
        }

        private static Assembly OnResolve(object sender, ResolveEventArgs args)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = String.Format("{0}.{1}.dll",
                execAssembly.GetName().Name,
                new AssemblyName(args.Name).Name);

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                int read = 0, toRead = (int)stream.Length;
                byte[] data = new byte[toRead];

                do
                {
                    int n = stream.Read(data, read, data.Length - read);
                    toRead -= n;
                    read += n;
                } while (toRead > 0);

                return Assembly.Load(data);
            }
        }
    }

    class Program
    {
        
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8005; // порт сервера
        static string address = "192.168.2.127"; // адрес сервера
        static void Main(string[] args)
        {
            try
            {
                Resolver.RegisterDependencyResolver();

                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // подключаемся к удаленному хосту
                socket.Connect(ipPoint);
                Console.Write("Введите сообщение:");
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                socket.Send(data);

                // получаем ответ
                data = new byte[256]; // буфер для ответа
                StringBuilder builder = new StringBuilder();
                int bytes = 0; // количество полученных байт

                do
                {
                    bytes = socket.Receive(data, data.Length, 0);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (socket.Available > 0);
                Console.WriteLine("ответ сервера: " + builder.ToString());

                // закрываем сокет
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador;
        private static Dictionary<string, int> contadores = new Dictionary<string, int>();

        static void Main()
        {
            escuchador = new TcpListener(IPAddress.Any, 8080);
            escuchador.Start();
            Console.WriteLine("Servidor inició en el puerto 8080...");
            while (true)
            {
                TcpClient cliente = escuchador.AcceptTcpClient();
                new Thread(ManipuladorCliente).Start(cliente);
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            try
            {
                using (NetworkStream flujo = cliente.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int leidos;
                    while ((leidos = flujo.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        string mensaje = Encoding.UTF8.GetString(buffer, 0, leidos);
                        var pedido = Protocolo.Pedido.Procesar(mensaje);
                        var resp = Protocolo.GestorProtocolo.ResolverPedido(pedido, cliente.Client.RemoteEndPoint.ToString(), contadores);
                        flujo.Write(Encoding.UTF8.GetBytes(resp.ToString()), 0, resp.ToString().Length);
                    }
                }
            }
            finally
            {
                cliente.Close();
            }
        }
    }
}
// ***************************************************
// Práctica 07 – Gestión de Protocolo
// Nombre: Verónica Aguilar
// Fecha de realización: 11/06/2025
// Fecha de entrega: 18/06/2025
// **********************************************


using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Protocolo
{
    /// <summary>
    /// Clase estática que gestiona el envío de pedidos (cliente)
    /// y el procesamiento de solicitudes (servidor).
    /// </summary>
    public static class GestorProtocolo
    {
        public static Respuesta HazOperacion(Pedido pedido)
        {
            try
            {
                using (var cliente = new TcpClient("127.0.0.1", 8080))
                using (var flujo = cliente.GetStream())
                {
                    byte[] bufferTx = Encoding.UTF8.GetBytes(pedido.ToString());
                    flujo.Write(bufferTx, 0, bufferTx.Length);

                    byte[] bufferRx = new byte[1024];
                    int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                    string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                    string[] partes = mensaje.Split(' ');
                    return new Respuesta
                    {
                        Estado = partes[0],
                        Mensaje = string.Join(" ", partes.Skip(1).ToArray())
                    };
                }
            }
            catch (SocketException ex)
            {
                return new Respuesta { Estado = "NOK", Mensaje = "Error de comunicación: " + ex.Message };
            }
        }

        public static Respuesta ResolverPedido(Pedido pedido, string direccionCliente, Dictionary<string, int> contadores)
        {
            var respuesta = new Respuesta { Estado = "NOK", Mensaje = "Comando no reconocido" };
            switch (pedido.Comando)
            {
                case "INGRESO":
                    if (pedido.Parametros.Length == 2 &&
                        pedido.Parametros[0] == "root" &&
                        pedido.Parametros[1] == "admin20")
                    {
                        respuesta = (new Random().Next(2) == 0)
                            ? new Respuesta { Estado = "OK", Mensaje = "ACCESO_CONCEDIDO" }
                            : new Respuesta { Estado = "NOK", Mensaje = "ACCESO_NEGADO" };
                    }
                    else
                    {
                        respuesta.Mensaje = "ACCESO_NEGADO";
                    }
                    break;

                case "CALCULO":
                    if (pedido.Parametros.Length == 3)
                    {
                        string placa = pedido.Parametros[2];
                        if (Regex.IsMatch(placa, "^[A-Z]{3}[0-9]{4}$"))
                        {
                            byte indicador = ObtenerIndicadorDia(placa);
                            respuesta = new Respuesta { Estado = "OK", Mensaje = placa + " " + indicador };
                            if (contadores.ContainsKey(direccionCliente))
                                contadores[direccionCliente]++;
                            else
                                contadores[direccionCliente] = 1;
                        }
                        else
                        {
                            respuesta.Mensaje = "Placa no válida";
                        }
                    }
                    break;

                case "CONTADOR":
                    if (contadores.ContainsKey(direccionCliente))
                    {
                        respuesta = new Respuesta
                        {
                            Estado = "OK",
                            Mensaje = contadores[direccionCliente].ToString()
                        };
                    }
                    else
                    {
                        respuesta.Mensaje = "No hay solicitudes previas";
                    }
                    break;
            }
            return respuesta;
        }

        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1));
            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes
                case 3:
                case 4:
                    return 0b00010000; // Martes
                case 5:
                case 6:
                    return 0b00001000; // Miércoles
                case 7:
                case 8:
                    return 0b00000100; // Jueves
                case 9:
                case 0:
                    return 0b00000010; // Viernes
                default:
                    return 0;
            }
        }
    }

    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }
        public static Pedido Procesar(string mensaje)
        {
            string[] partes = mensaje.Split(' ');
            return new Pedido { Comando = partes[0].ToUpper(), Parametros = partes.Skip(1).ToArray() };
        }
        public override string ToString()
        {
            return Comando + " " + string.Join(" ", Parametros);
        }
    }

    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }
        public override string ToString()
        {
            return Estado + " " + Mensaje;
        }
    }
}
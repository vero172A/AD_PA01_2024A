
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using Protocolo;
//
namespace Cliente
{
    public partial class FrmValidador : Form
    {
        public FrmValidador() { InitializeComponent(); }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            panLogin.Enabled = true;
            panPlaca.Enabled = false;

            chkDomingo.AutoCheck = false;
            chkLunes.AutoCheck = false;
            chkMartes.AutoCheck = false;
            chkMiercoles.AutoCheck = false;
            chkJueves.AutoCheck = false;
            chkViernes.AutoCheck = false;
            chkSabado.AutoCheck = false;

            chkDomingo.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkSabado.Enabled = false;
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string clave = txtPassword.Text.Trim();
            if (usuario == "" || clave == "")
            {
                MessageBox.Show("Usuario y contraseña requeridos", "ADVERTENCIA");
                return;
            }
            var pedido = new Protocolo.Pedido { Comando = "INGRESO", Parametros = new[] { usuario, clave } };
            var resp = Protocolo.GestorProtocolo.HazOperacion(pedido);
            if (resp.Estado == "OK" && resp.Mensaje == "ACCESO_CONCEDIDO")
            {
                panLogin.Enabled = false;
                panPlaca.Enabled = true;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
            }
            else
            {
                MessageBox.Show("Acceso denegado", "ERROR");
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            
            var pedido = new Protocolo.Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { txtModelo.Text, txtMarca.Text, txtPlaca.Text }
            };
            var resp = Protocolo.GestorProtocolo.HazOperacion(pedido);
            if (resp.Estado != "OK")
            {
                MessageBox.Show("Error en la solicitud", "ERROR");
                DesmarcarDias();
                return;
            }
            string[] partes = resp.Mensaje.Split(' ');
            byte indicador;
            if (Byte.TryParse(partes[1], out indicador))
            {
                chkLunes.Checked = (indicador == 0b00100000);
                chkMartes.Checked = (indicador == 0b00010000);
                chkMiercoles.Checked = (indicador == 0b00001000);
                chkJueves.Checked = (indicador == 0b00000100);
                chkViernes.Checked = (indicador == 0b00000010);
            }
        }

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            var resp = Protocolo.GestorProtocolo.HazOperacion(new Protocolo.Pedido { Comando = "CONTADOR", Parametros = new[] { "hola" } });
            if (resp.Estado == "OK")
                MessageBox.Show("Número de pedidos: " + resp.Mensaje, "INFORMACIÓN");
            else
                MessageBox.Show("Error en la solicitud", "ERROR");
        }

        private void DesmarcarDias()
        {
            chkLunes.Checked = chkMartes.Checked = chkMiercoles.Checked = chkJueves.Checked = chkViernes.Checked = false;
        }
    }
}
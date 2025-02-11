using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace projetoControlarVendas
{
    public partial class FormVendas : Form
    {
        public FormVendas()
        {
            InitializeComponent();
        }

        private void btnNovaVenda_Click(object sender, EventArgs e)
        {
            FormNovaVenda formNovaVenda = new FormNovaVenda();
            formNovaVenda.ShowDialog();
        }

        private void btnProdutos_Click(object sender, EventArgs e)
        {
            FormProdutos formProdutos = new FormProdutos();
            formProdutos.ShowDialog();
        }

        private void FormVendas_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void FormVendas_Load(object sender, EventArgs e)
        {

        }
    }
}

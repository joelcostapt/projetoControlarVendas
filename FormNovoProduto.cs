using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace projetoControlarVendas
{
    public partial class FormNovoProduto : Form
    {
        public FormNovoProduto()
        {
            InitializeComponent();
        }

        private void btnRegistar_Click(object sender, EventArgs e)
        {
            string nomeProduto = txtNomeProduto.Text;
            decimal preco = decimal.Parse(txtPreco.Text, CultureInfo.InvariantCulture);

            using (SqlConnection conn = new SqlConnection(MSSQL.StringConnection))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO sv_produtos (nome, preco) VALUES (@Nome, @Preco)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nome", nomeProduto);
                        cmd.Parameters.AddWithValue("@Preco", preco);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Produto inserido com sucesso!");
                        }
                        else
                        {
                            MessageBox.Show("Falha ao inserir o produto!");
                        }
                    }
                } catch (Exception ex)
                {
                    MessageBox.Show("MSSQL: " + ex.Message);
                }
            }

            // TENTAR CARREGAR PRODUTOS ATUALIZADOS APOS SUBMISSÃO ???

            /*
            FormProdutos formProdutos = new FormProdutos();
            formProdutos.CarregarProdutos();
            formProdutos.Refresh();
            */
            this.Close();
        }
    }
}

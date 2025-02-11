using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace projetoControlarVendas
{
    public partial class FormProdutos : Form
    {
        public FormProdutos()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnApagarProduto_Click(object sender, EventArgs e)
        {

        }

        private void btnAdicionarProduto_Click(object sender, EventArgs e)
        {
            FormNovoProduto formNovoProduto = new FormNovoProduto();
            formNovoProduto.ShowDialog();
        }

        private void FormProdutos_Load(object sender, EventArgs e)
        {
            CarregarProdutos();
        }

        public void CarregarProdutos()
        {
            using (SqlConnection conn = new SqlConnection(MSSQL.StringConnection)){
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM sv_produtos";

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            da.Fill(dt);
                            dgvProdutos.DataSource = dt;
                        }
                    }

                } catch (Exception ex)
                {
                    MessageBox.Show("MSSQL: " + ex.Message);
                }
            }
        }
    }
}

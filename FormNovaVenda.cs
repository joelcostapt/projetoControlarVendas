using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace projetoControlarVendas
{
    public partial class FormNovaVenda : Form
    {
        public FormNovaVenda()
        {
            InitializeComponent();
        }

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (cmbProdutos.SelectedIndex == -1 || string.IsNullOrEmpty(txtQuantidade.Text)) return;

            int idProduto = Convert.ToInt32(cmbProdutos.SelectedValue);
            string nomeProduto = cmbProdutos.Text;
            int quantidade;
            if (!int.TryParse(txtQnt.Text, out quantidade))
            {
                MessageBox.Show("Quantidade inválida! Introduza um número inteiro válido.");
                return;
            }



            using (SqlConnection conn = new SqlConnection(MSSQL.StringConnection))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT preco FROM sv_produtos WHERE id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", idProduto);
                    
                    decimal preco = (decimal)cmd.ExecuteScalar();

                    decimal total = quantidade * preco;

                    dgvPVenda.Rows.Add(idProduto, nomeProduto, quantidade, preco, total);

                    CalcularTotal();

                } catch (Exception ex)
                {
                    MessageBox.Show("MSSQL:" + ex.Message);
                }
            }
        }

        private void CalcularTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvPVenda.Rows)
            {
                total += Convert.ToDecimal(row.Cells["total"].Value);
            }

            lblSubTotal.Text = $"{total:C2}";

            decimal iva = 23;

            decimal totalIva = total * (iva / 100);

            decimal final = total + totalIva;

            lblIva.Text = $"{iva}%";
            lblTotal.Text = final.ToString("F2") + " €";
        }

        private void btnFinalizarVenda_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(MSSQL.StringConnection))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    string queryVenda = "INSERT INTO sv_vendas (total) OUTPUT INSERTED.ID VALUES (@total)";
                    SqlCommand cmdVenda = new SqlCommand(queryVenda, con, transaction);

                    cmdVenda.Parameters.AddWithValue("@total", lblTotal.Text.Replace(" €", ""));

                    int idVenda = (int)cmdVenda.ExecuteScalar();

                    foreach (DataGridViewRow row in dgvPVenda.Rows)
                    {
                        string queryItem = "INSERT INTO sv_itensVenda (id_venda, id_produto, quantidade, preco) VALUES (@idVenda, @idProduto, @quantidade, @preco)";
                        SqlCommand cmdItem = new SqlCommand(queryItem, con, transaction);
                        cmdItem.Parameters.AddWithValue("@idVenda", idVenda);
                        cmdItem.Parameters.AddWithValue("@idProduto", 1);
                        cmdItem.Parameters.AddWithValue("@quantidade", 1);
                        cmdItem.Parameters.AddWithValue("@preco", 19);
                        cmdItem.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Venda registrada com sucesso!");
                    string nomeCliente = "Joel Filipe Fernandes Costa";
                    string moradaCliente = "Urb. Quinta da Regucha, Lote 44 - 1º Direito";
                    string nifCliente = "256983976";
                    GerarFatura(idVenda, nomeCliente, moradaCliente, nifCliente);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Erro ao registrar venda.");
                    Console.WriteLine(ex.Message);
                }
            }
            this.Close();
        }


        private void GerarFatura(int idVenda, string nomeCliente, string moradaCliente, string nifCliente)
        {
            string caminhoPDF = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Fatura_{idVenda}.pdf");

            Document doc = new Document(PageSize.A4);
            PdfWriter.GetInstance(doc, new FileStream(caminhoPDF, FileMode.Create));
            doc.Open();

            // Fontes
            iTextSharp.text.Font fonteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            iTextSharp.text.Font fonteNegrito = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            iTextSharp.text.Font fonteTexto = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Nome da empresa e detalhes
            Paragraph empresa = new Paragraph("Tech Solutions Lda.", fonteTitulo)
            {
                Alignment = Element.ALIGN_CENTER
            };
            doc.Add(empresa);

            Paragraph detalhesEmpresa = new Paragraph(
                "Rua das Inovações, 123 - 1000-001 Lisboa\n" +
                "NIF: 123456789 | Tel: 211 234 567 | Email: info@techsolutions.pt\n" +
                "---------------------------------------------------------------\n",
                fonteTexto);
            detalhesEmpresa.Alignment = Element.ALIGN_CENTER;
            doc.Add(detalhesEmpresa);

            doc.Add(new Paragraph("\n"));

            // Destinatário (Cliente)
            Paragraph cliente = new Paragraph("Destinatário", fonteNegrito);
            doc.Add(cliente);

            Paragraph detalhesCliente = new Paragraph(
                $"Nome: {nomeCliente}\n" +
                $"Morada: {moradaCliente}\n" +
                $"NIF: {nifCliente}\n",
                fonteTexto);
            doc.Add(detalhesCliente);

            doc.Add(new Paragraph("\n"));

            // Número da venda e data
            Paragraph infoVenda = new Paragraph($"Fatura Nº: {idVenda}      Data: {DateTime.Now.ToString("dd/MM/yyyy")}", fonteNegrito);
            infoVenda.Alignment = Element.ALIGN_RIGHT;
            doc.Add(infoVenda);

            doc.Add(new Paragraph("\n"));

            // Tabela de Produtos
            PdfPTable tabela = new PdfPTable(5);
            tabela.WidthPercentage = 100;
            tabela.SetWidths(new float[] { 40f, 10f, 15f, 15f, 20f });

            tabela.AddCell(new PdfPCell(new Phrase("Produto", fonteNegrito)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabela.AddCell(new PdfPCell(new Phrase("Qtd.", fonteNegrito)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabela.AddCell(new PdfPCell(new Phrase("Preço Unitário", fonteNegrito)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabela.AddCell(new PdfPCell(new Phrase("IVA (%)", fonteNegrito)) { HorizontalAlignment = Element.ALIGN_CENTER });
            tabela.AddCell(new PdfPCell(new Phrase("Total", fonteNegrito)) { HorizontalAlignment = Element.ALIGN_CENTER });

            decimal subtotal = 0;
            decimal totalIVA = 0;
            decimal totalFinal = 0;
            decimal ivaPercentagem = 23; // IVA de 23%

            foreach (DataGridViewRow row in dgvPVenda.Rows)
            {
                if (row.IsNewRow) continue;

                string nomeProduto = row.Cells["nomeProduto"].Value?.ToString() ?? "Produto desconhecido";
                int quantidade = Convert.ToInt32(row.Cells["quantidade"].Value ?? "0");
                decimal preco = Convert.ToDecimal(row.Cells["preco"].Value ?? "0.00");
                decimal totalProduto = quantidade * preco;
                decimal ivaProduto = totalProduto * (ivaPercentagem / 100);

                subtotal += totalProduto;
                totalIVA += ivaProduto;
                totalFinal = subtotal + totalIVA;

                tabela.AddCell(new PdfPCell(new Phrase(nomeProduto, fonteTexto)) { HorizontalAlignment = Element.ALIGN_LEFT });
                tabela.AddCell(new PdfPCell(new Phrase(quantidade.ToString(), fonteTexto)) { HorizontalAlignment = Element.ALIGN_CENTER });
                tabela.AddCell(new PdfPCell(new Phrase($"{preco:F2} €", fonteTexto)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                tabela.AddCell(new PdfPCell(new Phrase($"{ivaPercentagem}%", fonteTexto)) { HorizontalAlignment = Element.ALIGN_CENTER });
                tabela.AddCell(new PdfPCell(new Phrase($"{totalProduto:F2} €", fonteTexto)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }
            doc.Add(tabela);

            doc.Add(new Paragraph("\n"));

            // Resumo do pagamento
            PdfPTable resumoTabela = new PdfPTable(2);
            resumoTabela.WidthPercentage = 50;
            resumoTabela.HorizontalAlignment = Element.ALIGN_RIGHT;

            resumoTabela.AddCell(new PdfPCell(new Phrase("Subtotal:", fonteNegrito)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
            resumoTabela.AddCell(new PdfPCell(new Phrase($"{subtotal:F2} €", fonteTexto)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

            resumoTabela.AddCell(new PdfPCell(new Phrase($"IVA ({ivaPercentagem}%):", fonteNegrito)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
            resumoTabela.AddCell(new PdfPCell(new Phrase($"{totalIVA:F2} €", fonteTexto)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

            resumoTabela.AddCell(new PdfPCell(new Phrase("Total a Pagar:", fonteTitulo)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
            resumoTabela.AddCell(new PdfPCell(new Phrase($"{totalFinal:F2} €", fonteTitulo)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(resumoTabela);

            doc.Add(new Paragraph("\n\n"));

            // Mensagem final
            Paragraph agradecimento = new Paragraph("Obrigado pela sua compra! Volte sempre.", fonteTexto);
            agradecimento.Alignment = Element.ALIGN_CENTER;
            doc.Add(agradecimento);

            doc.Close();

            MessageBox.Show("Fatura gerada com sucesso!");
            Process.Start(new ProcessStartInfo(caminhoPDF) { UseShellExecute = true });
        }


        private void FormNovaVenda_Load(object sender, EventArgs e)
        {
            dgvPVenda.Columns.Clear();
            dgvPVenda.Columns.Add("id", "ID");
            dgvPVenda.Columns.Add("nomeProduto", "Produto");
            dgvPVenda.Columns.Add("quantidade", "Quantidade");
            dgvPVenda.Columns.Add("preco", "Preço (€)");
            dgvPVenda.Columns.Add("total", "Total (€)");

            CarregarProdutosComboBox();
        }

        private void CarregarProdutosComboBox()
        {
            using (SqlConnection conn = new SqlConnection(MSSQL.StringConnection))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT id, nome FROM sv_produtos";
                    /*
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();


                    cmbProdutos.Items.Clear();
                    while (reader.Read())
                    {
                        cmbProdutos.Items.Add(reader["nome"].ToString());
                    }
                    */

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbProdutos.DataSource = dt;
                    cmbProdutos.DisplayMember = "Nome";
                    cmbProdutos.ValueMember = "ID";

                } catch (Exception ex)
                {
                    MessageBox.Show("MSSQL: " + ex.Message);
                }
            }
        }

        private void txtQnt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true; // Bloqueia qualquer caractere que não seja número ou "-"
            }
        }

    }
}

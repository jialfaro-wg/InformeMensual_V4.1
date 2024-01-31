using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OfficeOpenXml;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using OfficeOpenXml;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace InformeMensual_v4._1
{
    public partial class Form1 : Form
    {
        private DataTable dataTable;
        private string selectedMonth;
        private MemoryStream pdfMemoryStream;

        public Form1()
        {
            InitializeComponent();
            //InicializarComponentes();
            cmbMes.Items.AddRange(new[]
            {
                "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
            });
        }

        
        private void btnCargarExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos Excel|*.xlsx;*.xls";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    dataTable = new DataTable();

                    foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                    {
                        dataTable.Columns.Add(firstRowCell.Text);
                    }

                    for (int rowNumber = 3; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                        var newRow = dataTable.NewRow();

                        foreach (var cell in row)
                        {
                            //newRow[cell.Start.Column - 1] = cell.Text;
                            int columnIndex = cell.Start.Column - 1;

                            if (columnIndex >= 0 && columnIndex < dataTable.Columns.Count)

                            {

                                newRow[columnIndex] = cell.Text;

                            }
                        }

                        dataTable.Rows.Add(newRow);
                    }
                }
            }
        }

        private void btnAplicarFiltro_Click(object sender, EventArgs e)
        {
            selectedMonth = cmbMes.SelectedItem?.ToString() ?? "";
            DataView dataView = new DataView(dataTable);
            dataView.RowFilter = $"Mes = '{selectedMonth}'";
            dataTable = dataView.ToTable();

            if (cmbMes.SelectedItem != null)
            {
                MessageBox.Show("Filtro aplicado", "ツ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Seleccione un mes", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            

        }

        private void btnGenerarInforme_Click(object sender, EventArgs e)
        {
            List<Ticket> tickets = FilterTickets();
            GeneratePDF(tickets);
        }

        private List<Ticket> FilterTickets()
        {
            List<Ticket> tickets = new List<Ticket>();

            foreach (DataRow row in dataTable.Rows)
            {
                string numeroTicket = row["N°TK"].ToString();

                // Omitir el ticket si el número de ticket está vacío, es nulo, es una fecha o es solo un espacio en blanco
                if (string.IsNullOrEmpty(numeroTicket) || IsDate(numeroTicket) || string.IsNullOrWhiteSpace(numeroTicket))
                    continue;


                Ticket ticket = new Ticket
                {
                    Mes = row["Mes"].ToString(),
                    TipoTKT = row["Tipo-TKT"].ToString(),
                    NTK = row["N°TK"].ToString(),
                    Estado = row["Estado"].ToString(),
                    NameWS = row["WS"].ToString()
                };

                tickets.Add(ticket);
            }

            return tickets;
        }

        private bool IsDate(string input)
        {
            DateTime result;
            return DateTime.TryParse(input, out result);
        }

        private void GeneratePDF(List<Ticket> tickets)
        {
            // Crear un archivo temporal para el PDF
            string tempPdfPath = Path.Combine(Path.GetTempPath(), "Informe Mensual de Tickets.pdf");

            using (var writer = new PdfWriter(tempPdfPath))
            {
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new Document(pdf);

                    document.Add(new Paragraph("----------------------------------------------------------------").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    document.Add(new Paragraph("-------------------INFORME MENSUAL----------------").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    document.Add(new Paragraph("----------------------------------------------------------------").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    document.Add(new Paragraph($"Mes a evalauar: {selectedMonth.ToUpper()}").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    document.Add(new Paragraph("------TKT EN PROCESO------:").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    foreach (var ticket in tickets.Where(t => t.Estado.ToLower() == "en proceso" || t.Estado.ToLower() == "en tramite" || t.Estado.ToLower() == "abierto"))
                    {
                        document.Add(new Paragraph($"N°TK: {ticket.NTK}").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    }

                    document.Add(new Paragraph("------TKT GENERAL------").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    document.Add(new Paragraph($"* Cantidad Total --> {tickets.Count}").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    document.Add(new Paragraph($"* Cantidad Cerrado --> {tickets.Count(t => t.Estado.ToLower() == "cerrado" || t.Estado.ToLower() == "cerrado completo" || t.Estado.ToLower() == "derivado") }").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    document.Add(new Paragraph($"* Cantidad en Trámite --> {tickets.Count(t => t.Estado.ToLower() == "en proceso" || t.Estado.ToLower() == "abierto")}").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    document.Add(new Paragraph("------TKT TIPO------").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    document.Add(new Paragraph($"* Incidente --> {tickets.Count(t => t.TipoTKT.ToLower() == "incidente")}").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    document.Add(new Paragraph($"* Requerimiento --> {tickets.Count(t => t.TipoTKT.ToLower() == "requerimiento" || t.TipoTKT.ToLower() == "elemento pedido")}").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    document.Add(new Paragraph($"* Consulta --> {tickets.Count(t => t.TipoTKT.ToLower() == "consulta")}").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    var groupedByWS = tickets.GroupBy(t => t.NameWS);

                    document.Add(new Paragraph("------TKT WEB SERVICES------").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    foreach (var group in groupedByWS)
                    {
                        document.Add(new Paragraph("---------------------------------------").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        document.Add(new Paragraph($" WS : {group.Key}").SetBold().SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        var groupedByType = group.GroupBy(t => t.TipoTKT);
                        foreach (var typeGroup in groupedByType)
                        {
                            document.Add(new Paragraph($" * {typeGroup.Key} --> {typeGroup.Count()} Tickets").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        }
                    }
                }
            }

            // Mostrar el segundo formulario con la ruta del informe generado
            Form2 form2 = new Form2(tempPdfPath);
            form2.Show();
        }

        private void cmbMes_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

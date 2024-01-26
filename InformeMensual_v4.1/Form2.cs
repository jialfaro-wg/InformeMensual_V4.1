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

namespace InformeMensual_v4._1
{
    public partial class Form2 : Form
    {
        //private MemoryStream pdfMemoryStream;
        private string pdfPath;  // Declarar la variable para almacenar la ruta del archivo PDF
        public Form2(string pdfPath)
        {
            InitializeComponent();
            this.pdfPath = pdfPath;

            LoadPdf();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
           //webBrowser1.DocumentStream = pdfMemoryStream;


        }

        private void btnDescargarInforme_Click(object sender, EventArgs e)
        {
            // Abrir el explorador de archivos para seleccionar la ubicación de descarga
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos PDF|*.pdf";
            saveFileDialog.FileName = "Informe.pdf";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Copiar el archivo PDF al destino seleccionado por el usuario
                File.Copy(pdfPath, saveFileDialog.FileName, true);

                // Mostrar un mensaje de éxito
                MessageBox.Show("Informe descargado", "Descargar Informe", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            // Cerrar la aplicación al hacer clic en el botón "Finalizar"
            Application.Exit();
        }

        private void LoadPdf()
        {
            if (File.Exists(pdfPath))
            {
                // Cargar el documento PDF en el pdfViewer1
                pdfViewer1.LoadDocument(pdfPath);
            }
            else
            {
                MessageBox.Show("El archivo PDF no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        public void LoadPdfMemoryStream(MemoryStream pdfStream)
        {
            if (pdfStream == null || !pdfStream.CanSeek)
            {
                MessageBox.Show("Error al abrir el MemoryStream en Form2.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            // Asegúrate de que el MemoryStream esté en la posición correcta
            pdfStream.Seek(0, SeekOrigin.Begin);

            // Cargar el documento PDF en el pdfViewer1
            pdfViewer1.LoadDocument(pdfStream);
        }

        //private void btnFinalizar_Click_1(object sender, EventArgs e)
        //{

        //}

        //private void btnDescargarInforme_Click_1(object sender, EventArgs e)
        //{

        //}
    }
}

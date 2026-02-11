using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SA_ToolBelt
{
    public partial class DocxConverterForm : Form
    {
        private readonly DocxToMarkdownConverter _converter;
        private string? _selectedFilePath;

        public DocxConverterForm()
        {
            InitializeComponent();
            _converter = new DocxToMarkdownConverter();
            lblStatus.Text = "Idle";
            lblSelectedFile.Text = "No file selected";
            btnConvert.Enabled = false;
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a Word Document",
                Filter = "Word Documents (*.docx)|*.docx",
                FilterIndex = 1
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _selectedFilePath = ofd.FileName;
                lblSelectedFile.Text = System.IO.Path.GetFileName(_selectedFilePath);
                btnConvert.Enabled = true;
                lblStatus.Text = "Idle";
                lblStatus.ForeColor = System.Drawing.Color.Black;
            }
        }

        private async void btnConvert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
                return;

            btnConvert.Enabled = false;
            btnSelectFile.Enabled = false;
            lblStatus.Text = "Working";
            lblStatus.ForeColor = System.Drawing.Color.DarkOrange;

            try
            {
                string outputPath = await Task.Run(() => _converter.Convert(_selectedFilePath));

                lblStatus.Text = "Done";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                MessageBox.Show($"Conversion complete!\n\nSaved to:\n{outputPath}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error";
                lblStatus.ForeColor = System.Drawing.Color.Red;

                MessageBox.Show($"Conversion failed:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConvert.Enabled = !string.IsNullOrEmpty(_selectedFilePath);
                btnSelectFile.Enabled = true;
            }
        }
    }
}

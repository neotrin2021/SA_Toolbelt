// Enable class when we implement exporting data for reports
/*
using System;
using System.Runtime.InteropServices;


namespace SA_ToolBelt 
{
    public class ExportService
    {
        public void ExportToExcel(DataGridView dgv, string fileName)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Visible = false;
                Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
                Microsoft.Office.Interop.Excel.Worksheet sheet = workbook.ActiveSheet;

                // Headers
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    sheet.Cells[1, i + 1] = dgv.Columns[i].HeaderText;
                }

                // Data
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    for (int j = 0; j < dgv.Columns.Count; j++)
                    {
                        if (dgv.Rows[i].Cells[j].Value != null)
                        {
                            sheet.Cells[i + 2, j + 1] = dgv.Rows[i].Cells[j].Value.ToString();
                        }
                    }
                }

                // Save
                workbook.SaveAs(fileName);
                workbook.Close();
                excel.Quit();

                Marshal.ReleaseComObject(sheet);
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ExportToCSV(DataGridView dgv, string fileName)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false))
                {
                    // Headers
                    string headers = string.Join(",",
                        dgv.Columns.Cast<DataGridViewColumn>()
                        .Select(column => $"\"{column.HeaderText}\""));
                    sw.WriteLine(headers);

                    // Rows
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        string line = string.Join(",",
                            row.Cells.Cast<DataGridViewCell>()
                            .Select(cell => $"\"{(cell.Value ?? "").ToString()}\""));
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }


}
*/
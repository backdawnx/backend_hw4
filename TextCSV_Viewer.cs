/*
MIT License

Copyright (c) 2026 Sarayut Chaisuriya

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Note on dataset:
The included MalwareBazaar sample CSV has been modified:
- Limited to first 500 rows
- Header format adjusted for teaching purposes
See README.md for full details.
*/
using System;
using System.IO;
using System.Windows.Forms;

namespace FileProcessing
{
	public partial class frmTextView : Form
	{
		/// <summary>
		/// Initializes a new instance of the frmTextView class.
		/// </summary>
		public frmTextView()
		{
			InitializeComponent();

            cb_FileType.Items.Clear();

            // Combobox for filtering by file type
            cb_FileType.Items.Add("All");
            cb_FileType.Items.Add("exe");
            cb_FileType.Items.Add("dll");
            cb_FileType.Items.Add("elf");
            cb_FileType.Items.Add("sh");
            cb_FileType.Items.Add("lnk");
            cb_FileType.Items.Add("vbe");
            cb_FileType.Items.Add("xlsx");
            cb_FileType.Items.Add("tar");
            cb_FileType.Items.Add("rar");
            cb_FileType.Items.Add("msi");
            cb_FileType.Items.Add("bat");
            cb_FileType.Items.Add("js");
            cb_FileType.Items.Add("zip");
            cb_FileType.Items.Add("dll");
            cb_FileType.Items.Add("hta");

            cb_FileType.SelectedIndex = 0;
        }
		/// <summary>
		/// Handles the Click event of the Read button by loading the contents of the specified file into the display area.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event data.</param>
		private void btRead_Click(object sender, EventArgs e)
		{
			string content = File.ReadAllText(tbFileName.Text);
			rtbShow.Text = content;
		}
		/// <summary>
		/// Handles the Click event of the btReadCSV button, reading CSV data from the specified file and populating the
		/// DataGridView with its contents.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event data.</param>
		private void btReadCSV_Click(object sender, EventArgs e)
		{
			using (StreamReader srReader = new StreamReader(tbFileName.Text))
			{
				string strLine; // Variable to hold each line read from the file
				bool bHeaderRead = false;   // Flag to indicate whether the header line has been read

				// Main loop: Read the file line by line
				while ((strLine = srReader.ReadLine()) != null)
				{
					string[] strHeaders_arr = null;
					// Skip comment lines and check for header line
					if (strLine.StartsWith("#"))
					{
						if (strLine.Length > 8
						   && strLine.Substring(0, 8).Equals("#HEADER")
						   )
						{
							// Read the header line and split it into an array of headers
							strHeaders_arr = strLine.Substring(8).Split(',');
						}
						continue;
					}
					// Split the current line into an array of values
					string[] strValues_arr = strLine.Split(',');

					// If the header has not been read yet, add the headers to the DataGridView columns
					if (!bHeaderRead)
					{
						// Add the headers to the DataGridView columns, using the header names from the header line if available
						foreach (string strHeader in strValues_arr)
						{
							if (strHeaders_arr == null)
								dgvData.Columns.Add(strHeader.Trim(), strHeader.Trim());
							else
								dgvData.Columns.Add(strHeader.Trim(), strHeaders_arr[dgvData.Columns.Count].Trim());
						}
						bHeaderRead = true;
					}
					else
					{
						// Add the values to the DataGridView rows
						dgvData.Rows.Add(strValues_arr);
					}
				}   // Main loop: Read the file line by line
			}

		}
		/// <summary>
		/// Handles the Click event of the Browse button, allowing the user to select a file and displaying its path in the
		/// file name text box.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event data.</param>
		private void btBrowse_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
				if (ofd.ShowDialog() == DialogResult.OK)
				{
					tbFileName.Text = ofd.FileName;
				}
			}
		}

        // ==============================================================
        // Search & Filter Malware Data
        // ==============================================================

        private void LoadFilteredMalwareData()
        {
            // Check if the file path is valid and the file exists
            if (string.IsNullOrWhiteSpace(tbFileName.Text) || !File.Exists(tbFileName.Text))
            {
                MessageBox.Show("Please choose .csv file!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Pull startLine (m) and stopLine (n) from TextBoxes and validate them
            if (!int.TryParse(tb_m.Text, out int m) || !int.TryParse(tb_n.Text, out int n))
            {
                MessageBox.Show("Please enter startLine and stopLine with Integer!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Error Case: Ensure that stopLine (n) is greater than or equal to startLine (m)
            if (n < m)
            {
                MessageBox.Show("stopLine must be greater than startLine!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Pull data from ComboBox for filtering by file type
            string filterType = cb_FileType.SelectedItem?.ToString()?.Trim() ?? "";
            bool applyFilter = !string.IsNullOrEmpty(filterType) && !filterType.Equals("All", StringComparison.OrdinalIgnoreCase);

            dgvData.Rows.Clear(); // Remove existing rows before loading new data

            try
            {
                using (StreamReader srReader = new StreamReader(tbFileName.Text))
                {
                    string strLine;
                    bool bHeaderRead = false;
                    int currentRecord = 0;

                    while ((strLine = srReader.ReadLine()) != null)
                    {

                        if (strLine.StartsWith("#")) continue;

                        string[] strValues_arr = strLine.Split(',');

                        // Table Header
                        if (!bHeaderRead)
                        {
                            if (dgvData.Columns.Count == 0)
                            {
                                foreach (string strHeader in strValues_arr)
                                {
                                    dgvData.Columns.Add(strHeader.Trim(), strHeader.Trim());
                                }
                            }
                            bHeaderRead = true;
                            continue;
                        }

                        // Start counting records after the header
                        currentRecord++;

                        // Check Partial Loading
                        if (currentRecord >= m && currentRecord <= n)
                        {

                            string currentFileType = strValues_arr.Length > 6 ? strValues_arr[6].Replace("\"", "").Trim() : "";

                            // Filtering
                            if (!applyFilter || currentFileType.Equals(filterType, StringComparison.OrdinalIgnoreCase))
                            {
                                dgvData.Rows.Add(strValues_arr);
                            }
                        }

                        // Break the loop if program has already loaded up to line n
                        if (currentRecord > n)
                        {
                            break;
                        }
                    }
                }

                MessageBox.Show($"Searching Line {m} to {n} Successful!", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reason : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            LoadFilteredMalwareData();
        }
    }
}
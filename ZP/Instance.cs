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

namespace ZP
{
    public partial class Instance : Form
    {
        Metaheuristics metaheuristics;

        public Instance()
        {
            InitializeComponent();
        }

        public Random rand = new Random(); 
        DataGridView dataGrid;

        //additional functions
        public void Settings(DataGridView dataGridView) 
        {
            dataGridView.AllowUserToAddRows = false;  
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView.AllowUserToResizeColumns = false;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        }

        private void Delete() 
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView2.Rows.Clear();
            dataGridView2.Columns.Clear();
        }

        private void DeleteSelected(DataGridView dataGridView) 
        {
            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();
        }

        public void HeadersCol(DataGridView dataGridView, int tmp) 
        {
            for (int i = 0; i < tmp; i++)
            {
                dataGridView.ColumnCount = tmp;
                dataGridView.Columns[i].Name = "p" + Convert.ToString(i + 1);  
                dataGridView.Columns[i].Width = 40;
            }
        }

        public void HeadersRow(DataGridView dataGridView, int tmp) 
        {
            for (int i = 0; i < tmp; i++)
            {
                dataGridView.Rows[i].HeaderCell.Value = "f" + Convert.ToString(i + 1);
            }
        }

        private void Visibles() 
        {
            dataGridView2.Visible = true;
            label6.Visible = true;
            numericUpDown3.Visible = true;
            label4.Visible = true;
            RandomErrors.Visible = true;
            label12.Visible = true;
            UpdateOriginalMatrix.Visible = true;
        }

        private bool CheckMatrix(DataGridView dataGridView) 
        {
            if (dataGridView.Rows.Count == 0) 
            {
                MessageBox.Show("Lack of matrix!");
                return false;
            }

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                foreach (DataGridViewColumn col in dataGridView.Columns)
                {
                    if (dataGridView.Rows[row.Index].Cells[col.Index].Value == null)
                    {
                        MessageBox.Show("Empty field in the matrix!");
                        return false;
                    }
                    string value = dataGridView.Rows[row.Index].Cells[col.Index].Value.ToString();
                    bool isNumeric = int.TryParse(value, out int n);
                    if (!isNumeric || Convert.ToInt32(value) != 0 && Convert.ToInt32(value) != 1)
                    {
                        MessageBox.Show("Wrong value in matrix!");
                        return false;
                    }
                }
            }
            return true;
        }

        //GENERATORS 
        private void GenerateEmpty_Click(object sender, EventArgs e) //generating an empty matrix 
        {
            DeleteSelected(dataGridView3);

            int m = Convert.ToInt32(numericUpDown1.Value);
            int n = Convert.ToInt32(numericUpDown2.Value);
            HeadersCol(dataGridView3, n);
            dataGridView3.Rows.Add(m);  
            HeadersRow(dataGridView3, m);
            Settings(dataGridView3);
        }

        private void GenerateGenerator_Click(object sender, EventArgs e) //instance generator (matrix with 'consecutive ones' property) 
        {
            Delete();
            int m = Convert.ToInt32(numericUpDown4.Value);
            int n = Convert.ToInt32(numericUpDown5.Value);
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Choose a value!");
                return;
            }
            double percent = Convert.ToDouble(comboBox1.SelectedItem.ToString()); 
            percent = percent / 100; 
            int ones = Convert.ToInt32(Math.Floor(n * percent)); 
            if (ones == 0) 
            {
                ones = 1;
            }

            HeadersCol(dataGridView1, n);

            int[,] matrix = new int[m, n]; 
            for (int i = 0; i < m; i++)
            {
                int count_ones = rand.Next(1, ones + 1);
                int ind = n - count_ones; 
                int ind2 = rand.Next(ind + 1); 

                if(ind2 == 0) 
                {
                    for(int j = 0; j < n; j++)
                    {
                        if(j < count_ones)
                        {
                            matrix[i, j] = 1;
                        }
                        else
                        {
                            matrix[i, j] = 0;
                        }
                    }
                }
                else if (ind2 == ind) 
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (j < ind)
                        {
                            matrix[i, j] = 0;
                        }
                        else
                        {
                            matrix[i, j] = 1;
                        }
                    }
                }
                else 
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (ind2 <= j && j < ind2 + count_ones)
                        {
                            matrix[i, j] = 1;
                        }
                        else
                        {
                            matrix[i, j] = 0;
                        }
                    }
                }
            }
            


            for (int i = 0; i < m; i++) 
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1);
                for (int j = 0; j < n; j++)
                {
                    row.Cells[j].Value = matrix[i, j];
                }
                dataGridView1.Rows.Add(row);
            }


            HeadersRow(dataGridView1, m);
            Settings(dataGridView1);

            
            Visibles();

            dataGridView2.ColumnCount = dataGridView1.ColumnCount;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView2);
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {
                    row.Cells[j].Value = Convert.ToInt32(dataGridView1.Rows[i].Cells[j].Value.ToString());
                }
                dataGridView2.Rows.Add(row);
            }

            HeadersCol(dataGridView2, dataGridView1.ColumnCount);
            HeadersRow(dataGridView2, dataGridView2.Rows.Count);
            Settings(dataGridView2);
        }

        //RANDOMNESS OF ERRORS AND UPDATING THE MATRIX
        private bool CheckIfExist(List<int[]> lists, int m, int n) 
        {
            int[] tmp = new int[] { m, n };
            for (int i = 0; i < lists.Count; i++)
            {
                if (lists[i].SequenceEqual(tmp))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckValue(int[,] matrix, int m, int n) 
        {
            int row = matrix.GetLength(0);
            int col = matrix.GetLength(1);
            if(row >= 2 && col == 2) 
            {
                return false;
            }
            else if(n == 0) 
            {

                if(matrix[m, n] == 0 && matrix[m, n + 1] == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (n == col - 1) 
            {

                if (matrix[m, n] == 0 && matrix[m, n - 1] == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(matrix[m, n] == 1 && matrix[m, n - 1] == 1 && matrix[m, n + 1] == 1) 
            {
                return true;
            }
            else if (matrix[m, n] == 0 && matrix[m, n - 1] == 0 && matrix[m, n + 1] == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RandomErrors_Click(object sender, EventArgs e) 
        {
            DeleteSelected(dataGridView2);

            int errors_count = Convert.ToInt32(numericUpDown3.Value); 
            int max = dataGridView1.Rows.Count * dataGridView1.ColumnCount; 
            int[,] matrix = new int[dataGridView1.Rows.Count, dataGridView1.Columns.Count]; 
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    string value = dataGridView1.Rows[row.Index].Cells[col.Index].Value.ToString();
                    matrix[row.Index, col.Index] = Convert.ToInt32(value);
                }
            }

            List<int[]> indexes = new List<int[]>(); 
            List<int[]> all_indexes = new List<int[]>(); 
            int count = 0; 

            while (count != errors_count) 
            {
                int ind_m = rand.Next(dataGridView1.Rows.Count);
                int ind_n = rand.Next(dataGridView1.ColumnCount); 
                if (CheckValue(matrix, ind_m, ind_n) && !CheckIfExist(indexes, ind_m, ind_n))  
                {
                    
                    if (matrix[ind_m, ind_n] == 1) 
                    {
                        matrix[ind_m, ind_n] = 0;
                    }
                    else
                    {
                        matrix[ind_m, ind_n] = 1;
                    }
                    int[] tmp = new int[] { ind_m, ind_n }; 
                    indexes.Add(tmp);
                    all_indexes.Add(tmp);
                    count++;
                    continue;
                }
                else
                {
                    if(!CheckIfExist(all_indexes, ind_m, ind_n)) 
                    {
                        int[] tmp2 = new int[] { ind_m, ind_n };
                        all_indexes.Add(tmp2);
                    }
                    if (all_indexes.Count == max)
                    {
                        MessageBox.Show("Introduced " + count.ToString() + " errors.");
                        break;
                    }
                    continue;
                }
                
            }



            int r = matrix.GetLength(0);
            int c = matrix.GetLength(1);
            dataGridView2.ColumnCount = c;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView2);
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    row.Cells[j].Value = matrix[i, j];
                }
                dataGridView2.Rows.Add(row);
            }

            HeadersCol(dataGridView2, c);
            HeadersRow(dataGridView2, r);
            Settings(dataGridView2);
        }

        private void UpdateOriginalMatrix_Click(object sender, EventArgs e)  
        {
            if (!CheckMatrix(dataGridView2))
            {
                return;
            }
            DeleteSelected(dataGridView1);
            dataGridView1.ColumnCount = dataGridView2.ColumnCount;
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1);
                for (int j = 0; j < dataGridView2.ColumnCount; j++)
                {
                    row.Cells[j].Value = Convert.ToInt32(dataGridView2.Rows[i].Cells[j].Value.ToString());
                }
                dataGridView1.Rows.Add(row);
            }

            HeadersCol(dataGridView1, dataGridView2.ColumnCount);
            HeadersRow(dataGridView1, dataGridView1.Rows.Count);
            Settings(dataGridView1);
        }

        //SAVING TO FILES
        private void SaveGenerator_Click(object sender, EventArgs e) 
        {
            if (CheckMatrix(dataGridView2))
            {
                dataGrid = dataGridView1;
                saveFileDialog1.ShowDialog();
            }
        }

        private void SaveToFile(DataGridView dataGridView) 
        {
            string name = saveFileDialog1.FileName;
            List<string> tab = new List<string>();

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                string text = "";
                List<string> tmp = new List<string>();
                foreach (DataGridViewColumn col in dataGridView.Columns)
                {
                    tmp.Add(dataGridView.Rows[row.Index].Cells[col.Index].Value.ToString());
                }
                text = String.Join(" ", tmp);
                tab.Add(text);
            }
            tab.Add("//");
            if(dataGridView.Name != "dataGridView3")
            {
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    string text = "";
                    List<string> tmp = new List<string>();
                    foreach (DataGridViewColumn col in dataGridView2.Columns)
                    {
                        tmp.Add(dataGridView2.Rows[row.Index].Cells[col.Index].Value.ToString());
                    }
                    text = String.Join(" ", tmp);
                    tab.Add(text);
                }
            }
            
            File.WriteAllLines(name, tab);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) 
        {
            SaveToFile(dataGrid);
        }

        private void SaveEmpty_Click(object sender, EventArgs e) 
        {
            if (CheckMatrix(dataGridView3))
            {
                dataGrid = dataGridView3;
                saveFileDialog1.ShowDialog();
            }
        }

        //READING FROM FILES
        private void ReadGenerator_Click(object sender, EventArgs e)  
        {
            Delete();
            dataGridView2.Visible = true;

            DialogResult result = openFileDialog1.ShowDialog(); 
            if (result == DialogResult.OK) 
            {
                string file = openFileDialog1.FileName;
                try
                {
                    List<int> len = new List<int>();
                    string[] filelines = File.ReadAllLines(file); 
                    if( filelines.Length == 0)
                    {
                        MessageBox.Show("Lack of matrix!");
                        return;
                    }
                    int index = 0;
                    for (int i = 0; i < filelines.Length; i++)
                    {
                        if(filelines[i] == "//") 
                        {
                            if(i == filelines.Length - 1)
                            {
                                Delete();
                                MessageBox.Show("Lack of second matrix!");
                                return;
                            }
                            index = i - 1;
                            continue;
                        }
                        string[] dane = filelines[i].Split(' ');
                        len.Add(dane.Length);
                        for (int j = 0; j < dane.Length; j++) 
                        {
                            bool isNumeric = int.TryParse(dane[j], out int n);
                            if (!isNumeric || Convert.ToInt32(dane[j]) != 0 && Convert.ToInt32(dane[j]) != 1)
                            {
                                Delete();
                                MessageBox.Show("Wrong value in matrix!");
                                return;
                            }
                        }
                        if(i == 0)
                        {
                            HeadersCol(dataGridView1, dane.Length);
                            HeadersCol(dataGridView2, dane.Length);
                        }
                        if(i != 0)
                        {
                            if (len.Distinct().Count() != 1)  
                            {
                                Delete();
                                MessageBox.Show("Different number of fields in the matrix!");
                                return;
                            }
                        }
                        if(i > index)
                        {
                            dataGridView2.Rows.Add(dane);
                            continue;
                        }
                        dataGridView1.Rows.Add(dane);
                        index++;
                    }
                    
                    if(dataGridView2.Rows.Count == 0 || dataGridView1.Rows.Count != dataGridView2.Rows.Count)
                    {
                        Delete();
                        MessageBox.Show("Invalid file!");
                        return;
                    }
                    else
                    {
                        HeadersRow(dataGridView1, dataGridView1.Rows.Count);
                        HeadersRow(dataGridView2, dataGridView1.Rows.Count);
                        Settings(dataGridView1);
                        Settings(dataGridView2);

                        Visibles();
                        if (dataGridView1.Rows.Count < 2 || dataGridView1.ColumnCount < 2) 
                        {
                            Delete();
                            MessageBox.Show("Not enough matrix fields!");
                            return;
                        }
                    }
                    
                }
                catch (IOException)
                {
                    MessageBox.Show("File reading error!");
                    return;
                }
            }
        }

        private void ReadEmpty_Click(object sender, EventArgs e)
        {
            DeleteSelected(dataGridView3);

            DialogResult result = openFileDialog1.ShowDialog(); 
            if (result == DialogResult.OK) 
            {
                string file = openFileDialog1.FileName;
                try
                {
                    List<int> len = new List<int>();
                    string[] filelines = File.ReadAllLines(file); 

                    if (filelines.Length == 0)
                    {
                        MessageBox.Show("Lack of matrix!");
                        return;
                    }
                    for (int i = 0; i < filelines.Length; i++)
                    {
                        if (filelines[i] == "//" && i != filelines.Length - 1)  
                        {
                            DeleteSelected(dataGridView3);
                            MessageBox.Show("Wrong file format!");
                            return;
                        }
                        if (filelines[i] == "//" && i == filelines.Length - 1) 
                        {
                            continue;
                        }
                        string[] data = filelines[i].Split(' ');
                        len.Add(data.Length);
                        for (int j = 0; j < data.Length; j++) 
                        {
                            bool isNumeric = int.TryParse(data[j], out int n);
                            if (!isNumeric || Convert.ToInt32(data[j]) != 0 && Convert.ToInt32(data[j]) != 1)
                            {
                                DeleteSelected(dataGridView3);
                                MessageBox.Show("Wrong value in matrix!");
                                return;
                            }
                        }
                        if (i == 0)
                        {
                            HeadersCol(dataGridView3, data.Length);
                        }
                        if (i != 0)
                        {
                            if (len.Distinct().Count() != 1)  
                            {
                                DeleteSelected(dataGridView3);
                                MessageBox.Show("Different number of fields in the matrix!");
                                return;
                            }
                        }
                        dataGridView3.Rows.Add(data);
                    }
                    HeadersRow(dataGridView3, dataGridView3.Rows.Count);
                    Settings(dataGridView3);

                    if (dataGridView3.ColumnCount < 2) 
                    {
                        DeleteSelected(dataGridView3);
                        MessageBox.Show("Not enough matrix fields!");
                        return;
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("File reading error!");
                    return;
                }
            }
        }

        //METAHEURISTICS
        public List<int> RandomizeList(List<int> input) 
        {
            List<int> random = new List<int>();

            int index;
            while (input.Count > 0)
            {
                index = rand.Next(0, input.Count);
                random.Add(input[index]);
                input.RemoveAt(index);
            }

            return random;
        } 

        private int[,] CreateMatrix(DataGridView dataGridView) 
        {
            List<int> tmp = new List<int>(); 
            for (int i = 0; i < dataGridView.ColumnCount; i++)
            {
                tmp.Add(i);
            }

            List<int> random = new List<int>();
            random = RandomizeList(tmp);


            int[,] matrix = new int[dataGridView.Rows.Count, dataGridView.Columns.Count];
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                foreach (DataGridViewColumn col in dataGridView.Columns)
                {
                    string value = dataGridView.Rows[row.Index].Cells[random[col.Index]].Value.ToString();
                   // bool isNumeric = int.TryParse(value, out int n);
                    matrix[row.Index, col.Index] = Convert.ToInt32(value);
                }
            }
            return matrix;
        }

        private void SendToMetaheuristicsG_Click(object sender, EventArgs e) 
        {
            int[,] individual;
            if (CheckMatrix(dataGridView2))
            {
                individual = CreateMatrix(dataGridView2);
            }
            else
            {
                return;
            }

            if(metaheuristics == null) 
            {
                metaheuristics = new Metaheuristics(this);
                metaheuristics.Show();
            }

            this.metaheuristics.GetMatrix = individual;
        }

        private void SendToMetaheuristicsE_Click(object sender, EventArgs e) 
        {
            int[,] individual;
            if (CheckMatrix(dataGridView3))
            {
                individual = CreateMatrix(dataGridView3);
            }
            else
            {
                return;
            }

            if(metaheuristics == null)
            {
                metaheuristics = new Metaheuristics(this);
                metaheuristics.Show();
            }

            this.metaheuristics.GetMatrix = individual;
        }
    }
}

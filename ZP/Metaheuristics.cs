using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZP
{
    public partial class Metaheuristics : Form
    {
        private Individual best; //the best individual - updated after each iteration of the algorithm 
        private List<Individual> population;
        private Instance mainForm = null;
        int iteration; //current iteration 
        int gen; //the generation in which the best individual was found 
        Stopwatch watch = new Stopwatch(); 
        private int[,] main_matrix; //matrix from generator
        volatile bool Active = true;
        volatile bool Stop = false;

        public Metaheuristics()
        {
            InitializeComponent();
        }

        public Metaheuristics(Form callform) 
        {
            mainForm = callform as Instance;
            InitializeComponent();
        }

        public int[,] GetMatrix 
        {
            get { return main_matrix; }
            set
            {
                main_matrix = value;  
                Stop = true;
                label7.Text = "Instance loaded!";
            }
        }

        private void PopulationInitialization() 
        {
            int pop_size = Convert.ToInt32(numericUpDown1.Value); //population size

            List<int> tmp = new List<int>(); 
            for (int i = 0; i < main_matrix.GetLength(1); i++)
            {
                tmp.Add(i);
            }

            for (int size = 0; size < pop_size; size++)
            {
                List<int> random = new List<int>();
                List<int> copy_tmp = new List<int>(); 
                copy_tmp.AddRange(tmp);
                random = mainForm.RandomizeList(copy_tmp); 
                
                int row = main_matrix.GetLength(0);
                int col = main_matrix.GetLength(1);
                int[,] new_matrix = new int[row, col];

                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        new_matrix[i, j] = main_matrix[i, random[j]];
                    }
                }

                int fitness = Fitness(new_matrix);
                Individual new_individual = new Individual(new_matrix, row, col, fitness, random);
                population.Add(new_individual);
            }
            
        }

        private void Selection() //selection - tournament method 
        {
            int k = Convert.ToInt32(numericUpDown5.Value); //tournament size
            int pop_size = Convert.ToInt32(numericUpDown1.Value);
            List<Individual> new_population = new List<Individual>();

            for (int i = 0; i < pop_size; i++)
            {
                List<int> indexes = new List<int>();
                
                while (indexes.Count != k)
                {
                    int rand_index = mainForm.rand.Next(pop_size);
                    if (indexes.Contains(rand_index))
                    {
                        continue;
                    }
                    else
                    {
                        indexes.Add(rand_index);
                    }
                }

                Individual winner = new Individual (population[indexes[0]].matrix, population[indexes[0]].row, population[indexes[0]].col, population[indexes[0]].fitness, population[indexes[0]].original_indexes); 
                for (int j = 0; j < indexes.Count; j++)
                {
                    int index = indexes[j];
                    if(population[index].fitness < winner.fitness)
                    {
                        winner.matrix = population[index].matrix;
                        winner.fitness = population[index].fitness;
                        winner.original_indexes = population[index].original_indexes;
                    }
                }

                new_population.Add(winner); 
            }

            population = new_population;
        }

        private void Crossover() 
        {
            double crossover_p = Convert.ToDouble(numericUpDown3.Value); //crossover probability
            int count = Convert.ToInt32(Math.Floor(population.Count * crossover_p));
            if(count % 2 != 0) 
            {
                count++; 
            }

            List<int> indexes = new List<int>(); 
            while (indexes.Count != count)
            {
                int ind = mainForm.rand.Next(population.Count);
                if (indexes.Contains(ind))
                {
                    continue;
                }
                else
                {
                    indexes.Add(ind);
                }
            }

            for (int i = 0; i < count/2; i++)
            {
                int rand_ind1 = mainForm.rand.Next(main_matrix.GetLength(1)); 
                int rand_ind2 = mainForm.rand.Next(main_matrix.GetLength(1));

                int index_col1, index_col2;
                if(rand_ind1 < rand_ind2) 
                {
                    index_col1 = rand_ind1;
                    index_col2 = rand_ind2;
                }
                else
                {
                    index_col1 = rand_ind2;
                    index_col2 = rand_ind1;
                }

                int index_parent1 = mainForm.rand.Next(indexes.Count); 
                int index_parent2 = mainForm.rand.Next(indexes.Count);
                while (index_parent1 == index_parent2)
                {
                    index_parent2 = mainForm.rand.Next(indexes.Count);
                }

                Individual parent1 = population[indexes[index_parent1]];
                Individual parent2 = population[indexes[index_parent2]];
                Individual descendant1; 
                Individual descendant2;
                int[,] matrix1 = parent1.matrix.Clone() as int[,]; 
                int[,] matrix2 = parent2.matrix.Clone() as int[,];
                List<int> current1 = new List<int>();  
                List<int> current2 = new List<int>();
                List<int> check_second1 = new List<int>(); 
                List<int> check_second2 = new List<int>();

                if (index_col1 == 0 && index_col2 == parent1.col-1) 
                {
                    descendant1 = parent1;
                    descendant2 = parent2;
                }
                else if (index_col1 == 0)  
                {
                    for (int j = 0; j <= index_col2; j++) 
                    {
                        current1.Add(parent1.original_indexes[j]);
                        current2.Add(parent2.original_indexes[j]);
                    }

                    for (int j = index_col2+1; j < parent1.col; j++) 
                    {
                        check_second1.Add(parent2.original_indexes[j]);
                        check_second2.Add(parent1.original_indexes[j]);
                    }

                    for (int j = 0; j <= index_col2; j++)  
                    {
                        check_second1.Add(parent2.original_indexes[j]);
                        check_second2.Add(parent1.original_indexes[j]);
                    }

                    for (int ind = 0; ind < check_second1.Count; ind++) 
                    {
                        if (!current1.Contains(check_second1[ind]))
                        {
                            for (int r = 0; r < parent1.row; r++)
                            {
                                matrix1[r, current1.Count] = main_matrix[r, check_second1[ind]]; 
                            }
                            current1.Add(check_second1[ind]);
                        }
                        if (!current2.Contains(check_second2[ind]))
                        {
                            for (int r = 0; r < parent1.row; r++)
                            {
                                matrix2[r, current2.Count] = main_matrix[r, check_second2[ind]]; 
                            }
                            current2.Add(check_second2[ind]);
                        }
                    }
                    descendant1 = new Individual(matrix1, matrix1.GetLength(0), matrix1.GetLength(1), Fitness(matrix1), current1);  
                    descendant2 = new Individual(matrix2, matrix2.GetLength(0), matrix2.GetLength(1), Fitness(matrix2), current2);
                }
                else if (index_col2 == parent1.col - 1) 
                {
                    for (int j = index_col1; j <= index_col2; j++) 
                    {
                        current1.Add(parent1.original_indexes[j]);
                        current2.Add(parent2.original_indexes[j]);
                    }

                     
                    check_second1 = parent2.original_indexes;
                    check_second2 = parent1.original_indexes;
                    int tmp1 = 0; 
                    int tmp2 = 0;

                    for (int ind = 0; ind < check_second1.Count; ind++)  
                    {
                        if (!current1.Contains(check_second1[ind]))
                        {
                            for (int r = 0; r < parent1.row; r++)
                            {
                                matrix1[r, tmp1] = main_matrix[r, check_second1[ind]]; 
                            }
                            current1.Insert(tmp1, check_second1[ind]);
                            tmp1++;
                        }
                        if (!current2.Contains(check_second2[ind]))
                        {
                            for (int r = 0; r < parent1.row; r++)
                            {
                                matrix2[r, tmp2] = main_matrix[r, check_second2[ind]]; 
                            }
                            current2.Insert(tmp2, check_second2[ind]);
                            tmp2++;
                        }
                    }

                    descendant1 = new Individual(matrix1, matrix1.GetLength(0), matrix1.GetLength(1), Fitness(matrix1), current1);  
                    descendant2 = new Individual(matrix2, matrix2.GetLength(0), matrix2.GetLength(1), Fitness(matrix2), current2);
                }
                else 
                {
                    for (int j = index_col1; j <= index_col2; j++) 
                    {
                        current1.Add(parent1.original_indexes[j]);
                        current2.Add(parent2.original_indexes[j]);
                    }

                    for (int j = index_col2 + 1; j < parent1.col; j++) 
                    {
                        check_second1.Add(parent2.original_indexes[j]);
                        check_second2.Add(parent1.original_indexes[j]);
                    }

                    for (int j = 0; j <= index_col2; j++)  
                    {
                        check_second1.Add(parent2.original_indexes[j]);
                        check_second2.Add(parent1.original_indexes[j]);
                    }

                    int tmp1 = index_col2 + 1;
                    int tmp2 = index_col2 + 1;

                    for (int ind = 0; ind < check_second1.Count; ind++)  
                    {
                        if (!current1.Contains(check_second1[ind]))
                        {
                            for (int r = 0; r < parent1.row; r++)
                            {
                                matrix1[r, tmp1] = main_matrix[r, check_second1[ind]]; 
                            }
                            if(tmp1 > index_col2 && tmp1 < parent1.col) 
                            {
                                current1.Add(check_second1[ind]);
                                tmp1++;
                                if(tmp1 == parent1.col) 
                                {
                                    tmp1 = 0;
                                }
                            }
                            else
                            {
                                current1.Insert(tmp1, check_second1[ind]);
                                tmp1++;
                            }
                        }
                        if (!current2.Contains(check_second2[ind]))
                        {
                            for (int r = 0; r < parent1.row; r++)
                            {
                                matrix2[r, tmp2] = main_matrix[r, check_second2[ind]]; 
                            }
                            if (tmp2 > index_col2 && tmp2 < parent2.col) 
                            {
                                current2.Add(check_second2[ind]);
                                tmp2++;
                                if (tmp2 == parent2.col) 
                                {
                                    tmp2 = 0;
                                }
                            }
                            else
                            {
                                current2.Insert(tmp2, check_second2[ind]);
                                tmp2++;
                            }
                        }
                    }

                    descendant1 = new Individual(matrix1, matrix1.GetLength(0), matrix1.GetLength(1), Fitness(matrix1), current1); 
                    descendant2 = new Individual(matrix2, matrix2.GetLength(0), matrix2.GetLength(1), Fitness(matrix2), current2);
                }

                population[indexes[index_parent1]].matrix = descendant1.matrix; 
                population[indexes[index_parent1]].fitness = descendant1.fitness;
                population[indexes[index_parent1]].original_indexes = descendant1.original_indexes;
                population[indexes[index_parent2]].matrix = descendant2.matrix;
                population[indexes[index_parent2]].fitness = descendant2.fitness;
                population[indexes[index_parent2]].original_indexes = descendant2.original_indexes;

                 
                if (index_parent1 < index_parent2)
                {
                    indexes.RemoveAt(index_parent2);
                    indexes.RemoveAt(index_parent1);
                }
                else
                {
                    indexes.RemoveAt(index_parent1);
                    indexes.RemoveAt(index_parent2);
                }
            }
        }

        private void Mutation() 
        {
            double mutation_p = Convert.ToDouble(numericUpDown4.Value); // mutation probability
            int count = Convert.ToInt32(Math.Floor(population.Count * mutation_p)); 
            List<int> indexes = new List<int>(); 

            for (int i = 0; i < count; i++)
            {
                int index1 = mainForm.rand.Next(main_matrix.GetLength(1)); 
                int index2 = mainForm.rand.Next(main_matrix.GetLength(1));
                while (index1 == index2)
                {
                    index2 = mainForm.rand.Next(main_matrix.GetLength(1));
                }

                int individual_ind = mainForm.rand.Next(population.Count); 
                while (indexes.Contains(individual_ind))
                {
                    individual_ind = mainForm.rand.Next(population.Count);
                }
                indexes.Add(individual_ind);

                Individual mutated = new Individual (population[individual_ind].matrix, population[individual_ind].row, population[individual_ind].col, population[individual_ind].fitness, population[individual_ind].original_indexes);
                for (int j = 0; j < mutated.row; j++) 
                {
                    mutated.matrix[j, index1] = population[individual_ind].matrix[j, index2];
                    mutated.matrix[j, index2] = population[individual_ind].matrix[j, index1];
                }

                mutated.original_indexes[index1] = population[individual_ind].original_indexes[index2]; 
                mutated.original_indexes[index2] = population[individual_ind].original_indexes[index1];
                mutated.fitness = Fitness(mutated.matrix);

                population[individual_ind].matrix = mutated.matrix;
                population[individual_ind].fitness = mutated.fitness;
                population[individual_ind].original_indexes = mutated.original_indexes;
            }

        }

        private int CheckChanges(List<int> breaks_ones, List<int> breaks_zeros) 
        {
            List<int> changes = new List<int>();
            int size = breaks_ones.Count;
            if (breaks_ones.Count + breaks_zeros.Count == 3) 
            {
                changes.Add(breaks_ones[0]);
                changes.Add(breaks_zeros[0]);
                changes.Add(breaks_zeros[1]);
            }
            else if (breaks_ones.Count + breaks_zeros.Count == 5) 
            {
                changes.Add(breaks_ones[0] + breaks_ones[1]);
                changes.Add(breaks_ones[0] + breaks_zeros[2]);
                changes.Add(breaks_zeros[0] + breaks_ones[1]);
                changes.Add(breaks_zeros[0] + breaks_zeros[1]);
                changes.Add(breaks_zeros[1] + breaks_zeros[2]);
                changes.Add(breaks_zeros[0] + breaks_zeros[2]);
            }
            else 
            {
                int tmp = breaks_ones.Count - 2;
                int middle = 0;
                for (int i = tmp; i > 0; i--)  
                {
                    middle += i;
                }

                int end = breaks_ones.Count + breaks_zeros.Count + middle; 

                int count1 = 1;
                int count2 = size - 1;
                int ind2 = 2;
                for (int i = 0; i < size - 1; i++) 
                {
                    changes.Add(breaks_ones.GetRange(0, count1).Sum() + breaks_zeros.GetRange(ind2, count2).Sum());
                    count1++;
                    count2--;
                    ind2++;
                }
                changes.Add(breaks_ones.GetRange(0, size).Sum());
                changes.Add(breaks_zeros.GetRange(0, size).Sum());
                changes.Add(breaks_zeros.GetRange(1, size).Sum());

                count1 = 1;
                count2 = size - 1;
                ind2 = 1;
                for (int i = 0; i < size - 1; i++) 
                {
                    changes.Add(breaks_zeros.GetRange(0, count1).Sum() + breaks_ones.GetRange(ind2, count2).Sum());
                    count1++;
                    count2--;
                    ind2++;
                }

                count1 = 1; 
                count2 = size - 2; 
                ind2 = size; 
                int count3 = 1; 
                int check = size - 3; 
                for (int i = 0; i < middle; i++)
                {
                    if (count2 == 0)
                    {
                        count1++;
                        count2 = check;
                        ind2 = size;
                        count3 = 1;
                        check--;
                    }
                    changes.Add(breaks_zeros.GetRange(0, count1).Sum() + breaks_ones.GetRange(count1, count2).Sum() + breaks_zeros.GetRange(ind2, count3).Sum());
                    count2--;
                    ind2--;
                    count3++;
                }
            }

            int min = changes.Min(); 

            return min;
        }

        private int Fitness(int[,] matrix) //objective function
        {
            int changes = 0; 
            List<int[]> rows = new List<int[]>(); 

            for (int i = 0; i < matrix.GetLength(0); i++) 
            {
                int[] tmp = new int[matrix.GetLength(1)];
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    tmp[j] = matrix[i, j];
                }
                rows.Add(tmp);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                if (CheckConsecutive(rows[i]))
                {
                    continue;
                }
                else
                {
                    List<int> breaks_ones = new List<int>();
                    List<int> breaks_zeros = new List<int>(); 
                    List<int> index_ones = new List<int>();
                    List<int> index_zeros = new List<int>(); 

                    for (int j = 0; j < rows[i].Length; j++) 
                    {
                        if(rows[i][j] == 1)
                        {
                            index_ones.Add(j);
                        }
                        else
                        {
                            index_zeros.Add(j);
                        }
                    }

                    int prev = -1;
                    for (int j = 0; j < index_ones.Count; j++) 
                    {
                        if (j == 0)
                        {
                            prev = index_ones[j];
                            continue;
                        }
                        if (prev + 1 != index_ones[j])
                        {
                            breaks_ones.Add(index_ones[j] - prev - 1);
                            prev = index_ones[j];
                        }
                        else
                        {
                            prev = index_ones[j];
                            continue;
                        }
                    }
                    prev = -1;
                    for (int j = 0; j < index_zeros.Count; j++)  
                    {
                        if (j == 0)
                        {
                            prev = index_zeros[j];
                            continue;
                        }
                        if (prev + 1 != index_zeros[j])
                        {
                            breaks_zeros.Add(index_zeros[j] - prev - 1);
                            prev = index_zeros[j];
                        }
                        else
                        {
                            prev = index_zeros[j];
                            continue;
                        }
                    }

                    int tmp;

                    if (index_zeros[0] == 0 && index_zeros[index_zeros.Count - 1] == rows[i].Length - 1) 
                    {
                        tmp = CheckChanges(breaks_ones, breaks_zeros);
                    }
                    else if (index_zeros[0] == 0) 
                    {
                        breaks_zeros.Add(rows[i].Length - index_zeros[index_zeros.Count - 1] - 1); 
                        tmp = CheckChanges(breaks_ones, breaks_zeros);
                    }
                    else if (index_zeros[index_zeros.Count - 1] == rows[i].Length - 1) 
                    {
                        breaks_zeros.Insert(0, index_zeros[0]); 
                        tmp = CheckChanges(breaks_ones, breaks_zeros);
                    }
                    else 
                    {
                        breaks_zeros.Add(rows[i].Length - index_zeros[index_zeros.Count - 1] - 1); 
                        breaks_zeros.Insert(0, index_zeros[0]);
                        tmp = CheckChanges(breaks_ones, breaks_zeros);
                    }

                    changes += tmp;
                    
                }
            }

            return changes;
        }

        public void GeneticAlgorithm() //GA - main loop
        {
            if(dataGridView1.Rows.Count != 0) 
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
                label5.Text = "Objective function: ";
                label6.Text = "Time: ";
            }

            watch.Start();      

            population = new List<Individual>(); 

            PopulationInitialization();
            for (int i = 0; i < population.Count; i++) 
            {
                if (i == 0)
                {
                    best = new Individual(population[i].matrix, population[i].row, population[i].col, population[i].fitness, population[i].original_indexes);
                    gen = 0;
                    continue;
                }
                if (population[i].fitness < best.fitness)
                {
                    best.matrix = population[i].matrix;
                    best.fitness = population[i].fitness;
                    best.original_indexes = population[i].original_indexes;
                }
                else
                {
                    continue;
                }
            }

            int generations = Convert.ToInt32(numericUpDown2.Value); 

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;  
            bw.DoWork += new DoWorkEventHandler(             
            delegate (object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;

                for (int i = 0; i < generations; i++)
                {
                    iteration = i;
                    if (Active)
                    {
                        Selection();
                        Crossover();
                        Mutation();
                        
                        for (int j = 0; j < population.Count; j++)
                        {
                            if (population[j].fitness < best.fitness)
                            {
                                best.matrix = population[j].matrix;
                                best.fitness = population[j].fitness;
                                best.original_indexes = population[j].original_indexes;
                                gen = i+1; 
                            }
                            else
                            {
                                continue;
                            }
                        }
                      

                        generations = Convert.ToInt32(numericUpDown2.Value);

                        if (InvokeRequired) 
                        {
                            Invoke(new MethodInvoker(UpdateChart));
                        }
                        else
                        {
                            UpdateChart();
                        }

                        b.ReportProgress(i+1);
                        Thread.Sleep(20);
                    }
                    else
                    {
                        i--;
                        Thread.Sleep(100);
                    }
                    if (Stop)
                    {
                        Stop = false;
                        break;
                    }
                }
                watch.Stop();

            });

            
            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate (object o, ProgressChangedEventArgs args)
            {
                label7.Text = string.Format("Time: {0}",  watch.Elapsed);
            });

            
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate (object o, RunWorkerCompletedEventArgs args)
            {
                label5.Text = "Objective function: " + best.fitness.ToString();
                label6.Text = "Time: " + watch.Elapsed;
                int r = main_matrix.GetLength(0);
                int c = main_matrix.GetLength(1);
                dataGridView1.ColumnCount = c;
                for (int i = 0; i < r; i++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridView1);
                    for (int j = 0; j < c; j++)
                    {
                        row.Cells[j].Value = best.matrix[i, j];
                    }
                    dataGridView1.Rows.Add(row);
                }
                mainForm.HeadersCol(this.dataGridView1, c);
                mainForm.HeadersRow(this.dataGridView1, r);
                mainForm.Settings(this.dataGridView1);
                dataGridView1.ReadOnly = true;

                Start.Enabled = true;
                numericUpDown1.Enabled = true;
                numericUpDown5.Maximum = 1000;
                label7.Text = "End of calculations!";

            });

            bw.RunWorkerAsync();
            
        }

        private bool CheckConsecutive(int[] row) 
        {
            bool first;
            int prev;

            first = true;
            prev = -1;
            for (int i = 0; i < row.Length; i++)
            {
                 int value = row[i];
                 if (value == 1)
                 {
                     if (first) 
                     {
                         first = false;
                         prev = i;
                         continue;
                     }
                     if (i != prev + 1) 
                     {
                         return false;
                     }
                     else
                     {
                         prev = i;
                     }
                 }
            }
            return true;
        }

        private void Pause_Resume_Click(object sender, EventArgs e) 
        {
            if (PauseButton.Text == "Pause")
            {
                Active = false;
                watch.Stop();
                PauseButton.Text = "Resume";
                PauseButton.FlatAppearance.BorderColor = Color.ForestGreen;
            }
            else
            {
                Active = true;
                watch.Start();
                PauseButton.Text = "Pause";
                PauseButton.FlatAppearance.BorderColor = Color.Black;
            }

        }

        private void StopButton_Click(object sender, EventArgs e) 
        {
            Stop = true;
            watch.Stop();
            Start.Enabled = true;
            numericUpDown1.Enabled = true;
            numericUpDown5.Maximum = 1000;
        }

        private void UpdateChart()
        {
            chart.Series[0].Points.AddY(best.fitness);
            chart.Update();
        }

        private void Metaheuristics_FormClosing(object sender, FormClosingEventArgs e) 
        {
            e.Cancel = true;
        }

        private void SaveMetaheuristics_Click(object sender, EventArgs e) 
        {
            if(dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("No solution to save!");
                return;
            }
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string name = saveFileDialog1.FileName;
            List<string> tab = new List<string>();

            tab.Add(label5.Text);
            tab.Add(label6.Text);

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string text = "";
                List<string> tmp = new List<string>();
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    tmp.Add(dataGridView1.Rows[row.Index].Cells[col.Index].Value.ToString());
                }
                text = String.Join(" ", tmp);
                tab.Add(text);
            }
            tab.Add(numericUpDown1.Value.ToString()); 
            tab.Add(numericUpDown2.Value.ToString());
            tab.Add(numericUpDown3.Value.ToString());
            tab.Add(numericUpDown4.Value.ToString());
            tab.Add(numericUpDown5.Value.ToString());
            string a = "generation of the best solution: " + gen.ToString();
            tab.Add(a); 
            File.WriteAllLines(name, tab);
        }

        private void Start_Click(object sender, EventArgs e)
        {
            int k = Convert.ToInt32(numericUpDown5.Value); //tournament size
            int max = Convert.ToInt32(Convert.ToDouble(numericUpDown1.Value) * 0.5); //50% of population
            if (k > max)
            {
                MessageBox.Show("The size of the tournament is too large! (Max 50% of population)");
                return;
            }
            numericUpDown5.Maximum = max; //tournament size - maximum 50% of population
            groupBox2.Visible = false;
            chart.Series[0].Points.Clear();
            chart.ChartAreas[0].AxisX.Title = "Generation";
            chart.ChartAreas[0].AxisY.Title = "Objective function";
            watch.Reset(); 
            Stop = false;
            Start.Enabled = false;
            numericUpDown1.Enabled = false; 
            GeneticAlgorithm();
        }

        private void Info_Click(object sender, EventArgs e) 
        {
            if (Stop == true)
            {
                return;
            }
            groupBox2.Visible = true;
            int count = Convert.ToInt32(numericUpDown2.Value);
            progressBar1.Maximum = count;
            progressBar1.Minimum = 0;
            for (int i = iteration; i < count; i++)
            {
                if(dataGridView1.Rows.Count != 0)
                {
                    return;
                }
                if (Active)
                {
                    Thread.Sleep(20);
                    progressBar1.Value = i + 1;
                    Application.DoEvents();
                    label8.Text = "Number of generations: " + (i + 1).ToString();
                    count = Convert.ToInt32(numericUpDown2.Value);
                }
                else
                {
                    progressBar1.Value = i + 1;
                    label8.Text = "Number of generations: " + (i + 1).ToString();
                    i--;
                    Thread.Sleep(20);
                    Application.DoEvents();
                }
                if (Stop)
                {
                    return;
                }
                
            }
        }
    }
}

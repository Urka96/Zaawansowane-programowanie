using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZP
{
    class Individual 
    {
        public int[,] matrix; //chromosome
        public int row; //matrix parameters
        public int col;
        public int fitness; //objective function
        public List<int> original_indexes; //order of columns original indexes (received from input matrix)

        public Individual () { }

        public Individual(int[,] matrix, int row, int col, int fitness, List<int> original_indexes)
        {
            this.matrix = matrix.Clone() as int [,];
            this.row = row;
            this.col = col;
            this.fitness = fitness;
            this.original_indexes = new List<int>();
            foreach (int orig in original_indexes)
            {
                this.original_indexes.Add(orig);
            }
        }

    }
}

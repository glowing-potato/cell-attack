using System;
using System.Linq;

namespace server
{

    public class Chunk
    {

        public const int SIZE = 0x100;
        public const int COLOR_MASK = 0x07;
        public const int TIMER_MASK = 0xF8;

        public const int COLOR_DEAD = 0x00;
        public static readonly int[] COLORS = new int[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };

        private byte[] oldChunk = new byte[SIZE * SIZE];
        private byte[] newChunk = new byte[SIZE * SIZE];

        public Chunk()
        {

        }

        /// <summary>
        /// Simulates this chunk.
        /// </summary>
        /// <param name="n">The chunk to the north of this one</param>
        /// <param name="ne">The chunk to the northeast of this one</param>
        /// <param name="e">The chunk to the east of this one</param>
        /// <param name="se">The chunk to the southeast of this one</param>
        /// <param name="s">The chunk to the south of this one</param>
        /// <param name="sw">The chunk to the southwest of this one</param>
        /// <param name="w">The chunk to the west of this one</param>
        /// <param name="nw">The chunk to the northwest of this one</param>
        /// <returns>If the simulator needs to keep simulating this chunk</returns>
        public bool Simulate(Chunk n, Chunk ne, Chunk e, Chunk se, Chunk s, Chunk sw, Chunk w, Chunk nw)
        {
            bool result = false;

            // simulate main area
            for (int y = 1; y < SIZE - 1; ++y)
            {
                for (int x = 1; x < SIZE - 1; ++x)
                {
                    // get cell
                    int cell = GetCellFromLocalCoords(x, y, oldChunk);

                    // check if the cell is dead
                    if ((cell & TIMER_MASK) == (0xFF & TIMER_MASK))
                    {
                        SetCellFromLocalCoords(x, y, newChunk, 0);
                        continue;
                    }

                    // figure out color counts of neigbors
                    int[] colorCounts = GetNeighborColorCounts(x, y, oldChunk);

                    int total = colorCounts.Sum();
                    int maxIndex = IndexOfMax(colorCounts);

                    bool alive = (cell & COLOR_MASK) != 0 && (cell & TIMER_MASK) == 0;

                    // figure out what to do with this cell
                    if (alive) {
                        // cell is already alive, update it
                        result = true;
                        if (total < 3 || total > 4)
                        {
                           // less than 3 neighbors or more than 4 neighbors, cell dies
                           SetCellFromLocalCoords(x, y, newChunk, cell | 0x08);
                        }
                        else if (total == 3 || total == 4)
                        {
                            // 3 or 4 neighbors, cell lives
                            SetCellFromLocalCoords(x, y, newChunk, cell);
                        }
                    }
                    else
                    {
                        // cell is dead, see if we need to create one
                        if (total == 3)
                        {
                            // exactly 3 neighbors, create the cell
                            SetCellFromLocalCoords(x, y, newChunk, COLORS[maxIndex]);
                        }
                    }

                    // increment timer
                    if ((cell & TIMER_MASK) != 0)
                    {
                        cell++;
                    }


                }
            }

            // swap cell grids
            byte[] temp = oldChunk;
            oldChunk = newChunk;
            newChunk = temp;

            // return the state of the chunk
            return result;
        }

        public int IndexOfMax(int[] array)
        {
            int maxIndex = 0;
            for (int i = 0; i < array.Length; ++i)
            {
                if (array[i] > array[maxIndex])
                {
                    maxIndex = i;
                }
            }
            return maxIndex;
        }

        public byte[] GetOldBackingArray()
        {
            return oldChunk;
        }

        public byte[] GetNewBackingArray()
        {
            return newChunk;
        }

        public int[] GetNeighborColorCounts(int x, int y, byte[] array)
        {
            int[] neighbors = new int[] {  GetCellFromLocalCoords(x + 1, y, array),
                GetCellFromLocalCoords(x + 1, y - 1, array),
                GetCellFromLocalCoords(x, y - 1, array),
                GetCellFromLocalCoords(x - 1, y - 1, array),
                GetCellFromLocalCoords(x - 1, y, array),
                GetCellFromLocalCoords(x - 1, y + 1, array),
                GetCellFromLocalCoords(x, y + 1, array),
                GetCellFromLocalCoords(x + 1, y + 1, array) };

            int[] result = new int[7];
            foreach (IGrouping<int, int> g in neighbors.GroupBy((b) => b & COLOR_MASK))
            {
                if (g.Key > 0)
                {
                    result[g.Key - 1] = g.Count();
                }
            }

            return result;//.OrderBy((e) => e.Item1).Select((e) => e.Item2).ToArray();

        }

        public int GetCellCountFromCoordsMatching(byte x, byte y, int[] neighbors, int mask)
        {
            int total = 0;
            for (int i = 0; i < neighbors.Length; ++i)
            {
                total += (neighbors[i] & mask) == mask ? 1 : 0;
            }
            return total;
        }

        public int GetCellFromLocalCoords(int x, int y, byte[] array)
        {
            return array[y * SIZE + x];
        }

        public void SetCellFromLocalCoords(int x, int y, byte[] array, int value)
        {
            array[y * SIZE + x] = (byte) value;
        }

        public override string ToString()
        {
            string result = "";
            for (int y = 1; y < SIZE - 1; ++y)
            {
                for (int x = 1; x < SIZE - 1; ++x)
                {
                    if ((GetCellFromLocalCoords(x, y, oldChunk) & TIMER_MASK) == 0 && ((GetCellFromLocalCoords(x, y, oldChunk) & COLOR_MASK) != 0))
                    {
                        result += "*";
                    }
                    else
                    {
                        result += " ";
                    }
                }
                result += "\n";
            }
            return result;
        }

    }
}

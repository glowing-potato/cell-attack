using System;
using System.Linq;

namespace GlowingPotato.CellAttack.Server.World
{

    public class Chunk
    {

        public const int SIZE = 64;
        public const int COLOR_MASK = 0x07;
        public const int TIMER_MASK = 0xF8;
        public const int DEFAULT_CELL = 0xF8;

        public static readonly int[] COLORS = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };

        private byte[] oldChunk = new byte[SIZE * SIZE];
        private byte[] newChunk = new byte[SIZE * SIZE];

        public Chunk()
        {
            Array.Fill(oldChunk, (byte)TIMER_MASK);
            Array.Fill(newChunk, (byte)TIMER_MASK);
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
        /// <param name="worldInterface">The interface allowing access to the world's chunk map</param>
        /// <returns>If the simulator needs to keep simulating this chunk</returns>
        public bool Simulate(Chunk n, Chunk ne, Chunk e, Chunk se, Chunk s, Chunk sw, Chunk w, Chunk nw)
        {
            bool result = false;

            // simulate main area
            for (int y = 0; y < SIZE; ++y)
            {
                for (int x = 0; x < SIZE; ++x)
                {
                    // get cell
                    int cell = GetCellFromLocalCoordsOld(x, y, n, ne, e, se, s, sw, w, nw);

                    // figure out color counts of neigbors
                    int[] colorCounts = GetNeighborColorCounts(x, y, n, ne, e, se, s, sw, w, nw);

                    int total = colorCounts.Sum();
                    int maxIndex = IndexOfMax(colorCounts);

                    bool alive = (cell & TIMER_MASK) == 0;

                    // figure out what to do with this cell
                    if (alive)
                    {
                        // cell is already alive, update it
                        result = true;
                        if (total < 2 || total > 3)
                        {
                            // less than 3 neighbors or more than 4 neighbors, cell dies
                            SetCellFromLocalCoords(x, y, newChunk, cell | 0x08);
                        }
                        else if (total == 2 || total == 3)
                        {
                            // 3 or 4 neighbors, cell lives
                            SetCellFromLocalCoords(x, y, newChunk, cell & COLOR_MASK);
                        }
                    }
                    else
                    {
                        // cell is dead, see if we need to create one
                        if (total == 3)
                        {
                            // exactly 3 neighbors, create the cell
                            SetCellFromLocalCoords(x, y, newChunk, COLORS[maxIndex]);
                            alive = true;
                            result = true;
                        }
                    }

                    // increment timer
                    if (!alive && (cell & TIMER_MASK) != TIMER_MASK)
                    {
                        SetCellFromLocalCoords(x, y, newChunk, cell + 0x08);
                    }
                }
            }

            // return the state of the chunk
            return result;
        }

        public void SwapBuffers()
        {
            // swap cell grids
            byte[] temp = oldChunk;
            oldChunk = newChunk;
            newChunk = temp;
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

        public void CheckEdges(Chunk n, Chunk ne, Chunk e, Chunk se, Chunk s, Chunk sw, Chunk w, Chunk nw, out bool cn, out bool cne, out bool ce, out bool cse, out bool cs, out bool csw, out bool cw, out bool cnw)
        {
            cn = false;
            cne = false;
            ce = false;
            cse = false;
            cs = false;
            csw = false;
            cw = false;
            cnw = false;

            int count1 = 0;
            int count2 = 0;

            // check north and south edge
            for (int i = -1; i <= SIZE; i++)
            {
                int cell1 = GetCellFromLocalCoordsOld(i, 0, n, ne, e, se, s, sw, w, nw);
                if ((cell1 & TIMER_MASK) == 0)
                {
                    count1++;
                } else
                {
                    count1 = 0;
                }
                if (count1 == 3)
                {
                    cn = true;
                }

                int cell2 = GetCellFromLocalCoordsOld(i, SIZE - 1, n, ne, e, se, s, sw, w, nw);
                if ((cell2 & TIMER_MASK) == 0)
                {
                    count2++;
                }
                else
                {
                    count2 = 0;
                }
                if (count2 == 3)
                {
                    cs = true;
                }
            }
            count1 = 0;
            count2 = 0;

            // check east and west edge
            for (int i = -1; i <= SIZE; i++)
            {
                int cell1 = GetCellFromLocalCoordsOld(0, i, n, ne, e, se, s, sw, w, nw);
                if ((cell1 & TIMER_MASK) == 0)
                {
                    count1++;
                }
                else
                {
                    count1 = 0;
                }
                if (count1 == 3)
                {
                    cw = true;
                }

                int cell2 = GetCellFromLocalCoordsOld(SIZE - 1, i, n, ne, e, se, s, sw, w, nw);
                if ((cell2 & TIMER_MASK) == 0)
                {
                    count2++;
                }
                else
                {
                    count2 = 0;
                }
                if (count2 == 3)
                {
                    ce = true;
                }
            }

            // check corners
            int cell3 = GetCellFromLocalCoordsOld(-1, 0, n, ne, e, se, s, sw, w, nw);
            int cell4 = GetCellFromLocalCoordsOld(0, 0, n, ne, e, se, s, sw, w, nw);
            int cell5 = GetCellFromLocalCoordsOld(0, -1, n, ne, e, se, s, sw, w, nw);
            if ((cell3 & TIMER_MASK) == 0 && (cell4 & TIMER_MASK) == 0 && (cell5 & TIMER_MASK) == 0)
            {
                cnw = true;
            }
            cell3 = GetCellFromLocalCoordsOld(SIZE - 1, -1, n, ne, e, se, s, sw, w, nw);
            cell4 = GetCellFromLocalCoordsOld(SIZE - 1, 0, n, ne, e, se, s, sw, w, nw);
            cell5 = GetCellFromLocalCoordsOld(SIZE, 0, n, ne, e, se, s, sw, w, nw);
            if ((cell3 & TIMER_MASK) == 0 && (cell4 & TIMER_MASK) == 0 && (cell5 & TIMER_MASK) == 0)
            {
                cne = true;
            }
            cell3 = GetCellFromLocalCoordsOld(SIZE, SIZE - 1, n, ne, e, se, s, sw, w, nw);
            cell4 = GetCellFromLocalCoordsOld(SIZE - 1, SIZE - 1, n, ne, e, se, s, sw, w, nw);
            cell5 = GetCellFromLocalCoordsOld(SIZE - 1, SIZE, n, ne, e, se, s, sw, w, nw);
            if ((cell3 & TIMER_MASK) == 0 && (cell4 & TIMER_MASK) == 0 && (cell5 & TIMER_MASK) == 0)
            {
                cse = true;
            }
            cell3 = GetCellFromLocalCoordsOld(0, SIZE, n, ne, e, se, s, sw, w, nw);
            cell4 = GetCellFromLocalCoordsOld(0, SIZE - 1, n, ne, e, se, s, sw, w, nw);
            cell5 = GetCellFromLocalCoordsOld(-1, SIZE - 1, n, ne, e, se, s, sw, w, nw);
            if ((cell3 & TIMER_MASK) == 0 && (cell4 & TIMER_MASK) == 0 && (cell5 & TIMER_MASK) == 0)
            {
                csw = true;
            }

        }

        public void LoadThing(byte[] thing, int left, int top, int width, int height, byte defaultCell)
        {
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    SetCellFromLocalCoords(x + left, y + top, oldChunk, thing[y * width + x] != 0 ? defaultCell : TIMER_MASK);
                }
            }
        }

        public byte[] GetOldBackingArray()
        {
            return oldChunk;
        }

        public byte[] GetNewBackingArray()
        {
            return newChunk;
        }

        public int[] GetNeighborColorCounts(int x, int y, Chunk n, Chunk ne, Chunk e, Chunk se, Chunk s, Chunk sw, Chunk w, Chunk nw)
        {
            int[] neighbors = new int[] {
                GetCellFromLocalCoordsOld(x + 1, y, n, ne, e, se, s, sw, w, nw),
                GetCellFromLocalCoordsOld(x + 1, y - 1, n, ne, e, se, s, sw, w, nw),
                GetCellFromLocalCoordsOld(x, y - 1, n, ne, e, se, s, sw, w, nw),
                GetCellFromLocalCoordsOld(x - 1, y - 1, n, ne, e, se, s, sw, w, nw),
                GetCellFromLocalCoordsOld(x - 1, y, n, ne, e, se, s, sw, w, nw),
                GetCellFromLocalCoordsOld(x - 1, y + 1, n, ne, e, se, s, sw, w, nw),
                GetCellFromLocalCoordsOld(x, y + 1, n, ne, e, se, s, sw, w, nw),
                GetCellFromLocalCoordsOld(x + 1, y + 1, n, ne, e, se, s, sw, w, nw)
            };

            int[] result = new int[8];
            foreach (IGrouping<int, int> g in neighbors.Where((a) => (a & TIMER_MASK) == 0).GroupBy((b) => b & COLOR_MASK))
            {
                    result[g.Key] = g.Count();
                
            }

            return result;

        }

        public int GetCellFromLocalCoordsOld(int x, int y, Chunk n, Chunk ne, Chunk e, Chunk se, Chunk s, Chunk sw, Chunk w, Chunk nw)
        {
            if (y >= 0 && y < SIZE)
            {
                if (x >= 0 && x < SIZE)
                {
                    return oldChunk[y * SIZE + x];
                }
                else if (x < 0)
                {
                    return w == null ? DEFAULT_CELL : w.oldChunk[y * SIZE + (x + SIZE)];
                }
                else if (x >= SIZE)
                {
                    return e == null ? DEFAULT_CELL : e.oldChunk[y * SIZE + (x - SIZE)];
                }
            } else if (y < 0)
            {
                if (x >= 0 && x < SIZE)
                {
                    return n == null ? DEFAULT_CELL : n.oldChunk[(y + SIZE) * SIZE + x];
                }
                else if (x < 0)
                {
                    return nw == null ? DEFAULT_CELL : nw.oldChunk[(y + SIZE) * SIZE + (x + SIZE)];
                }
                else if (x >= SIZE)
                {
                    return ne == null ? DEFAULT_CELL : ne.oldChunk[(y + SIZE) * SIZE + (x - SIZE)];
                }
            } else if (y >= SIZE)
            {
                if (x >= 0 && x < SIZE)
                {
                    return s == null ? DEFAULT_CELL : s.oldChunk[(y - SIZE) * SIZE + x];
                }
                else if (x < 0)
                {
                    return sw == null ? DEFAULT_CELL : sw.oldChunk[(y - SIZE) * SIZE + (x + SIZE)];
                }
                else if (x >= SIZE)
                {
                    return se == null ? DEFAULT_CELL : se.oldChunk[(y - SIZE) * SIZE + (x - SIZE)];
                }
            }
            return 0;
            
        }

        public int GetCellFromLocalCoords(int x, int y, byte[] array)
        {

            if (x >= 0 && x < SIZE && y >= 0 && y < SIZE)
            {
                return array[y * SIZE + x];
            }
            return -1;
        }

        public void SetCellFromLocalCoords(int x, int y, byte[] array, int value)
        {
            array[y * SIZE + x] = (byte)value;
        }
    }
}

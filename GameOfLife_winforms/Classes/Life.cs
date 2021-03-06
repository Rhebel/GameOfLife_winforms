﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife_winforms.Classes
{
    class Life
    {
        #region Properties
        public int Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        public int Rows
        {
            get { return _rows; }
            set { _rows = value; }
        }

        public int Cells
        {
            get { return _cells; }
            set { _cells = value; }
        }

        public int LiveCellCount
        {
            get { return _liveCellCount; }
            set { _liveCellCount = value; }
        }

        public bool[] CurrentStates
        {
            get { return _currentStates; }
            set { _currentStates = value; }
        }

        public bool[] NewStates
        {
            get { return _newStates; }
            set { _newStates = value; }
        }

        public bool[] StartStates
        {
            get { return _startStates; }
            set { _startStates = value; }
        }

        public List<string> LastStates
        {
            get { return _lastStates; }
            set { _lastStates = value; }
        }

        public bool ShortStop { get; set; }

        //instead of optional rules just use static ones
        public static readonly List<int> SurviveRules = new List<int>() { 2, 3 }; //const doesn't work here
        public const int BirthRules = 3; //initially was a list

        #endregion

        #region Members
        private int _columns;
        private int _rows;
        private int _cells;
        private int _liveCellCount;
        private bool[] _currentStates;
        private bool[] _newStates;
        private bool[] _startStates;

        private List<string> _lastStates = new List<string>();

        #endregion

        #region Constructors        
        //grid sizes initializer        
        public Life() { }
        //set the columns and rows to the same number
        public Life(int xy)
        {
            //_rows = xy; original code used a lot of the private members instead of properties...why??
            Rows = xy;
            Columns = xy;
            Cells = Rows * Columns;

            ClearGrid();
            StartStates = new bool[Cells];
        }

        //set the columns and rows individually
        public Life(int x, int y)
        {
            Rows = x;
            Columns = y;
        }
        #endregion

        #region Methods

        public void ClearGrid()
        {
            LiveCellCount = 0;
            CurrentStates = new bool[Cells];
            NewStates = new bool[Cells];
        }

        public void RandomFill(double percent)
        {
            Random rnd = new Random();
            LiveCellCount = 0;
            for (int i = 0; i < Cells; i++)
            {
                CurrentStates[i] = rnd.NextDouble() <= percent; //set the current value to true/false if the randome is less than the percentage passed (percent is 1/100 to 100/100
                if (CurrentStates[i])
                {
                    LiveCellCount++;
                }
            }
        }

        //sets the grid to a previous state
        public void ResetGrid()
        {
            StartStates.CopyTo(CurrentStates, 0);
        }

        //save state that reset will revert to
        public void SaveGrid()
        {
            CurrentStates.CopyTo(StartStates, 0);
        }

        public void Step()
        {
            ShortStop = false;

            AdvancePopulation();
            NewStates.CopyTo(CurrentStates, 0);

            string curr = GetStateString(CurrentStates);

            if (LastStates.Count == 10)
            {
                if (LastStates.Contains(curr))
                {
                    ShortStop = true;
                }

                LastStates.RemoveAt(0);
                LastStates.Add(curr);
            }
            else
            {
                LastStates.Add(curr);
            }
        }

        public bool GetCellState(int x, int y)
        {
            //the value is outside of the available coordinates
            if (y < 0 || y >= Rows || x < 0 || x >= Columns)
            {
                return false;
            }

            return CurrentStates[x + (y * Columns)]; //return the cell's value, this is one of the things I had trouble getting my head around. Getting the correct coordinate value from the value list
        }

        public void ToggleCellState(int x, int y)
        {
            if (!(y < 0 || y >= Rows || x < 0 || x >= Columns))
            {
                //only do something if it's inside the grid
                int index = x + (y * Columns);
                CurrentStates[index] = !(CurrentStates[index]); //set to the opposite of the currrent value at that index

                //depending on if it was flipped update the count accordingly.
                if (CurrentStates[index])
                {
                    LiveCellCount++;
                }
                else
                {
                    LiveCellCount--;
                }
            }
        }

        public void AddGlider(int x, int y, string direction, int width, int height)
        {
            //TODO add glider!
            //direction will be the direction to point it. the x/y will be the starting point on the back (the part that 'pushes')
            //might have issues with out of boundsness so near the edges might be iffy. restrict to 10 pixels of border to account for that
            //width/height are part of grid and not the same as rows/columns (potentially)
            if (x > 10 && y > 10 && x < width - 10 && y < height - 10)
            {
                //ready to add!
                //start with the current pixel
                int index = x + (y * Columns);

                if (!CurrentStates[index])
                {
                    CurrentStates[index] = true;
                    LiveCellCount++;
                }

                //add the rest of the glider. 
                //glider (SE) looks like
                /*
                     *..
                     .**
                     **.    
                     
                x, y+2; y+2, y+1
                x+1, y+1; x+1, y+2                
                */

                int n1 = 0, n2 = 0, n3 = 0, n4 = 0;
                switch (direction)
                {
                    case "nw":
                        n1 = (x) + ((y - 2) * Columns);
                        n2 = (x - 1) + ((y - 1) * Columns);
                        n3 = (x - 2) + ((y - 1) * Columns);
                        n4 = (x - 1) + ((y - 2) * Columns);
                        break;
                    case "se":
                        n1 = (x) + ((y + 2) * Columns);
                        n2 = (x + 1) + ((y + 1) * Columns);
                        n3 = (x + 2) + ((y + 1) * Columns);
                        n4 = (x + 1) + ((y + 2) * Columns);
                        break;
                    case "ne":
                        n1 = (x + 2) + ((y) * Columns);
                        n2 = (x + 1) + ((y - 1) * Columns);
                        n3 = (x + 2) + ((y - 1) * Columns);
                        n4 = (x + 1) + ((y - 2) * Columns);
                        break;
                    case "sw":
                        n1 = (x - 2) + ((y) * Columns);
                        n2 = (x - 1) + ((y + 1) * Columns);
                        n3 = (x - 2) + ((y + 1) * Columns);
                        n4 = (x - 1) + ((y + 2) * Columns);
                        break;                   
                    default:
                        break;
                }



                if (!CurrentStates[n1])
                {
                    CurrentStates[n1] = true;
                    LiveCellCount++;
                }

                if (!CurrentStates[n2])
                {
                    CurrentStates[n2] = true;
                    LiveCellCount++;
                }

                if (!CurrentStates[n3])
                {
                    CurrentStates[n3] = true;
                    LiveCellCount++;
                }

                if (!CurrentStates[n4])
                {
                    CurrentStates[n4] = true;
                    LiveCellCount++;
                }

            }
        }

        //dead = . , alive = * 
        //the original notes say this converts the grid to a string representation. 
        //supposed to be outputting the booleans as a string representation, not sure why it didn't work on my *States.ToString() calls.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    sb.Append(CurrentStates[x + (y * Columns)] ? "*" : ".");
                }
            }

            return sb.ToString();
        }

        //since the override wasn't working for me make a new call that I can use explicitly
        public string GetStateString(bool[] values)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    sb.Append(values[x + (y * Columns)] ? "*" : ".");
                }
            }

            return sb.ToString();
        }

        private void AdvancePopulation()
        {
            //only do stuff if there was cells
            if (LiveCellCount > 1)
            {
                LiveCellCount = 0;

                int contacts; //number of cells surrounding the current
                int index;
                bool alive;

                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++)
                    {
                        contacts = GetContacts(x, y);
                        index = x + (y * Columns);
                        alive = CurrentStates[index];

                        if ((alive && SurviveRules.Contains(contacts)) || (!alive && BirthRules == contacts))
                        {
                            NewStates[index] = true;
                            LiveCellCount++;
                        }
                        else
                        {
                            NewStates[index] = false;
                        }
                    }
                }
            }
        }

        //this was always the function that I could never wrap my head around
        private int GetContacts(int x, int y)
        {
            //had the groupings all wrong. fixed and made it a bit clearer
            int ContactCount = 0;

            //have to test each because it could have up to 8 neighbors. probably could find a way to shortcut the number of checks if it's already over the number of birth rules
            //top left
            if ((x > 0 && y > 0) && CurrentStates[(x - 1) + ((y - 1) * Columns)])
            {
                ContactCount++;
            }
            //top
            if (y > 0 && CurrentStates[x + ((y - 1) * Columns)])
            {
                ContactCount++;
            }
            //top right
            if (y > 0 && x + 1 < Columns && CurrentStates[(x + 1) + ((y - 1) * Columns)])
            {
                ContactCount++;
            }
            //left
            if (x + 1 < Columns && CurrentStates[(x + 1) + (y * Columns)])
            {
                ContactCount++;
            }
            //bottom left
            if (x + 1 < Columns && y + 1 < Rows && CurrentStates[(x + 1) + ((y + 1) * Columns)])
            {
                ContactCount++;
            }
            //bottom
            if (y + 1 < Rows && CurrentStates[x + ((y + 1) * Columns)])
            {
                ContactCount++;
            }
            //bottom right
            if (x > 0 && y + 1 < Rows && CurrentStates[(x - 1) + ((y + 1) * Columns)])
            {
                ContactCount++;
            }
            //right
            if ((x > 0) && CurrentStates[(x - 1) + (y * Columns)])
            {
                ContactCount++;
            }

            return ContactCount++;

        }

        #endregion

    }
}

using System;

namespace Lab2_MultiStack
{
    using static System.Console;
    internal class MultiStackMain
    {
        private enum Data //used for displaying the names properly with memArray since it's an int array
        {
            Joe = 1, Bob, Devin,
            Elwin, Sagar, Ryan,
            Kyle, Tyler, Minh,
            Adrian, Daniel, Jailene,
            Jacob, Bryce, Jaylene,
            Ayran, Travis, Rahul,
            Victor, Corey, Bipin,
            Frank, Nathan, Amber,
            Amy
        }

        private static void Main(string[] args)
        {
            //OptionC();
            OptionB();
        }

        #region Options

        //B-Option main method
        private static void OptionB()
        {
            WriteLine(Banners("Option B"));

            int lower, upper = 0;
            (lower, upper) = GetBounds();
            int size = MainMemSize(lower, upper);
            Span<int> memArray = stackalloc int[size]; //array from 0-81 (hhave to do offset due to 0 based arrays)
                                                       //stack.Slice(11, 34).ToArray();
            for (int i = 0; i < memArray.Length; i++)
            {
                memArray[i] = 0;
            }
            //memArray.Fill(0) does the same as the for loop above

            //number of stacks in the memory array
            WriteLine("Enter number of stacks: ");
            int numStacks = int.Parse(ReadLine());

            /* The following article online talkes all about Span<T> and its functionality
             *      -- https://codingblast.com/span-of-t/ -- (there are other articles as well)
             * Span<T> is a struct and it will NOT cause heap allocation.
             * Span<T> provides type-safe access to memory while maintaining the performance of arrays.
             * 
             * --This is for istructor as he had specific requirements that needed to be met
             * and this shows that I have met them with the constraints given
             */

            //Create each stack that will hold the top, base and oldtop indexes
            Span<int> TOP = stackalloc int[numStacks];
            Span<int> BASE = stackalloc int[numStacks + 1];
            Span<int> OTGNB = stackalloc int[numStacks + 1]; //oldtop, growth, newbase (single array)

            //Get L0 and M from user
            WriteLine("Enter starting mem location (L0): ");
            var L0 = int.Parse(ReadLine());
            Console.WriteLine("Enter maximum mem location (M): ");
            var M = int.Parse(ReadLine());

            //create useable memory array
            int startLoc = L0 - lower;
            Span<int> memArrayUsable = memArray.Slice(startLoc, M + 1); //0 - 33 of the array (considerind offest

            InitStacks(L0, M, numStacks, TOP);     //fill TOP    with initial index locations
            InitStacks(L0, M, numStacks, BASE);    //fill BASE   with initial index locations (same as top plus index at M for extra base)
            InitStacks(L0, M, numStacks, OTGNB);

            WriteLine("+++++    BASE   +++++");
            DisplayArray(BASE);
            WriteLine("+++++    TOP    +++++");
            DisplayArray(TOP);

            ProcessTransactions(memArrayUsable, TOP, BASE, OTGNB, L0, M, numStacks);

            WriteLine("***** memArrayUsable *****");
            DisplayArray(memArrayUsable, BASE, TOP);

            TOP.Clear();
            BASE.Clear();
            OTGNB.Clear();
        }

        #endregion Options

        #region Util Functions

        //Banner to identify and separte each option
        private static string Banners(string name)
        {
            string banner = "======================================================" +
                            $"\n============           {name}          =============" +
                            "\n======================================================\n";
            return banner;
        }

        private static (int lower, int upper) GetBounds()
        {
            string input;
            WriteLine("Enter stack lower bounds:");
            input = ReadLine();
            int lower = int.Parse(input);

            WriteLine("Enter stack upper bounds:");
            input = ReadLine();
            int upper = int.Parse(input);
            return (lower, upper);
        }

        private static int MainMemSize(int lower, int upper)
        {
            int size;
            //Internal function... this is the only time its used.
            int StackSize(int l, int u) => Math.Abs(l) + Math.Abs(u) + 1; //adding +1 to account for 0 to get total size
                                                                          //options b and c want -11 to 60 for bounds;
                                                                          //but need to account for 0 in the range for the max size of the array
            return size = StackSize(lower, upper);
        }

        //Reads in all insert and delete transactions
        private static void ProcessTransactions(Span<int> memArray, Span<int> t, Span<int> b, Span<int> otgnb, int l, int m, int n)
        {
            bool go = true;
            while (go)
            {
                string input = ReadLine();
                ReadOnlySpan<char> slicedInput = input.AsSpan().Slice(1, 1); //grabs the stack number

                string info;
                int element;

                //if no more input end while loop
                if (input == "****")
                {
                    go = false;
                    break;
                }

                int k = int.Parse(slicedInput) - 1; //this is the stack number to insert into
                
                //Push data to stack (insert)
                if (input.StartsWith("I")) //check for insert command
                {
                    ReadOnlySpan<char> infoData = input.AsSpan().Slice(3);
                    info = infoData.ToString(); //item to insert...name of person in our case
                    element = CheckInfo(info); //enum element index

                    ProcessInsert(memArray, t, b, otgnb, k, element, n, info, m, l);
                }

                if (input.StartsWith("D")) //check for delete command
                {
                    element = memArray[t[k]];
                    ProcessDelete(memArray, t, b, k, element);
                }
            }
        }

        private static void ProcessDelete(Span<int> memArray, Span<int> t, Span<int> b, int k, int element)
        {
            if (t[k] == b[k])
            {
                WriteLine("**:: Underflow ::**");
            }
            else
            {
                DeleteElement(memArray, t, k, element);
            }
        }

        private static void DeleteElement(Span<int> memArray, Span<int> t, int k, int element)
        {
            memArray[t[k]] = 0;
            WriteLine($"POP  {CheckInfo(element),-7} from: stack({k + 1}) @ location[{t[k]:00}]");
            t[k] = t[k] - 1;
        }

        private static void ProcessInsert(Span<int> memArray, Span<int> t, Span<int> b, Span<int> otgnb, int k, int element, int n, string info, int m, int l)
        {
            t[k] = t[k] + 1;
            if (t[k] > b[k + 1])
            {
                WriteLine($"PUSH {info,-7} to:   stack({k + 1}) @ location[{t[k]:00}]");

                WriteLine("\n** !!Overflow!! **");
                WriteLine("v============== BEFORE REALLOC ==============v");
                WriteLine("+++++    BASE   +++++");
                DisplayArray(b);
                WriteLine("+++++    TOP    +++++");
                DisplayArray(t);
                WriteLine("+++++   OLDTOP  +++++");
                DisplayArray(otgnb);
                WriteLine("+++++  USEDMEM  +++++");
                DisplayArray(memArray, b, t);
                WriteLine("^===========================================^\n");

                //Call Reallocate function here
                Reallocate(b, t, otgnb, n, k, memArray, element, info, m, l);
            }
            else
            {
                WriteLine($"PUSH {info,-7} to:   stack({k + 1}) @ location[{t[k]:00}]");
                InsertToStack(memArray, t, k, element);
            }
        }

        private static void InsertToStack(Span<int> memArray, Span<int> t, int k, int element)
        {
            memArray[t[k]] = element;
        }

        private static void DisplayArray(Span<int> arr) //used for top, base, etc.
        {
            string output = "";
            int element = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                output = $"items in array[{i}]: {element = arr[i]}";
                WriteLine(output);
            }
            WriteLine();
        }

        private static void DisplayArray(Span<int> arr, Span<int> b, Span<int> t) //this is used to display the main array
        {
            string info, bases, tops, output = "";
            string stacks = null;
            for (int i = 0; i < arr.Length; i++)
            {
                bases = ""; tops = "";
                info = CheckInfo(arr[i]).ToString();
                output = $"MainMemory[{i:00}]: {info}";

                foreach (var element in b) //base locations
                {
                    int index = b.IndexOf(element);
                    if (i == 0 && index == 0)
                    {
                        bases = $"Base({index + 1})";
                        break;
                    }
                    if (i == element)
                    {
                        bases = $"Base({index + 1})";
                        break;
                    }
                    else
                    {
                        bases = null;
                    }
                }

                foreach (var element in t) //top locations
                {
                    int index = t.IndexOf(element);
                    if (i == element)
                    {
                        tops = $"Top({index + 1})";
                        break;
                    }
                    else
                    {
                        tops = null;
                    }
                }

                string topStacks = tops;
                string baseStacks = bases;
                if (baseStacks == null && topStacks == null) //empty if not either location
                {
                    stacks = "";
                }
                if (baseStacks != null && topStacks == null) //show base location
                {
                    stacks = $"{baseStacks}";
                }
                if (baseStacks == null && topStacks != null) //show top location...only the one that moved into that space (growing stack)
                {
                    stacks = $"{topStacks}";
                }
                if (baseStacks != null && topStacks != null) //show both
                {
                    stacks = $"{baseStacks} {topStacks}";
                }

                WriteLine($"{output,-30}{stacks,-6}");
            }
        }

        private static string CheckInfo(int info) //returns name of the enum value passed to it
        {
            string name = "";
            if (info == 0)
            {
                return name;
            }
            else
            {
                foreach (var item in Enum.GetValues(typeof(Data)))
                {
                    if (info == (int)Enum.Parse(typeof(Data), item.ToString()))
                    {
                        name = Enum.GetName(typeof(Data), info);
                        break;
                    }
                }
            }
            return name;
        }

        private static int CheckInfo(string info) //returns the value of the enum name passed to it
        {
            int value = 0;
            string str = "";
            foreach (var item in Enum.GetNames(typeof(Data)))
            {
                str = item.ToString();
                if (info == str)
                {
                    value = (int)Enum.Parse(typeof(Data), item);
                    break;
                }
            }
            return value;
        }

        private static void InitStacks(int l0, int m, int numStacks, Span<int> arr)
        {
            int availSpace = (m - l0) / numStacks;
            int extraSpace = (m - l0) % numStacks;

            int i = 0;
            int j = l0;
            while (i < arr.Length)
            {
                arr[i] = j;

                i++;
                if (arr.Length > numStacks)
                {
                    arr[arr.Length - 1] = j + extraSpace; //added extra space to end
                }
                j += availSpace;
            }
        }

        #endregion Util Functions

        #region Special Lab Functions

        private static void Reallocate(Span<int> b, Span<int> t, Span<int> otgnb, int n, int k, Span<int> memArray, int element, string info, int m, int l)
        {
            int availSpace = m - l;
            int minSpace = availSpace / 20; //5% minimum space
            int totalInc = 0;
            int j = n - 1; //due to zero based indexing of arrays

            decimal equalAlloc, growthAlloc = 0.0m; //changed to decimal... might cause issues
            decimal alpha, beta, tau, sigma;

            while (j >= 0) //set growth of stacks
            {
                availSpace = availSpace - (t[j] - b[j]);
                if (t[j] > otgnb[j])
                {
                    otgnb[j + 1] = t[j] - otgnb[j]; //growth[] == otgnb[j+1]
                    totalInc += otgnb[j + 1];
                }
                else
                {
                    otgnb[j + 1] = 0;
                }
                j -= 1;
            }

            //ReA2
            if (availSpace < (minSpace - 1))
            {
                WriteLine($"Available Space: {availSpace} Minspace: {minSpace}");
                WriteLine("Not enough memory. Program terminating");
                WriteLine("\n** Final Results **");
                DisplayArray(memArray, b, t);
                Environment.Exit(0);
            }

            //ReA3
            equalAlloc = 0.2m;
            growthAlloc = 1 - equalAlloc;
            alpha = equalAlloc * ((decimal)availSpace / n);

            //ReA4
            if (totalInc == 0)
            {
                beta = growthAlloc * availSpace;
            }
            else
                beta = growthAlloc * ((decimal)availSpace / totalInc);

            //ReA5
            otgnb[0] = 0;
            sigma = 0m;
            for (j = 1; j < n; j++) //calculate new bases
            {
                tau = sigma + alpha + otgnb[j] * beta;
                otgnb[j] = otgnb[j - 1] + (t[j - 1] - b[j - 1]) + (int)Math.Floor(tau) - (int)Math.Floor(sigma);
                sigma = tau;
            }
            otgnb[n] = b[n];

            //ReA6
            t[k] = t[k] - 1;
            MoveStack(b, t, n, otgnb, memArray); //Call Move Stack
            t[k] = t[k] + 1;

            //Insert item into t[k]
            WriteLine($"++++   Insert problematic item (after moving stacks):   ++++");
            WriteLine($"PUSH {info,-7} to: S{k + 1} @ location[{t[k]:00}]");
            InsertToStack(memArray, t, k, element);

            //Loop for other overflow
            for (int jj = 0; jj < n; jj++)
            {
                otgnb[jj] = t[jj];
            }

            //Required output after reallocation and moving
            WriteLine("v============== AFTER REALLOC ==============v");
            WriteLine("+++++    BASE   +++++");
            DisplayArray(b);
            WriteLine("+++++    TOP    +++++");
            DisplayArray(t);
            WriteLine("+++++  USEDMEM  +++++");
            DisplayArray(memArray, b, t);
            WriteLine("^===========================================^\n");
        }

        private static void MoveStack(Span<int> b, Span<int> t, int n, Span<int> nb, Span<int> memArray)
        {
            //MoA1 ... move down
            for (int j = 1; j < n; j++)
            {
                if (nb[j] < b[j]) //this will skip base 5 at index of j = 4
                {
                    var delta = b[j] - nb[j];
                    for (int l = b[j] + 1; l <= t[j]; l++)
                    {
                        memArray[l - delta] = memArray[l];
                        memArray[l] = 0; //replace previous location with empty info (cleans up garbage left behind from moving)
                    }//end loop
                    b[j] = nb[j];
                    t[j] = t[j] - delta;
                }//end if
            }//end loop

            //MoA2 ... move up
            for (int j = n; j >= 1; j--) //
            {
                if (nb[j] > b[j]) // this will skip base 5 at index of jj = 4
                {
                    var delta = nb[j] - b[j];
                    for (int l = t[j]; l >= b[j] + 1; l--)
                    {
                        memArray[l + delta] = memArray[l];
                        memArray[l] = 0; //replace previous location with empty info (cleans up garbage left behind from moving)
                    }//end loop
                    b[j] = nb[j];
                    t[j] = t[j] + delta;
                }//end if
            }//end loop
        }

        #endregion Special Lab Functions
    }//end class
}//end namespace
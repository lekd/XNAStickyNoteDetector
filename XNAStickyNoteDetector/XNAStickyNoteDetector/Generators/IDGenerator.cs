using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAStickyNoteDetector
{
    public class IDGenerator
    {
        static int saltValue;
        static bool isInitialized = false;
        public static void Initialize()
        {
            Random rnd = new Random();
            saltValue = rnd.Next(1, short.MaxValue / 2);
            isInitialized = true;
        }
        static int hash(int a, int b)
        {
            return ((a + b) * (a + b + 1) / 2 + b);
        }
        public static int getHashedID(int inputID)
        {
            if (!isInitialized)
            {
                Initialize();
            }
            return hash(saltValue, inputID);
        }
    }
}

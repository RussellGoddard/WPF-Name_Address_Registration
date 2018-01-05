using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGoddard_Project_4
{
    [Serializable]
    public static class IdNumber
    {
        public static int initializeIdNum { set { idNum = value; } }
        public static string getNewIdNum { get { return (idNum++).ToString(); } }
        private static int idNum = 0; //variable that holds the next open variable (should be used with post increment)
        public static List<string> availableIdNum = new List<string>(); //list to contain any freed up IdNumbers (for when addresses are deleted)
    }
}

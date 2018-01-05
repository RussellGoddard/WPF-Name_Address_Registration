using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGoddard_Project_4
{
    public static class ObservablecollectionExtension
    {
        public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
        {
            var sortableList = new List<T>(collection);
            sortableList.Sort(comparison);

            for (int i = 0; i < sortableList.Count; i++)
            {
                collection.Move(collection.IndexOf(sortableList[i]), i);
            }
        }


        public static void AddRange<T>(this ObservableCollection<T> original, ObservableCollection<T> newCollection)
        {
            if (newCollection == null)
            {
                throw new ArgumentNullException("newCollection");
            }   
            foreach (var i in newCollection)
            {
                original.Add(i);
            }
        }
    }

    public static class StringExtension
    {
        //taken from: https://stackoverflow.com/questions/16725848/how-to-split-text-into-words
        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || char.IsLetter(c) || c == '\'' || char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}

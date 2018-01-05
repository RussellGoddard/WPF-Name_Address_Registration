using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Text;
using System.Threading.Tasks;

namespace RGoddard_Project_4
{
    public static class GlobalVariables
    {
        //collection of Person
        public static ObservableCollection<Person> allPerson = new ObservableCollection<Person>();
        //collection of Person that is exposed to the WPF form
        public static ICollectionView allPersonView = CollectionViewSource.GetDefaultView(GlobalVariables.allPerson);

        public static FixedSizeObservableCollection<string> errorList = new FixedSizeObservableCollection<string>(4);

        //file path info for saving/loading stored addresses
        //the different variations on filepath creations was done solely for my own understanding
        public static string directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SavedAddresses");
        public static string path = System.IO.Path.Combine(directory, "Addresses.stf");
        public static string serializedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SavedAddresses\SerializedAddresses.stf";
    }

    public static class StringPlaceholders
    {
        //Error text
        public static string errorEnterBothNames = "First and last name must be entered";
        public static string errorNameLetters = "Name must be all letters \n";
        public static string errorZipNumeric = "ZIP must be all numeric \n";
        public static string errorZipLength = "ZIP entered was incorrect length \n";
        public static string errorZipGeneric = "ZIP entered incorrect format \n";
        public static string errorPhoneGeneric = "Phone entered incorrectly \n";
        public static string errorTextContainsDigits = "Error, name cannot contain digits";
        public static string errorTextUnableToParse = "Error, unable to parse search, too many white spaces?";
        public static string errorDuplicateLoad = "Error, saved addresses already contain all load addresses.";
        public static string errorSerialNotFound = @"Error, MyDocuments\SavedAddresses\SerializedAddresses.stf not found";
        public static string errorInvalidIndex = "Invalid index selected, please select someone in the combobox or grid";

        //placeholder text
        public static string placeholderAddress = "Street Address, P.O. Box, company name, c/o";
        public static string placeholderAddress2 = "Apartment, suite, unit, building, floor, etc (optional)";
        public static string placeholderZip = "#####";
        public static string placeholderPhone = "(###) ###-####";
        public static string placeholderSearch = "\"First Name\" \"Last Name\" lead with a space if searching by last name";
        public static string placeholderIdNum = "None";
        public static string placeholderSaveStf = "Filed saved in MyDocuments.SavedAddresses.Addresses.stf";
    }
}

using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/*
Project # 4 Requirements

Requirements: Create a form to enter and store multiple name and addresses.  
Create a list by storing entry’s in an array so multiple name and addresses can be entered, stored and then displayed on the form.  
The application should capture name and addresses entered, then redisplay on user request.

Create a form that would capture and validate a user’s :
•	Name
•	Address
•	City, State, Zip 
•	Phone Number
Validate the Name for valid characters
Validate Zip code and phone number for valid digits
Store the address in an array so a user can clear the form and enter another name and address.
In the future we will be storing this information in a file so we can retrieve after the program has shutdown.   
For now provide the functionality to record customer, student or member information as if you were maintaining a list for future use.

*/

/* 
Re-using the form from our previous project, develop a name and address Class.  When a user enters data instantiate the class to validate and reformat the output.   Within the Name and Address class include the following:
•	Members and properties
•	Default Constructor
•	Validation Methods
•	Default Sort
•	Include or Inherit an interface, for example IComparer or IDispose

As we did in the previous project, store the Name and Address class in a list of name and address classes, then display and:
	Include functionality to search on a name and present only the name found if a match is found

Add an input box (textbox) to allow the user to search on a first or last name, then display only the name being searched for or display a message if no match is found.

*/

/*
Final Project Requirements
Using the application we completed for the previous two projects, when a user enters a name and address save the information entered to a file and write one or more records out in XML format.  This process will be covered on Wednesday November 29th.

The process will be as follows:
•	Open form, read and display any records previously entered.
•	Enter data, when a save event is activated (could just add a save button) write the data to a file.  
•	Convert the output into XML and write a separate record out in XML format.

*/


/*
    Big level TO DO

    implement search DONE
    implement delete address with message box                                       DONE
    store and load addresses from text file                                         DONE
    Implement formatting in Person.name.first/last (first letter capatalized)       DONE
    Implement formatting on datagrid outputs (phone number and zip)                 DONE
    Implement userID to identify people to allow duplicate names                    DONE
        figure out how to handle duplicate ID numbers                               DONE
            on delete can say that the ID number is freely available to use 
            (ex: List<int> AvailableID) that lists any freed up ID numbers, 
            else use newID = lastID + 1
        figure out how to tell ID numbers to use between session                    DONE
        implement method of reassigning duplicate idNum when loading                DONE
            (load gets pref)
    Change load to import, let user specify file path.                              DONE
        Keep current load functionality when program starts
    Let user select name and file path on save                                      DONE
    Search by IdNum                                                                 DONE
    Search not case sensitive                                                       DONE
    Make a load by parsing strings (maybe change to JSON instead of plain strings)  TO DO
    support for resizeable window                                                   DONE (max window size is arbitrary)
    in line validation                                                              DONE (For now, not happy with colors, and text_error overflow mentioned below)
    fix text_error overflow issues                                                  DONE
        need a clean way to limit error text from covering up search data grid 
        if user continues to trigger error without fixing
    Remove highlight on mouseover from text_Error                                   TO DO
    Test new error view with error messages from newPerson                          TO DO
    Better organize code                                                            Sometimes
    Bugfix                                                                          Always
*/

namespace RGoddard_Project_4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            //create a collection view based on our allPersons container
            //bind view to combobox and datagrid
            combo_SavedAddr.ItemsSource = GlobalVariables.allPersonView;
            grid_output.ItemsSource = GlobalVariables.allPersonView;
            text_State.ItemsSource = States.los;
            text_Addr.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_Addr));
            text_Addr2.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_Addr2));
            text_ZIP.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_ZIP));
            text_PhoneNumber.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_PhoneNumber));
            text_Search.DataContext = this;
            text_Search.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_Search));
            text_IdNum.Text = StringPlaceholders.placeholderIdNum;

            text_Error.ItemsSource = GlobalVariables.errorList;

            ReadFromFile();
            GlobalVariables.errorList.Clear(); //reset error text because if read from file fails due to no file existing that is ok
        }

        #region Search Related
        //a lot of the search related functions and xaml copied from/inspired by https://marlongrech.wordpress.com/2008/11/22/icollectionview-explained/
        string searchText = String.Empty;
        /// <summary>
        /// Gets and sets the text used for searching.
        /// Please note that when this property is set a search will be executed
        /// </summary>
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                GlobalVariables.allPersonView.Filter = SearchFilter;
                OnPropertyChanged("SearchText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SearchFilter(object item)
        {
            Person person = item as Person;
            //search based on given first name, last name or both
            //look through create another observableCollection allPerson, as characters are typed into search field eliminate indexes from showing
            //tell first name from last name based on white space between them
            //if the only whitespace is a leading one, name entered is last name

            string firstNameSearch = "";
            string lastNameSearch = "";

            #region old search function
            /*
            string firstName = text_Search.Text.Trim();
            string lastName = text_Search.Text.Trim();
            bool nameFound = false; //bool to track if a name is found

            //if both are empty return error
            if (firstName == "" && lastName == "")
            {
                text_error.Text = "Search requires first or last name";
                return;
            }
            //if first name is empty search based on last name
            else if (firstName == "")
            {
                for (int i = 0; i < GlobalVariables.allPerson.Count; ++i)
                {
                    if (GlobalVariables.allPerson[i].name.lastNameAccess == lastName)
                    {
                        combo_SavedAddr.SelectedIndex = i; //changes combo_SavedAddr triggering recallAddr() function
                        nameFound = true;
                        break; //end loop
                    }
                }
            }
            //if last name is empty search based on first name
            else if (lastName == "")
            {
                for (int i = 0; i < GlobalVariables.allPerson.Count; ++i)
                {
                    if (GlobalVariables.allPerson[i].name.firstNameAccess == firstName)
                    {
                        combo_SavedAddr.SelectedIndex = i; //changes combo_SavedAddr triggering recallAddr() function
                        nameFound = true;
                        break; //end loop
                    }
                }
            }
            //else search based on both
            else
            {
                for (int i = 0; i < GlobalVariables.allPerson.Count; ++i)
                {
                    if (GlobalVariables.allPerson[i].name.lastNameAccess == lastName && GlobalVariables.allPerson[i].name.firstNameAccess == firstName)
                    {
                        combo_SavedAddr.SelectedIndex = i; //changes combo_SavedAddr triggering recallAddr() function
                        nameFound = true;
                        break; //end loop
                    }
                }
            }

            if (nameFound == false)
            {
                text_error.Text = "Name not found";
            } 
            */
            #endregion
            //check for empty SearchText
            if (SearchText == "")
            {
                return true;
            }
            //if the first character is a digit, search on idNum, else attempt to search by first/last name
            else if (Char.IsDigit(SearchText[0]))
            {
                if (person.IdNum.Contains(SearchText))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (SplitByName(ref firstNameSearch, ref lastNameSearch))
                {
                    //filter by first and last, converting names to uppercase to allow easier searching
                    if (firstNameSearch != "" && lastNameSearch != "")
                    {
                        if ((person.name.FirstName.ToUpper().Contains(firstNameSearch)) && (person.name.LastName.ToUpper().Contains(lastNameSearch)))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //filter by first only
                    else if (firstNameSearch != "")
                    {
                        return person.name.FirstName.ToUpper().Contains(firstNameSearch);
                    }
                    //filter by last only
                    else if (lastNameSearch != "")
                    {
                        return person.name.LastName.ToUpper().Contains(lastNameSearch);
                    }
                }
            }
            //reset text_error.Text
            //text_error.Text = "";
            return true;
        }

        private bool SplitByName(ref string firstNameSearch, ref string lastNameSearch)
        {
            string[] searchString;

            //test for empty string
            if (SearchText == "" || SearchText == StringPlaceholders.placeholderSearch)
            {
                return false;
            }
            //test for invalid string (contains numbers)
            for (int i = 0; i < SearchText.Length; ++i)
            {
                if (char.IsDigit(SearchText[i]))
                {
                    //text_error.Text = textContainsDigits; OLD
                    GlobalVariables.errorList.Add(StringPlaceholders.errorTextContainsDigits);
                    return false;
                }
            }
            //text_Search.Text doesn't isn't empty and doesn't contain digits, split into searchString, removing leading and trailing whitespace
            searchString = String.Copy(SearchText.Trim()).RemoveSpecialCharacters().Split(' ');

            //test for first or last name
            if (searchString.Length == 1 && searchString[0] != "")
            {
                //check for leading whitespace and then a following character to see if its first name or last name
                if (char.IsWhiteSpace(SearchText[0]))
                {
                    if (char.IsLetter(SearchText[1]))
                    {
                        lastNameSearch = searchString[0];
                    }
                }
                else
                {
                    firstNameSearch = searchString[0];
                }
            }
            //test for first + last name
            else if (searchString.Length == 2)
            {
                firstNameSearch = searchString[0]; //first name is first element
                lastNameSearch = searchString[1]; //last name is last element
            }
            else
            {
                //text_error.Text = textUnableToParse; OLD
                GlobalVariables.errorList.Add(StringPlaceholders.errorTextUnableToParse);
            }
            //convert first and last name to uppercase
            firstNameSearch = firstNameSearch.ToUpper();
            lastNameSearch = lastNameSearch.ToUpper();
            return true;
        }

#endregion
        
        //events to handle event routing for all button clicks
        private void buttonClick(object Sender, RoutedEventArgs e)
        {
            Button myButton = new Button();

            if (Sender.GetType().Equals(new Button().GetType()))
            {
                  myButton = (Button)Sender;
            }

            if (myButton.Name == "button_SaveAddr")
            {
                SaveAddr();
            }
            else if (myButton.Name == "button_Reset")
            {
                ResetFields();
            }
            else if (myButton.Name == "button_DeleteAddr")
            {
                DeleteField();
            }
            else if (myButton.Name == "button_SaveToText")
            {
                //change which write you want to use IF USING XML THEN LOAD SHOULD ALSO HAS TO BE XML

                //WriteToFile();
                WriteToFileUserSelectBiForm();
                //WriteToFileUserSelectXML();
            }
            else if (myButton.Name == "button_LoadFromText")
            {
                //change which load you want to use IF USING XML THEN WRITE SHOULD ALSO BE XML

                //ReadFromFile();
                ImportFromFileBiForm();
                //ImportFromFileXML();
            }
        }

        //save new person
        #region store new person
        private void SaveAddr()
        {
            Person newPerson = new Person();
            //if there is something in first and last name, then try to add newPerson to allPerson
            if (text_FirstName.Text != "" && text_LastName.Text != "") {
                pushPerson(ref newPerson);
            }
            else
            {
                //text_error.Text = "First and last name must be entered";
                GlobalVariables.errorList.Add(StringPlaceholders.errorEnterBothNames);
            }
        }

        private bool pushPerson(ref Person newPerson)
        {

            newPerson.name.FirstName = text_FirstName.Text.Trim();
            newPerson.name.LastName = text_LastName.Text.Trim();
            if (text_Addr.Text != StringPlaceholders.placeholderAddress)
            {
                newPerson.address.Address = text_Addr.Text.Trim();
            }
            if (text_Addr2.Text != StringPlaceholders.placeholderAddress2)
            {
                newPerson.address.Address2 = text_Addr2.Text.Trim();
            }
            newPerson.address.City = text_City.Text.Trim();
            newPerson.address.State = text_State.Text.Trim(); //retrieves the 2 letter abbreviation
            newPerson.address.StateIndex = text_State.SelectedIndex;
            if (text_ZIP.Text != StringPlaceholders.placeholderZip)
            {
                newPerson.address.Zip = text_ZIP.Text.Trim();
            }
            if (text_PhoneNumber.Text != StringPlaceholders.placeholderPhone)
            {
                newPerson.PhoneNumber = text_PhoneNumber.Text.Trim();
            }

            //check for an error message, if none exist save newPerson to allPerson
            if (newPerson.ErrorMessage == "")
            {
                //check to see if IdNum is loaded, if it is empty then it is a new person
                if (text_IdNum.Text == "" || text_IdNum.Text == StringPlaceholders.placeholderIdNum)
                {
                    //need to assign an idNum to newPerson
                    //check to see if there is anything in availableNum, if not assign a new number
                    if (IdNumber.availableIdNum.Count == 0)
                    {
                        newPerson.IdNum = IdNumber.getNewIdNum;
                    }
                    else
                    {
                        newPerson.IdNum = IdNumber.availableIdNum[0]; //assign the first object in availableIdNum to newPerson
                        IdNumber.availableIdNum.RemoveAt(0); //remove the now used idNum
                    }
                    GlobalVariables.allPerson.Add(newPerson);
                    //select the new person in the combobox
                    GlobalVariables.allPersonView.MoveCurrentTo(newPerson);
                    //sort allPerson by last name
                    GlobalVariables.allPerson.Sort((a, b) => { return a.name.LastName.CompareTo(b.name.LastName); });
                }
                else
                {
                    //find out the matching IdNum and update the info, if IdNum not found t
                    for (int i = 0; i < GlobalVariables.allPerson.Count; ++i)
                    {
                        if (newPerson.IdNum == GlobalVariables.allPerson[i].IdNum)
                        {
                            GlobalVariables.allPerson[i] = newPerson;
                            break; //loop can stop
                        }
                    }
                }
            }
            //else display error message
            else
            {
                //text_error.Text = newPerson.ErrorMessage; OLD
                GlobalVariables.errorList.Add(newPerson.ErrorMessage);
            }

            return false;
        }
#endregion

        //Recall Address
        //Recall based on combobox
        private void recallAddr(object sender, SelectionChangedEventArgs e)
        {
            //check to see if index = -1
            if (combo_SavedAddr.SelectedIndex != -1)
            {
                //zero out every field
                //combo_SavedAddr.SelectedIndex = -1;
                text_IdNum.Text = "";
                text_FirstName.Text = "";
                text_LastName.Text = "";
                text_Addr.Text = "";
                text_Addr2.Text = "";
                text_City.Text = "";
                text_State.SelectedIndex = -1;
                text_ZIP.Text = "";
                text_PhoneNumber.Text = "";
                //text_error.Text = ""; OLD
                GlobalVariables.errorList.Clear();

                //make sure every field foreground is set to black and full opacity
                text_Addr.RaiseEvent(new RoutedEventArgs(GotFocusEvent, text_Addr));
                text_Addr2.RaiseEvent(new RoutedEventArgs(GotFocusEvent, text_Addr2));
                text_ZIP.RaiseEvent(new RoutedEventArgs(GotFocusEvent, text_ZIP));
                text_PhoneNumber.RaiseEvent(new RoutedEventArgs(GotFocusEvent, text_PhoneNumber));
                text_IdNum.RaiseEvent(new RoutedEventArgs(GotFocusEvent, text_IdNum));

                //place all saved properties in their respective field
                int index = combo_SavedAddr.SelectedIndex;
                //recall saved address based on SelectedIndex
                text_IdNum.Text = GlobalVariables.allPerson[index].IdNum;
                text_FirstName.Text = GlobalVariables.allPerson[index].name.FirstName;
                text_LastName.Text = GlobalVariables.allPerson[index].name.LastName;
                text_Addr.Text = GlobalVariables.allPerson[index].address.Address;
                text_Addr2.Text = GlobalVariables.allPerson[index].address.Address2; //still don't know if addr2 is needed or how to handle it
                text_City.Text = GlobalVariables.allPerson[index].address.City;
                text_State.SelectedIndex = GlobalVariables.allPerson[index].address.StateIndex;
                text_ZIP.Text = GlobalVariables.allPerson[index].address.Zip;
                text_PhoneNumber.Text = GlobalVariables.allPerson[index].PhoneNumber;

                //check to see if any field needs to have placeholder text
                text_Addr.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_Addr));
                text_Addr2.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_Addr2));
                text_ZIP.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_ZIP));
                text_PhoneNumber.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_PhoneNumber));
                text_IdNum.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_IdNum));
            }

            return;
        }

        //Recall based on grid selection, made useless with collectionView and IsSynchronizedWithCurrentItem
        #region old grid click
        //private void grid_RowClicked(object sender, MouseButtonEventArgs e)
        //{
        //    //put selected row into a new Person oject
        //    //Person selectedPerson = (Person)grid_output.SelectedItem;

        //    //Saved Addresses combobox selected index is the same as the grid selected index
        //    //also calls recallAddr function
        //    //combo_SavedAddr.SelectedIndex = grid_output.SelectedIndex;

        //    return;
        //}
        #endregion

        //Read and Write to file
        #region read and write to file
        //original, automatically saves to mydocuments/savedaddresses, saves as string and serialized
        private void WriteToFile()
        {
            Directory.CreateDirectory(GlobalVariables.directory); //create our directory folder if it doesn't exist
            using (StreamWriter file = new StreamWriter(GlobalVariables.path))
            {
                try
                {
                    //write all the people
                    for (int i = 0; i < GlobalVariables.allPerson.Count; ++i)
                    {
                        file.WriteLine(GlobalVariables.allPerson[i].Print());
                        file.WriteLine();
                    }
                    file.WriteLine();
                    //write the availableIdNum
                    for (int i = 0; i < IdNumber.availableIdNum.Count; ++i)
                    {
                        file.Write(IdNumber.availableIdNum.Count + ", ");
                    }
                    //idNum can be figured out on load based off the highest Person idNum and availableIdNum

                    //text_error.Text = "Filed saved in MyDocuments.SavedAddresses.Addresses.stf"; OLD
                    GlobalVariables.errorList.Add(StringPlaceholders.placeholderSaveStf);
                }
                catch (Exception e)
                {
                    //text_error.Text = e.Message; OLD
                    GlobalVariables.errorList.Add(e.Message);
                }
            }
            using (FileStream sFile = new FileStream(GlobalVariables.serializedPath, FileMode.Create))
            {
                try
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(sFile, GlobalVariables.allPerson); //first object always allPerson
                    bFormatter.Serialize(sFile, IdNumber.availableIdNum); //second object always availableNum
                }
                catch (Exception e)
                {
                    //text_error.Text = e.Message; OLD
                    GlobalVariables.errorList.Add(e.Message);
                }
            }

        }

        //lets user select file path and create name, only bFormatter serialized saving
        private void WriteToFileUserSelectBiForm()
        {
            Stream myStream;
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = ".stf";
            //create default folder
            Directory.CreateDirectory(GlobalVariables.directory); //create our directory folder if it doesn't exist
            saveFileDialog.InitialDirectory = GlobalVariables.directory; //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "stf files (*.stf)|*.stf|All files (*.*)|*.*";
            saveFileDialog.FileName = "SerializedAddresses";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                if ((myStream = saveFileDialog.OpenFile()) != null)
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(myStream, GlobalVariables.allPerson); //first object always allPerson
                    bFormatter.Serialize(myStream, IdNumber.availableIdNum); //second object always availableNum

                    myStream.Close();
                }
            }
        }

        //lets user select file path and create name, only XML saving
        private void WriteToFileUserSelectXML()
        {
            Stream myStream;
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = ".xtf";
            //create default folder
            Directory.CreateDirectory(GlobalVariables.directory); //create our directory folder if it doesn't exist
            saveFileDialog.InitialDirectory = GlobalVariables.directory; //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "xtf files (*.xtf)|*.xtf|All files (*.*)|*.*";
            saveFileDialog.FileName = "SerializedAddresses";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                if ((myStream = saveFileDialog.OpenFile()) != null)
                {
                    List<object> AddrIndex = new List<object>();
                    AddrIndex.Add(GlobalVariables.allPerson);
                    AddrIndex.Add(IdNumber.availableIdNum);
                    System.Xml.Serialization.XmlSerializer xmlWrite = new System.Xml.Serialization.XmlSerializer(typeof(List<object>), new Type[] { typeof(ObservableCollection<Person>), typeof(List<string>) });
                    xmlWrite.Serialize(myStream, AddrIndex);

                    myStream.Close();
                }
            }
        }

        //automatically loads serializedaddresses.stf from mydocuments/SavedAddresses
        private void ReadFromFile()
        {
            //TO DO parse string file
            //string notFound = @"Error, MyDocuments\SavedAddresses\Addresses.txt not found";

            //will load "SerializedAddresses.stf" first because it should be easier
            if (File.Exists(GlobalVariables.serializedPath))
            {
                //add check to see if there are users already loaded into allPerson
                //if yes provide a warning that some entered idNum might be reassigned
                if (GlobalVariables.allPerson.Count != 0)
                {
                    MessageBoxResult result = MessageBox.Show("Some ID Numbers might be reassigned if they conflict with loaded addresses. Do you want to continue?", "Continue?", MessageBoxButton.YesNo);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            ObservableCollection<Person> loadPerson = new ObservableCollection<Person>();
                            using (FileStream inSFile = new FileStream(GlobalVariables.serializedPath, FileMode.Open, FileAccess.Read))
                            {
                                BinaryFormatter bFormatter = new BinaryFormatter();
                                loadPerson = (ObservableCollection<Person>)bFormatter.Deserialize(inSFile); //first object is always the saved allPerson
                                IdNumber.availableIdNum.AddRange((List<string>)bFormatter.Deserialize(inSFile)); //second object is always the saved availableNum
                            } //done reading from the file
                            //check to see if loadPerson contains unique entries
                            if (NotDuplicateAddr(loadPerson))
                            {
                                return;
                            }
                            //figure out the next available idNum
                            FindMax(loadPerson);
                            break;
                        case MessageBoxResult.No:
                            //cancel load
                            return;
                    }
                }
                else
                {
                    using (FileStream inSFile = new FileStream(GlobalVariables.serializedPath, FileMode.Open, FileAccess.Read))
                    {
                        BinaryFormatter bFormatter = new BinaryFormatter();
                        GlobalVariables.allPerson.AddRange((ObservableCollection<Person>)bFormatter.Deserialize(inSFile)); //first object is always the saved allPerson
                        IdNumber.availableIdNum = (List<string>)bFormatter.Deserialize(inSFile); //second object is always the saved availableNum
                    } //done reading from the file
                    //calculate idNum
                    FindMax();
                }
            }
            else
            {
                //text_error.Text = StringPlaceholders.serialNotFound;
                GlobalVariables.errorList.Add(StringPlaceholders.errorSerialNotFound);
            }
        }

        //let user select bFormatter serialized text file to load
        private void ImportFromFileBiForm()
        {
            //TO DO parse string file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "stf files (*.stf)|*.stf|All files (*.*)|*.*";
            string serialNotFound = @"Error, " + openFileDialog.FileName.ToString() + " not found.";


            //will load "SerializedAddresses.stf" first because it should be easier
            if (openFileDialog.ShowDialog() == true)
            {
                //add check to see if there are users already loaded into allPerson
                //if yes provide a warning that some entered idNum might be reassigned
                if (GlobalVariables.allPerson.Count != 0)
                {
                    MessageBoxResult result = MessageBox.Show("Some ID Numbers might be reassigned if they conflict with loaded addresses. Do you want to continue?", "Continue?", MessageBoxButton.YesNo);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            ObservableCollection<Person> loadPerson = new ObservableCollection<Person>();
                            using (FileStream inSFile = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                            {
                                BinaryFormatter bFormatter = new BinaryFormatter();
                                loadPerson = (ObservableCollection<Person>)bFormatter.Deserialize(inSFile); //first object is always the saved allPerson
                                IdNumber.availableIdNum = (List<string>)bFormatter.Deserialize(inSFile); //second object is always the saved availableNum
                            } //done reading from the file
                            //check to see if loaded addresses are the same as current, if yes stop loading
                            if (NotDuplicateAddr(loadPerson))
                            {
                                return;
                            }
                            //figure out the next available idNum
                            FindMax(loadPerson);
                            break;
                        case MessageBoxResult.No:
                            //cancel load
                            return;
                    }
                }
                else
                {
                    using (FileStream inSFile = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        BinaryFormatter bFormatter = new BinaryFormatter();
                        GlobalVariables.allPerson.AddRange((ObservableCollection<Person>)bFormatter.Deserialize(inSFile)); //first object is always the saved allPerson
                        IdNumber.availableIdNum = (List<string>)bFormatter.Deserialize(inSFile); //second object is always the saved availableNum
                    } //done reading from the file
                    FindMax();
                }
            }
            else
            {
                //text_error.Text = serialNotFound; OLD
                GlobalVariables.errorList.Add(StringPlaceholders.errorSerialNotFound);
            }
        }

        //let user select XML serialized text file to load
        private void ImportFromFileXML()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "xtf files (*.xtf)|*.xtf|All files (*.*)|*.*";
            string serialNotFound = @"Error, " + openFileDialog.FileName.ToString() + " not found.";


            //will load "SerializedAddresses.stf" first because it should be easier
            if (openFileDialog.ShowDialog() == true)
            {
                //add check to see if there are users already loaded into allPerson
                //if yes provide a warning that some entered idNum might be reassigned
                if (GlobalVariables.allPerson.Count != 0)
                {
                    MessageBoxResult result = MessageBox.Show("Some ID Numbers might be reassigned if they conflict with loaded addresses. Do you want to continue?", "Continue?", MessageBoxButton.YesNo);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            ObservableCollection<Person> loadPerson = new ObservableCollection<Person>();
                            using (FileStream inSFile = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                            {
                                System.Xml.Serialization.XmlSerializer xmlLoad = new System.Xml.Serialization.XmlSerializer(typeof(List<object>), new Type[] { typeof(ObservableCollection<Person>), typeof(List<string>) });
                                List<object> AddrIndex = (List<object>)xmlLoad.Deserialize(inSFile);
                                loadPerson = (ObservableCollection<Person>)AddrIndex[0]; //first object is always the saved allPerson
                                IdNumber.availableIdNum = (List<string>)AddrIndex[1]; //second object is always the saved availableNum
                            }
                            //check to see if loaded addresses are the same as current, if yes stop loading
                            if (NotDuplicateAddr(loadPerson))
                            {
                                return;
                            }
                            //figure out the next available idNum
                            FindMax(loadPerson);
                            break;
                        case MessageBoxResult.No:
                            //cancel load
                            return;
                    }
                }
                else
                {
                    using (FileStream inSFile = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        System.Xml.Serialization.XmlSerializer xmlLoad = new System.Xml.Serialization.XmlSerializer(typeof(List<object>), new Type[] { typeof(ObservableCollection<Person>), typeof(List<string>) });
                        List<object> AddrIndex = (List<object>)xmlLoad.Deserialize(inSFile);
                        GlobalVariables.allPerson.AddRange((ObservableCollection<Person>)AddrIndex[0]); //first object is always the saved allPerson
                        IdNumber.availableIdNum = (List<string>)AddrIndex[1]; //second object is always the saved availableNum
                    }
                    //calculate idNum
                    FindMax();
                }
            }
            else
            {
                //text_error.Text = serialNotFound; OLD
                GlobalVariables.errorList.Add(StringPlaceholders.errorSerialNotFound);
            }
        }

        private void FindMax(ObservableCollection<Person> loadPerson = null)
        {
            //figure out the next available idNum
            //calculate idNum
            int max = 0;
            //look through allPerson
            for (int i = 0; i < GlobalVariables.allPerson.Count; ++i)
            {
                if (Convert.ToInt32(GlobalVariables.allPerson[i].IdNum) > max)
                {
                    max = Convert.ToInt32(GlobalVariables.allPerson[i].IdNum);
                }
            }
            if (loadPerson != null)
            {
                //look through loadPerson
                for (int i = 0; i < loadPerson.Count; ++i)
                {
                    if (Convert.ToInt32(loadPerson[i].IdNum) > max)
                    {
                        max = Convert.ToInt32(loadPerson[i].IdNum);
                    }
                }
            }
            //look through availableIdNum
            for (int i = 0; i < IdNumber.availableIdNum.Count; ++i)
            {
                if (Convert.ToInt32(IdNumber.availableIdNum[i]) > max)
                {
                    max = Convert.ToInt32(IdNumber.availableIdNum[i]);
                }
            }
            //initialize idNum
            IdNumber.initializeIdNum = max + 1;
            if (loadPerson != null)
            {
                //find out if idNum match, if they match reassign allPerson
                for (int i = 0; i < GlobalVariables.allPerson.Count; ++i)
                {
                    for (int j = 0; j < loadPerson.Count; ++j)
                    {
                        if (GlobalVariables.allPerson[i].IdNum == loadPerson[j].IdNum)
                        {
                            if (IdNumber.availableIdNum.Count != 0)
                            {
                                GlobalVariables.allPerson[i].IdNum = IdNumber.availableIdNum[0];
                                IdNumber.availableIdNum.RemoveAt(0);
                            }
                            else
                            {
                                GlobalVariables.allPerson[i].IdNum = IdNumber.getNewIdNum;
                            }
                            break; //exit the loadPerson for loop
                        }
                    }
                }
                //add loadPerson to allPerson and refresh allPersonView
                GlobalVariables.allPerson.AddRange(loadPerson);
                GlobalVariables.allPersonView.Refresh();
            }
            //make sure no num in idNum is being used
            for (int i = 0; i < GlobalVariables.allPerson.Count; ++i)
            {
                int j = 0;
                while (j < IdNumber.availableIdNum.Count)
                {
                    if (GlobalVariables.allPerson[i].IdNum == IdNumber.availableIdNum[j])
                    {
                        IdNumber.availableIdNum.RemoveAt(j);
                    }
                    else
                    {
                        ++j;
                    }
                }
            }
            return;
        }

        private bool NotDuplicateAddr(ObservableCollection<Person> loadPerson)
        {
            if (GlobalVariables.allPerson.Equals(loadPerson))
            {
                //text_error.Text = StringPlaceholders.duplicateLoad; OLD
                GlobalVariables.errorList.Add(StringPlaceholders.errorDuplicateLoad);
                return false;
            }
            //check to see if current addresses contain any of the loaded ones, if so delete the load one. Note they have to be exactly the same in all categories to be deleted
            for (int i = 0; i < loadPerson.Count; ++i)
            {
                for (int j = 0; j < GlobalVariables.allPerson.Count; ++j)
                {
                    if (loadPerson[i].Equals(GlobalVariables.allPerson[j]))
                    {
                        IdNumber.availableIdNum.Add(loadPerson[i].IdNum);
                        loadPerson.RemoveAt(i);
                    }
                }
            }
            //check to see if there are any objects in loadPerson, if not show an error and abort load
            if (loadPerson.Count == 0)
            {
                //text_error.Text = StringPlaceholders.duplicateLoad; OLD
                GlobalVariables.errorList.Add(StringPlaceholders.errorDuplicateLoad);
                return false;
            }

            return true;
        }
        #endregion

        //Misc Functions
        //reset Fields
        private void ResetFields()
        {
            combo_SavedAddr.SelectedIndex = -1;

            text_IdNum.Text = StringPlaceholders.placeholderIdNum;
            text_FirstName.Text = "";
            text_LastName.Text = "";
            text_Addr.Text = "";
            text_Addr2.Text = "";
            text_City.Text = "";
            text_State.SelectedIndex = -1;
            text_ZIP.Text = "";
            text_PhoneNumber.Text = "";
            //text_error.Text = ""; OLD
            GlobalVariables.errorList.Clear();

            text_Addr.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_Addr));
            text_Addr2.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_Addr2));
            text_ZIP.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_ZIP));
            text_PhoneNumber.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_PhoneNumber));

            grid_output.ItemsSource = GlobalVariables.allPerson;
        }

        //delete Field
        private void DeleteField()
        {
            //take the selected index and remove it for allPerson
            int selectedPerson = -1;
            selectedPerson = combo_SavedAddr.SelectedIndex;
            if (selectedPerson == -1)
            {
                //text_error.Text = invalidIndex;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + GlobalVariables.allPerson[selectedPerson].IdNum + ' ' + GlobalVariables.allPerson[selectedPerson].name.FirstName + ' ' + GlobalVariables.allPerson[selectedPerson].name.LastName + '?', "Delete Address", MessageBoxButton.YesNo);
                switch(result)
                {
                    case MessageBoxResult.Yes:
                        IdNumber.availableIdNum.Add(GlobalVariables.allPerson[selectedPerson].IdNum); //mark this idNum as available to be used
                        GlobalVariables.allPerson.RemoveAt(selectedPerson);
                        ResetFields(); //reset the selected index
                        break;
                    case MessageBoxResult.No:
                        //do nothing
                        break;
                }
                //reset text_error
                //text_error.Text = ""; OLD
                GlobalVariables.errorList.Clear();
            }
        }

        //Return key pressed
        private void KeyPressed(object sender, KeyEventArgs e)
        {
            //used for when return is entered
            //if search boxes have focus call searchByNames()
            //if another property is focused OR the store addr button call button_SaveAddr_Click
            if(e.Key == Key.Return)
            {
                if (text_FirstName.IsFocused || text_LastName.IsFocused || text_Addr.IsFocused || text_Addr2.IsFocused || text_City.IsFocused || text_State.IsFocused || text_ZIP.IsFocused || text_PhoneNumber.IsFocused || button_SaveAddr.IsFocused)
                {
                    text_Addr.RaiseEvent(new RoutedEventArgs(LostFocusEvent, text_Addr));

                    button_SaveAddr.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); //clicks Store Address button
                }
            }
        }

        #region focus and blur events

        private void FirstNameFocus(object sender, RoutedEventArgs e)
        {
            text_FirstName.Background = Brushes.White;
        }

        private void FirstNameBlur(object sender, RoutedEventArgs e)
        {
            //validate first name (no digits)
            if (text_FirstName.Text != "")
            {
                for (int i = 0; i < text_FirstName.Text.Length; ++i)
                {
                    //if digit was found, output error message and change background to red
                    if (Char.IsDigit(text_FirstName.Text[i]))
                    {
                        //text_error.Text = StringPlaceholders.NameErrorLetters; OLD
                        GlobalVariables.errorList.Add(StringPlaceholders.errorNameLetters);
                        text_FirstName.Background = Brushes.Salmon;
                        return;
                    }
                }
                //reached this point means name has no digits
                //text_error.Text = ""; OLD
                GlobalVariables.errorList.Remove(StringPlaceholders.errorNameLetters);
            }
        }

        private void LastNameFocus(object sender, RoutedEventArgs e)
        {
            text_LastName.Background = Brushes.White;
        }

        private void LastNameBlur(object sender, RoutedEventArgs e)
        {
            //validate first name (no digits)
            if (text_LastName.Text != "")
            {
                for (int i = 0; i < text_LastName.Text.Length; ++i)
                {
                    //if digit was found, output error message and change background to red
                    if (Char.IsDigit(text_LastName.Text[i]))
                    {
                        //text_error.Text = StringPlaceholders.NameErrorLetters; OLD
                        GlobalVariables.errorList.Add(StringPlaceholders.errorNameLetters);
                        text_LastName.Background = Brushes.Salmon;
                        return;
                    }
                }
                //reached this point means name has no digits
                //text_error.Text = ""; OLD
                GlobalVariables.errorList.Remove(StringPlaceholders.errorNameLetters);
            }
        }

        private void AddressFocus(object sender, RoutedEventArgs e)
        {
            if (text_Addr.Text == StringPlaceholders.placeholderAddress || text_Addr.Text == "")
            {
                text_Addr.Text = "";
                text_Addr.Foreground = Brushes.Black;
                text_Addr.Opacity = 1;
            }
        }

        private void AddressBlur(object sender, RoutedEventArgs e)
        {
            if (text_Addr.Text == "")
            {
                text_Addr.Foreground = Brushes.Gray;
                text_Addr.Opacity = 0.75;
                text_Addr.Text = StringPlaceholders.placeholderAddress;
            }
        }

        private void Address2Focus(object sender, RoutedEventArgs e)
        {
            if (text_Addr2.Text == StringPlaceholders.placeholderAddress2 || text_Addr2.Text == "")
            {
                text_Addr2.Text = "";
                text_Addr2.Foreground = Brushes.Black;
                text_Addr2.Opacity = 1;
            }
        }

        private void Address2Blur(object sender, RoutedEventArgs e)
        {
            if (text_Addr2.Text == "")
            {
                text_Addr2.Foreground = Brushes.Gray;
                text_Addr2.Opacity = 0.75;
                text_Addr2.Text = StringPlaceholders.placeholderAddress2;
            }
        }

        private void ZipFocus(object sender, RoutedEventArgs e)
        {
            text_ZIP.Background = Brushes.White;
            if (text_ZIP.Text == StringPlaceholders.placeholderZip || text_ZIP.Text == "")
            {
                text_ZIP.Text = "";
                text_ZIP.Foreground = Brushes.Black;
                text_ZIP.Opacity = 1;
            }
        }

        private void ZipBlur(object sender, RoutedEventArgs e)
        {
            if (text_ZIP.Text == "")
            {
                text_ZIP.Foreground = Brushes.Gray;
                text_ZIP.Opacity = 0.75;
                text_ZIP.Text = StringPlaceholders.placeholderZip;
            }
            else
            {
                Regex zipCheck = new Regex(@"^\d{5}(?:[-\s]?\d{4})?$"); //taken from https://stackoverflow.com/questions/2577236/regex-for-zip-code

                if (!zipCheck.IsMatch(text_ZIP.Text))
                {
                    //text_error.Text = StringPlaceholders.errorZipGeneric;  OLD
                    GlobalVariables.errorList.Add(StringPlaceholders.errorZipGeneric);
                    text_ZIP.Background = Brushes.Salmon;
                }
                else
                {
                    //text_error.Text = ""; OLD
                    GlobalVariables.errorList.Remove(StringPlaceholders.errorZipGeneric);
                }
            }
        }

        private void PhoneFocus(object sender, RoutedEventArgs e)
        {
            text_PhoneNumber.Background = Brushes.White;
            if (text_PhoneNumber.Text == StringPlaceholders.placeholderPhone || text_PhoneNumber.Text == "")
            {
                text_PhoneNumber.Text = "";
                text_PhoneNumber.Foreground = Brushes.Black;
                text_PhoneNumber.Opacity = 1;
            }
        }

        private void PhoneBlur(object sender, RoutedEventArgs e)
        {
            if (text_PhoneNumber.Text == "")
            {
                text_PhoneNumber.Foreground = Brushes.Gray;
                text_PhoneNumber.Opacity = 0.75;
                text_PhoneNumber.Text = StringPlaceholders.placeholderPhone;
            }
            else 
            {
                Regex phoneCheck = new Regex(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$"); //taken from https://stackoverflow.com/questions/16699007/regular-expression-to-match-standard-10-digit-phone-number

                if (!phoneCheck.IsMatch(text_PhoneNumber.Text))
                {
                    //text_error.Text = StringPlaceholders.PhoneErrorGeneric; OLD
                    GlobalVariables.errorList.Add(StringPlaceholders.errorPhoneGeneric);
                    text_PhoneNumber.Background = Brushes.Salmon;
                }
                else {
                    //text_error.Text = "";
                    GlobalVariables.errorList.Remove(StringPlaceholders.errorPhoneGeneric);
                }
            }
        }

        private void SearchFocus(object sender, RoutedEventArgs e)
        {
            if (text_Search.Text == StringPlaceholders.placeholderSearch || text_Search.Text == "")
            {
                text_Search.Text = "";
                text_Search.Foreground = Brushes.Black;
                text_Search.Opacity = 1;
            }
        }

        private void SearchBlur(object sender, RoutedEventArgs e)
        {
            if (text_Search.Text == "")
            {
                text_Search.Foreground = Brushes.Gray;
                text_Search.Opacity = 0.75;
                text_Search.Text = StringPlaceholders.placeholderSearch;
            }
        }

        //No longer used
        //private void IdFocus(object sender, RoutedEventArgs e)
        //{
        //    if (text_IdNum.Text == GlobalVariables.IdNumPlaceholder || text_IdNum.Text == "")
        //    {
        //        text_IdNum.Text = "";
        //        text_IdNum.Foreground = Brushes.Black;
        //        text_IdNum.Opacity = 1;
        //    }
        //}

        //private void IdBlur(object sender, RoutedEventArgs e)
        //{
        //    if (text_IdNum.Text == "")
        //    {
        //        text_IdNum.Foreground = Brushes.Gray;
        //        text_IdNum.Opacity = 0.75;
        //        text_IdNum.Text = GlobalVariables.IdNumPlaceholder;
        //    }
        //}
        #endregion
    }
}

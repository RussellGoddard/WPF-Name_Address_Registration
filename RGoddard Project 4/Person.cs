using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace RGoddard_Project_4
{
    [Serializable]
    public class Person : IComparable<Person>, IEquatable<Person>
    {
        //kept as a string to keep idNum flexible, possible to have letters in it would just need to modify class IdNum
        public string IdNum { get { return idNum; } set { idNum = value; } }
        private string idNum;

        public Name name { get; set; }
        public PersonalAddress address { get; set; }

        public string PhoneNumber { get { return phoneNumber; } set { phoneNumber = VerifyPhone(value); } }
        public string PhoneNumberFormatted { get { return FormatPhone(); } }
        private string phoneNumber;

        public string ErrorMessage { get { return errorMessage + name.ErrorMessage + address.ErrorMessage; } } //read only
        private string errorMessage;

        private string FormatPhone()
        {
            if (phoneNumber.Length == 0)
            {
                return "";
            }
            else if (phoneNumber.Length == 10)
            {
                return string.Format("({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6));
            }
            else if (phoneNumber.Length == 11)
            {
                return string.Format("{0} ({1}) {2}-{3}", phoneNumber.Substring(0, 1), phoneNumber.Substring(1, 3), phoneNumber.Substring(4, 3), phoneNumber.Substring(7));
            }
            throw new ArgumentException("phoneNumber"); //only reached if verifyPhone fails
        }
        private string VerifyPhone(string phone)
        {
            string errorNumeric = "Phone number must be all numeric \n";
            string errorLength = "Phone number entered was incorrect length \n";
            Regex phoneCheck = new Regex(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$"); //taken from https://stackoverflow.com/questions/16699007/regular-expression-to-match-standard-10-digit-phone-number
            //switch to REGEX? Not needed, currently functionality works fine
            //care about dialing prefixes that don't match US numbers? Keeping things US focused for now

            int index = 0; //counter for while loop
            while (index < phone.Length)
            {
                //if character at index i is not numeric
                if (!Char.IsDigit(phone[index]))
                {
                    phone = phone.Remove(index, 1); //remove 1 character at index i
                }
                else
                {
                    ++index;
                }
            }

            //check length, phone numbers should be 10 or 11 (with international dialing prefix) characters
            switch (phone.Length)
            {
                case 10:
                    //check to make sure every character is numeric
                    for (int i = 0; i < phone.Length; ++i)
                    {
                        if (!Char.IsDigit(phone[i]))
                        {
                            //failed check
                            errorMessage += errorNumeric;
                            return ""; //failed check, return empty string
                        }
                    }

                    //assume they are from North America and add 1 to front of string
                    phone = phone.Insert(0, "1");

                    break;
                case 11:
                    //check to make sure every character is numeric
                    for (int i = 0; i < phone.Length; ++i)
                    {
                        if (!Char.IsDigit(phone[i]))
                        {
                            //failed check
                            errorMessage += errorNumeric;
                            return ""; //failed check, return empty string
                        }
                    }
                    break;
                default:
                    //phone number incorrect length
                    errorMessage += errorLength;
                    return ""; //failed check, return empty string
            }


            return phone;
        }

        public override string ToString()
        {
            return name.FirstName + ' ' + name.LastName;
        }

        public int CompareTo(Person other)
        {
            return this.name.LastName.CompareTo(other.name.LastName);
        }

        public string Print()
        {
            return name.Print() + Environment.NewLine + address.Print() + Environment.NewLine + PhoneNumber;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Person);
        }

        public bool Equals(Person other)
        {
            if (other == null)
            {
                return false;
            }

            return this.idNum.Equals(other.idNum) && this.phoneNumber.Equals(other.phoneNumber) && 
                this.name.Equals(other.name) && this.address.Equals(other.address);
        }

        //taken from https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 79;
                hash = hash * 31 + idNum.GetHashCode();
                hash = hash * 31 + phoneNumber.GetHashCode();
                return hash;
            }
        }

        public Person()
        {
            name = new Name();
            address = new PersonalAddress();
            phoneNumber = "";
            errorMessage = "";
        }
    }

    [Serializable]
    public class Name : IEquatable<Name>
    {
        public string FirstName { get { return firstName; } set { firstName = verifyName(value); } }
        private string firstName;
        public string LastName { get { return lastName; } set { lastName = verifyName(value); } }
        private string lastName;

        public string ErrorMessage { get { return errorMessage; } } //read only
        private string errorMessage = ""; //string to hold any error message that may occur during validation

        private string verifyName(string name)
        {
            name = name.Replace(" ", ""); //remove whitespace

            foreach (char element in name)
            {
                //if selected char is not a letter
                if (!char.IsLetter(element))
                {
                    errorMessage += StringPlaceholders.errorNameLetters;
                    return ""; //false, return empty string
                }
            }

            //capatalize first letter (further validation not done because of edge cases like Irish names having capatalized letters in the middle)
            char[] a = name.ToCharArray();
            a[0] = Char.ToUpper(a[0]);
            name = new string(a);

            return name;
        }

        public string Print()
        {
            return FirstName + ' ' + LastName;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Name);
        }

        public bool Equals(Name other)
        {
            if (other == null)
            {
                return false;
            }
            return this.firstName.Equals(other.firstName) && this.lastName.Equals(other.lastName);
        }


        //taken from https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        public override int GetHashCode()
        {
            unchecked 
            {
                int hash = 79;
                hash = hash * 31 + firstName.GetHashCode();
                hash = hash * 31 + lastName.GetHashCode();
                return hash;
            }
        }

        public Name()
        {
            firstName = "";
            lastName = "";
        }
    }

    [Serializable]
    public class PersonalAddress : IEquatable<PersonalAddress>
    {
        public string Address { get { return address; } set { address = value; } }
        private string address;

        public string Address2 { get { return address2; } set { address2 = value; } }
        private string address2;

        public string AddressFull
        {
            get
            {
                string tempString = address;
                if (address2 != "")
                {
                    tempString += " " + address2;
                }
                return tempString;
            }
        }

        public string City { get { return city; } set { city = value; } }
        private string city;

        public string State { get { return state; } set { state = verifyState(value); } }
        private string state;

        public int StateIndex { get { return stateIndex; } set { stateIndex = value; } }
        private int stateIndex;

        public string Zip { get { return zip; } set { zip = verifyZip(value); } }
        public string ZipFormatted { get { return ZipFormat(); } }
        private string zip;

        public string ErrorMessage { get { return errorMessage; } } //read only
        private string errorMessage = ""; //string to hold any error message that may occur during validation

        private string verifyState(string state)
        {
            //retrieve abbreviation from string
            if (state.Length != 0)
            {
                return state.Substring(0, 2);
            }
            else
            {
                return "";
            }
        }

        private string ZipFormat()
        {
            if (Zip.Length == 0 || Zip.Length == 5)
            {
                return Zip;
            }
            else if (Zip.Length == 9)
            {
                return string.Format("{0}-{1}", Zip.Substring(0, 5), Zip.Substring(5));
            }
            throw new ArgumentException("Zip"); //should never be reached unless verifyZip fails
        }
        private string verifyZip(string zip)
        {
            Regex zipCheck = new Regex(@"^\d{5}(?:[-\s]?\d{4})?$"); //taken from https://stackoverflow.com/questions/2577236/regex-for-zip-code

            if (zipCheck.IsMatch(zip))
            {
                //valid zip entered
                //if length == 10, remove index 5 (which will always be a space or hyphen since it passed the regex test)
                if (zip.Length == 10)
                {
                    zip = zip.Remove(5, 1);
                }
                return zip;
            }
            else
            {
                //invalid zip entered, look for the error and add to errorMessage
                //verify that ZIP is 5 digits or 9 digits (remove space/dash between zip code if 9 characters)
                switch (zip.Length)
                {
                    case 5:
                        //verify that ZIP is all digits
                        for (int i = 0; i < zip.Length; ++i)
                        {
                            if (!Char.IsDigit(zip[i]))
                            {
                                errorMessage += StringPlaceholders.errorZipNumeric;
                                break; //error found exit loop
                            }
                        }
                        break;
                    case 9:
                        //verify that ZIP is all digits
                        for (int i = 0; i < zip.Length; ++i)
                        {
                            if (!Char.IsDigit(zip[i]))
                            {
                                errorMessage += StringPlaceholders.errorZipNumeric;
                                break; //error found exit loop
                            }
                        }
                        break;
                    case 10:
                        //remove all whitespace and dashes, check to see if length is 9 then check for digits
                        zip = zip.Replace(" ", "");
                        zip = zip.Replace("-", "");

                        if (zip.Length != 9)
                        {
                            //ZIP wrong length, return false
                            errorMessage += StringPlaceholders.errorZipLength;
                        }

                        //verify that ZIP is all digits
                        for (int i = 0; i < zip.Length; ++i)
                        {
                            if (!Char.IsDigit(zip[i]))
                            {
                                errorMessage += StringPlaceholders.errorZipNumeric;
                                break; //error found exit loop
                            }
                        }
                        break;
                    default:
                        //ZIP doesn't meet length requirements, return false
                        errorMessage += StringPlaceholders.errorZipLength;
                        break;
                }
                return ""; //invalid zip return empty string
            }




        }

        public string Print()
        {
            string output = AddressFull + Environment.NewLine + City + ", " + State + " " + Zip;
            return output;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as PersonalAddress);
        }

        public bool Equals(PersonalAddress other)
        {
            if (other == null)
            {
                return false;
            }
            return this.address.Equals(other.address) && this.address2.Equals(other.address2) && 
                this.city.Equals(other.city) && this.state.Equals(other.state) && 
                this.stateIndex.Equals(other.stateIndex) && this.zip.Equals(other.zip);
        }

        //taken from https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 79;
                hash = hash * 31 + address.GetHashCode();
                hash = hash * 31 + address2.GetHashCode();
                hash = hash * 31 + city.GetHashCode();
                hash = hash * 31 + state.GetHashCode();
                hash = hash * 31 + stateIndex.GetHashCode();
                hash = hash * 31 + zip.GetHashCode();
                return hash;
            }
        }

        public PersonalAddress()
        {
            address = "";
            address2 = "";
            city = "";
            state = "";
            stateIndex = -1;
            zip = "";
            errorMessage = "";
        }
    }
}
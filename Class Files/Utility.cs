﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MMR_Tracker_V2
{
    class Utility
    {
        public static string ConvertCsvFileToJsonObject(string path)
        {
            var csv = new List<string[]>();
            var lines = File.ReadAllLines(path);

            foreach (string line in lines)
                csv.Add(line.Split(','));

            var properties = lines[0].Split(',');

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j], csv[i][j]);

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }
        public static List<LogicObjects.LogicEntry> CloneLogicList(List<LogicObjects.LogicEntry> logic)
        {
            //Create a deep copy of a logic object by converting it to a json and coverting it back.
            //I have no idea why this works and it seems silly but whatever.
            return JsonConvert.DeserializeObject<List<LogicObjects.LogicEntry>>(JsonConvert.SerializeObject(logic));
        }
        public static LogicObjects.TrackerInstance CloneTrackerInstance(LogicObjects.TrackerInstance instance)
        {
            //Create a deep copy of a tracker object by converting it to a json and coverting it back.
            return JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(JsonConvert.SerializeObject(instance));
        }
        public static string FileSelect(string title, string filter)
        {
            OpenFileDialog SelectedFile = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                FilterIndex = 1,
                Multiselect = false
            };
            if (SelectedFile.ShowDialog() != DialogResult.OK) { return ""; }
            return SelectedFile.FileName;
        }
        public static bool FilterSearch(LogicObjects.LogicEntry logic, string searchTerm, string NameToCompare, LogicObjects.LogicEntry RandomizedItem = null)
        {
            if (NameToCompare == null) { NameToCompare = logic.DictionaryName; }
            if (searchTerm == "") { return true; }
            if (searchTerm[0] == '^')
            {
                searchTerm = searchTerm.Substring(1);
                if (searchTerm == "") { return true; }
            }
            string[] searchTerms = searchTerm.Split('|');
            foreach (string term in searchTerms)
            {
                string[] subTerms = term.Split('&');
                bool valid = true;
                foreach (string i in subTerms)
                {
                    bool Inverse = false;
                    bool Perfect = false;
                    var subterm = i;
                    if (subterm == "") { continue; }
                    if (subterm[0] == '!')
                    {
                        Inverse = true;
                        subterm = subterm.Substring(1);
                    }
                    else if (subterm[0] == '=')
                    {
                        Perfect = true;
                        subterm = subterm.Substring(1);
                    }
                    if (subterm == "") { continue; }
                    if (subterm[0] == '#')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (logic.LocationArea.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && logic.LocationArea.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '@')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if ((logic.ItemSubType.ToLower().Contains(subterm.Substring(1).ToLower())) == Inverse) { valid = false; }
                        if (Perfect && logic.ItemSubType.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '~')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if ((logic.DictionaryName.ToLower().Contains(subterm.Substring(1).ToLower())) == Inverse) { valid = false; }
                        if (Perfect && logic.DictionaryName.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '$')
                    {
                        if (subterm.Substring(1) == "" || logic.ItemName == null) { continue; }
                        if (logic.ItemName.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && logic.ItemName.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '%')
                    {
                        if (subterm.Substring(1) == "" || logic.LocationName == null) { continue; }
                        if (logic.LocationName.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && logic.LocationName.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '_')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (RandomizedItem == null) { valid = false; }
                        else if (RandomizedItem.ItemName.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && logic.ItemName.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                    }
                    else if (subterm[0] == '*')
                    {
                        if (subterm.Substring(1) == "") { continue; }
                        if (NameToCompare.ToLower().Contains(subterm.Substring(1).ToLower()) == Inverse) { valid = false; }
                        if (Perfect && NameToCompare.ToLower() != subterm.Substring(1).ToLower()) { valid = false; }
                        if (!logic.Starred) { valid = false; }
                    }
                    else
                    {
                        if (!NameToCompare.ToLower().Contains(subterm.ToLower()) == !Inverse) { valid = false; }
                        if (Perfect && NameToCompare.ToLower() != subterm.ToLower()) { valid = false; }
                    }
                }
                if (valid) { return true; }
            }
            return false;
        }
        public static bool CheckforSpoilerLog(List<LogicObjects.LogicEntry> Logic, bool full = false, bool FakeAllowed = true)
        {
            bool fullLog = true;
            bool Spoiler = false;
            foreach (var i in Logic)
            {
                if (i.IsFake || string.IsNullOrWhiteSpace(i.LocationName)) { continue; }
                if (i.SpoilerRandom > (FakeAllowed ? -2 : -1)) 
                { 
                    Spoiler = true;
                    if (!full) { Console.WriteLine(i.DictionaryName + " Had SpoilerData"); }
                }
                if (i.SpoilerRandom < (FakeAllowed ? -1 : 0)) 
                { 
                    fullLog = false;
                    if (full) { Console.WriteLine(i.DictionaryName + " Does not have SpoilerData"); }
                }
            }
            return (full) ? fullLog : Spoiler;
        }
        public static bool IsDivider(string text)
        {
            if (text == null || text == "") { return false; }
            int occurences = 0;
            foreach (var i in text)
            {
                if (i == '=') 
                { 
                    occurences++; 
                    if (occurences >= 5) { return true; } 
                }
                else { occurences = 0; }
            }
            return false;
        }
        public static bool CheckForRandomEntrances(LogicObjects.TrackerInstance Instance, bool Spoiler = false, int validEntranceCount = 6)
        {
            if (!Instance.EntranceRando) { return false; }
            int count = 0;
            foreach (var i in Instance.Logic.Where(x => x.IsEntrance()))
            {
                if (!Spoiler && i.AppearsInListbox()) { count += 1; }
                if (Spoiler && i.SpoilerRandom != i.ID && i.SpoilerRandom > -1) { count += 1; }
                if (count >= validEntranceCount) { return true; }
            }
            return false;
        }
        public static void FixSpoilerInconsistency(LogicObjects.TrackerInstance Instance)
        {
            var RandoOptionsContradictSpoiler = false;
            foreach (var i in Instance.Logic)
            {
                if (i.Unrandomized(2) && i.SpoilerRandom != i.ID && i.SpoilerRandom > -1) { RandoOptionsContradictSpoiler = true; break; }
            }
            if (RandoOptionsContradictSpoiler)
            {
                var AttemptFix = MessageBox.Show("You have marked items as unrandomized that are are radnomized according to your spoiler log. Would you like the tracker to attempt to correct this? This may leave items unrandomized that were simply place vanilla which could spoil your seed.", "Randomization Option Inconsistency", MessageBoxButtons.YesNo);
                if (AttemptFix == DialogResult.Yes)
                {
                    foreach (var i in Instance.Logic)
                    {
                        if (i.Unrandomized(2) && i.SpoilerRandom != i.ID && i.SpoilerRandom > -1) { i.Options = (i.StartingItem()) ? 4 : 0; }
                    }
                }
            }
        }
        public static int BoolToInt(bool Bool, bool FalseFirst = true)
        {
            return Bool ? (FalseFirst ? 1 : 0 ) : (FalseFirst ? 0 : 1);
        } 
        public static List<string> WrapStringInListBox(ListBox container, string Measure, string indent = "    ")
        {
            Font font = container.Font;
            int width = container.Width;
            Graphics g = container.CreateGraphics();

            if (IsDivider(Measure) || (int)g.MeasureString(Measure, font).Width < width) { return new List<string> { Measure }; }

            List<string> ShortenedStrings = new List<string>();
            string[] words = Measure.Split(' ');
            string ShortenedString = "";
            foreach (string word in words)
            {
                string testString = ShortenedString + word + " ";
                int Stringwidth = (int)g.MeasureString(testString, font).Width;
                if (Stringwidth > width)
                {
                    ShortenedStrings.Add(ShortenedString);
                    ShortenedString = indent + word + " ";
                }
                else
                {
                    ShortenedString = ShortenedString + word + " ";
                }
            }
            ShortenedStrings.Add(ShortenedString);
            return ShortenedStrings;
        }
        public static string CreateDivider(ListBox container, string DividerText = "")
        {
            Font font = container.Font;
            int width = container.Width;
            Graphics g = container.CreateGraphics();

            int marks = 1;
            string Divider = "";
            while((int)g.MeasureString(Divider, font).Width <= width)
            {
                string Section = "";
                for (var i = 0; i < marks; i++)
                {
                    Section += "=";
                }
                Divider = Section + DividerText + Section;
                marks++;
            }
            return Divider;
        }
    }
    public class Crypto
    {

        //While an app specific salt is not the best practice for
        //password based encryption, it's probably safe enough as long as
        //it is truly uncommon. Also too much work to alter this answer otherwise.
        private static byte[] _salt = Encoding.ASCII.GetBytes("YesThisIsBadEncryptionButItDoesntNeedToBeGood");

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        public static string EncryptStringAES(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            string outStr = null;                       // Encrypted string to return
            RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        public static string DecryptStringAES(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}

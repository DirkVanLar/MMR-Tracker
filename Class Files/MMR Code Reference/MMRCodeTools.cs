using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker.Class_Files.MMR_Code_Reference.items;

namespace MMR_Tracker.Class_Files.MMR_Code_Reference
{
    class MMRCodeTools
    {
        public static void CheckMMRItemsAgainstDictionary()
        {
            var itemPool = Enum.GetValues(typeof(Item)).Cast<Item>();
            var itemPoolNames = itemPool.Select(x => x.ToString()).ToArray();
            var LogicDictionary = JsonConvert.DeserializeObject<List<LogicObjects.LogicDictionaryEntry>>(Utility.ConvertCsvFileToJsonObject(File.ReadAllLines(@"Recources\Dictionaries\MMRDICTIONARYV8.csv")));
            Console.WriteLine("Logic Items Missing From Dictionary=====================");
            foreach (var i in itemPoolNames)
            {
                if (LogicDictionary.Find(x => x.DictionaryName == i) == null)
                {
                    Console.WriteLine(i);
                }
            }
            Console.WriteLine("\nDictionary Items Missing From Logic=====================");
            foreach (var i in LogicDictionary)
            {
                string EntryName = i.DictionaryName;
                if (!itemPoolNames.Contains(EntryName))
                {
                    Console.WriteLine(i);
                }
            }
        }

    }

    public static class AttributeExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null)
            {
                return null;
            }
            return type.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null)
            {
                return null;
            }
            return type.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>();
        }

        public static bool HasAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null)
            {
                return false;
            }
            return type.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .Any();
        }
    }
}

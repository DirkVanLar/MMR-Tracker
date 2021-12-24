using Helpers;
using MMR_Tracker_V2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMR_Tracker.Other_Games
{
    public class OOT3DR
    {
        public static void GetSpoilerLog()
        {
            string xml = File.ReadAllText(@"C:\Users\drumm\Downloads\168670225_Longshot_Gold_Scale_Map_Bow_Saw-spoilerlog.xml");
            var SpoilerLog = xml.ParseXML<spoilerlog>();

            string DictionaryPath = @"Recources\Dictionaries\Other Games\OOTR V1 json Logic Dictionary.json";
            LogicObjects.LogicDictionary OOTRDict = JsonConvert.DeserializeObject<LogicObjects.LogicDictionary>(File.ReadAllText(DictionaryPath));

            foreach (var i in SpoilerLog.alllocations)
            {
                var t = Regex.Replace(i.name, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);
                t = t.Replace("Gerudo Training Grounds", "Gerudo Training Ground");
                if (t.StartsWith("MK ")) { t = t.Replace("MK ", "Market "); }
                if (t.StartsWith("KF Mido ")) { t = t.Replace("KF Mido ", "KF Midos "); }

                if (OOTRDict.LogicDictionaryList.Where(x => x.SpoilerLocation != null && x.SpoilerLocation.Contains(t)).Count() == 0)
                {
                    Console.WriteLine(t);
                }
            }
        }

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute("spoiler-log", Namespace = "", IsNullable = false)]
        public partial class spoilerlog
        {

            private spoilerlogSetting[] settingsField;

            private spoilerlogTrick[] enabledtricksField;

            private spoilerlogDungeon[] masterquestdungeonsField;

            private spoilerlogTrial[] requiredtrialsField;

            private spoilerlogSphere[] playthroughField;

            private spoilerlogLocation[] wayoftheherolocationsField;

            private spoilerlogHint[] hintsField;

            private spoilerlogLocation1[] alllocationsField;

            private string versionField;

            private uint seedField;

            private string hashField;

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("setting", IsNullable = false)]
            public spoilerlogSetting[] settings
            {
                get
                {
                    return this.settingsField;
                }
                set
                {
                    this.settingsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayAttribute("enabled-tricks")]
            [System.Xml.Serialization.XmlArrayItemAttribute("trick", IsNullable = false)]
            public spoilerlogTrick[] enabledtricks
            {
                get
                {
                    return this.enabledtricksField;
                }
                set
                {
                    this.enabledtricksField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayAttribute("master-quest-dungeons")]
            [System.Xml.Serialization.XmlArrayItemAttribute("dungeon", IsNullable = false)]
            public spoilerlogDungeon[] masterquestdungeons
            {
                get
                {
                    return this.masterquestdungeonsField;
                }
                set
                {
                    this.masterquestdungeonsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayAttribute("required-trials")]
            [System.Xml.Serialization.XmlArrayItemAttribute("trial", IsNullable = false)]
            public spoilerlogTrial[] requiredtrials
            {
                get
                {
                    return this.requiredtrialsField;
                }
                set
                {
                    this.requiredtrialsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("sphere", IsNullable = false)]
            public spoilerlogSphere[] playthrough
            {
                get
                {
                    return this.playthroughField;
                }
                set
                {
                    this.playthroughField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayAttribute("way-of-the-hero-locations")]
            [System.Xml.Serialization.XmlArrayItemAttribute("location", IsNullable = false)]
            public spoilerlogLocation[] wayoftheherolocations
            {
                get
                {
                    return this.wayoftheherolocationsField;
                }
                set
                {
                    this.wayoftheherolocationsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("hint", IsNullable = false)]
            public spoilerlogHint[] hints
            {
                get
                {
                    return this.hintsField;
                }
                set
                {
                    this.hintsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayAttribute("all-locations")]
            [System.Xml.Serialization.XmlArrayItemAttribute("location", IsNullable = false)]
            public spoilerlogLocation1[] alllocations
            {
                get
                {
                    return this.alllocationsField;
                }
                set
                {
                    this.alllocationsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public uint seed
            {
                get
                {
                    return this.seedField;
                }
                set
                {
                    this.seedField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string hash
            {
                get
                {
                    return this.hashField;
                }
                set
                {
                    this.hashField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class spoilerlogSetting
        {

            private string nameField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class spoilerlogTrick
        {

            private string nameField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class spoilerlogDungeon
        {

            private string nameField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class spoilerlogTrial
        {

            private string nameField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class spoilerlogSphere
        {

            private spoilerlogSphereLocation[] locationField;

            private byte levelField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("location")]
            public spoilerlogSphereLocation[] location
            {
                get
                {
                    return this.locationField;
                }
                set
                {
                    this.locationField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte level
            {
                get
                {
                    return this.levelField;
                }
                set
                {
                    this.levelField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class spoilerlogSphereLocation
        {

            private string nameField;

            private string _Field;

            private byte priceField;

            private bool priceFieldSpecified;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string _
            {
                get
                {
                    return this._Field;
                }
                set
                {
                    this._Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte price
            {
                get
                {
                    return this.priceField;
                }
                set
                {
                    this.priceField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool priceSpecified
            {
                get
                {
                    return this.priceFieldSpecified;
                }
                set
                {
                    this.priceFieldSpecified = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class spoilerlogLocation
        {

            private string nameField;

            private string _Field;

            private byte priceField;

            private bool priceFieldSpecified;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string _
            {
                get
                {
                    return this._Field;
                }
                set
                {
                    this._Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte price
            {
                get
                {
                    return this.priceField;
                }
                set
                {
                    this.priceField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool priceSpecified
            {
                get
                {
                    return this.priceFieldSpecified;
                }
                set
                {
                    this.priceFieldSpecified = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class spoilerlogHint
        {

            private string locationField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string location
            {
                get
                {
                    return this.locationField;
                }
                set
                {
                    this.locationField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class spoilerlogLocation1
        {

            private string nameField;

            private string _Field;

            private ushort priceField;

            private bool priceFieldSpecified;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string _
            {
                get
                {
                    return this._Field;
                }
                set
                {
                    this._Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ushort price
            {
                get
                {
                    return this.priceField;
                }
                set
                {
                    this.priceField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool priceSpecified
            {
                get
                {
                    return this.priceFieldSpecified;
                }
                set
                {
                    this.priceFieldSpecified = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }


    }
}

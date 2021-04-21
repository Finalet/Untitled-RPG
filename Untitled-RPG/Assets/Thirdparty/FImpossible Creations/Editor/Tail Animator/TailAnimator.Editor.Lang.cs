using System.Collections;
using System.Xml;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class FTailAnimator2_Editor
    {
        private TextAsset langFile { get { if (_langFile == null) _langFile = Resources.Load("Tail Animator/TailAnimator_Langs") as TextAsset; return _langFile; } }
        private TextAsset _langFile;

        /// <summary> Readed langs from file </summary>
        private Hashtable langTexts;

        public enum ELangs { English, Polski, русский, 中文, 日本語, 한국어 }
        private static ELangs choosedLang = 0;


        // Setup langs ----------------------------------------
        private void SetupLangs()
        {
            // Checking if lang was setted through player prefs
            choosedLang = (ELangs)PlayerPrefs.GetInt("FimposLang", 0);

            if (langFile == null)
            {
                Debug.LogError("No lang file! You moved it from Editor/Resources/Tail Animator/ ???");
                return;
            }

            #region Generating lang hashtable

            var xml = new XmlDocument();
            xml.LoadXml(langFile.text);


            langTexts = new Hashtable();
            var element = xml.DocumentElement[choosedLang.ToString()];
            if (element != null)
            {

                var elemEnum = element.GetEnumerator();
                while (elemEnum.MoveNext())
                {
                    var xmlItem = (XmlElement)elemEnum.Current;
                    langTexts.Add(xmlItem.GetAttribute("name"), xmlItem.InnerText);
                }
            }
            else
            {
                Debug.LogError("The specified language does not exist: " + choosedLang.ToString());
            }

            #endregion
        }


        private string Lang(string title)
        {
            if (langTexts == null)
            {
                //Debug.Log("No Lang: [" + title + "]");
                return title;
            }

            if (!langTexts.ContainsKey(title))
            {
                return title;
            }

            string target = (string)langTexts[title];

            if (string.IsNullOrEmpty(target)) return title;

            return target;
        }


        private bool LangBig()
        {
            if (choosedLang == ELangs.中文 || choosedLang == ELangs.日本語) return true;
            return false;
        }
    }
}

using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace LilYeHelpers
{
    public class Helpers
    {
        public static IEnumerator Deactivate(GameObject gameObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }

        public static string SerializeDictStringInt(DictionaryOfStringAndInt dict)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, dict);
            return System.Convert.ToBase64String(stream.ToArray());
        }

        public static DictionaryOfStringAndInt DeserializeDictStringInt(string s)
        {
            IFormatter formatter = new BinaryFormatter();
            return (DictionaryOfStringAndInt)formatter.Deserialize(new MemoryStream(System.Convert.FromBase64String(s)));
        }

        public static DictionaryOfStringAndInt defaultProgress = new DictionaryOfStringAndInt() {
            { "main-menu", 1 },
            { "ghosts-level-1", 0 },
            { "ghosts-level-2", 0 },
            { "ghosts-level-3", 0 } };

        public static DictionaryOfStringAndInt GetUserProgess()
        {
            return DeserializeDictStringInt(CustomPlayerPrefs.GetString("Progress", SerializeDictStringInt(defaultProgress)));
        }

        public static void UpdateUserProgress(DictionaryOfStringAndInt progress)
        {
            CustomPlayerPrefs.SetString("Progress", SerializeDictStringInt(progress));
            CustomPlayerPrefs.Save();
        }

        public static void ChangeUserProgress(string level, int stars)
        {
            DictionaryOfStringAndInt progress = GetUserProgess();
            if (progress.ContainsKey(level))
                if (stars > progress[level])
                    progress[level] = stars;
            UpdateUserProgress(progress);
        }
    }

    public class CustomPlayerPrefs
    {
        static DictionaryOfStringAndObj prefs;

        static string filePath = Application.persistentDataPath + "//prefs.dat";

        public static void Load()
        {
            Debug.Log("Loaded PlayerPrefs");
            if (File.Exists(filePath))
            {
                IFormatter formatter = new BinaryFormatter();
                prefs = (DictionaryOfStringAndObj)formatter.Deserialize(new MemoryStream(System.Convert.FromBase64String(
                                                                                         File.ReadAllText(filePath))));
            }
            else
                prefs = new DictionaryOfStringAndObj();
        }

        public static void Save()
        {
            Debug.Log("Updated PlayerPrefs");
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, prefs);
            File.WriteAllText(filePath, System.Convert.ToBase64String(stream.ToArray()));
        }

        public static int GetInt(string key, int defaultVal)
        {
            Load();
            if (prefs.ContainsKey(key))
                return (int)prefs[key];
            else
                return defaultVal;
        }

        public static float GetFloat(string key, float defaultVal)
        {
            Load();
            if (prefs.ContainsKey(key))
                return (float)prefs[key];
            else
                return defaultVal;
        }

        public static string GetString(string key, string defaultVal)
        {
            Load();
            if (prefs.ContainsKey(key))
                return (string)prefs[key];
            else
                return defaultVal;
        }

        public static void SetInt(string key, int value)
        {
            prefs[key] = value;
            Save();
        }

        public static void SetFloat(string key, float value)
        {
            prefs[key] = value;
            Save();
        }

        public static void SetString(string key, string value)
        {
            prefs[key] = value;
            Save();
        }
    }

    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }

        public SerializableDictionary()
        { }

        public SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }

    [System.Serializable] public class DictionaryOfStringAndInt : SerializableDictionary<string, int>
    {
        public DictionaryOfStringAndInt()
        { }

        public DictionaryOfStringAndInt(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
    
    [System.Serializable]
    public class DictionaryOfStringAndObj : SerializableDictionary<string, object>
    {
        public DictionaryOfStringAndObj()
        { }

        public DictionaryOfStringAndObj(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
};


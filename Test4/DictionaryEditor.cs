using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DictionaryTools
{
    public class DictionaryEditor : IDictionaryEditor
    {

        private Dictionary<string, string> _dictionary { get; set; }
        private IDictionaryEditHistoryManager _historyManager;

        public DictionaryEditor(Dictionary<string,string> dictionary, IDictionaryEditHistoryManager historyManager)
        {
            _dictionary = dictionary;
            _historyManager = historyManager;
        }

        public Dictionary<string, string> LoadDictionary(Dictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
            _historyManager.Clear();
            _historyManager.Watch(dictionary);
            return _dictionary;
        }

        public Dictionary<string, string> GetDictionary()
        {
            return _dictionary;
        }

        public bool HasUnsavedChanges()
        {
            return _historyManager.HasUnsavedChanges();
        }

        public void EditValue(string key, string newValue)
        {
            ValidateEdit(key, newValue);
            EditSingleValueInternal(key, newValue);
        }

        public void ReplaceValueGlobally(string oldValue, string newValue)
        {
            ValidateGlobalReplacement(oldValue, newValue);
            ReplaceValueGloballyInternal(oldValue, newValue);
        }

        public Dictionary<string, string> LoadDictionary(string filepath)
        {
            ValidateLoad(filepath);
            var readDict = new Dictionary<string, string>();
            var allLines = File.ReadAllText(filepath).Split("\r\n");
            for (int i = 0; i < allLines.Length; i++)
            {
                var lineValues = allLines[i].Split('\t');
                Complain.If(lineValues.Count() != 2, Complaints.MUST_BE_TWO_VALUES_PER_LINE + allLines[i]);
                readDict.Add(lineValues[0], lineValues[1]);
            }
            _dictionary = readDict;
            return _dictionary;
        }

        public void SaveDictionary(string filepath)
        {
            ValidateSave();
            File.WriteAllText(filepath, string.Join("\r\n", _dictionary.Select(kv => $"{kv.Key}\t{kv.Value}").ToList()));
            _historyManager.RegisterSave();
        }

        private void ValidateSave()
        {
            Complain.IfNull(_dictionary);
        }

        private static void ValidateLoad(string filepath)
        {
            Complain.If(!File.Exists(filepath), Complaints.DICTIONARY_FILE_NOT_FOUND);
        }

        public void Undo()
        {
            _historyManager.Undo();
        }

        public void Redo()
        {
            _historyManager.Redo();
        }

        private void ValidateEdit(string key, string newValue)
        {
            Complain.IfNull(_dictionary);
            Complain.IfNull(key);
            Complain.IfNull(newValue);
            Complain.If(!_dictionary.ContainsKey(key), Complaints.KEY_NOT_FOUND);
        }

        private void ReplaceValueGloballyInternal(string oldValue, string newValue)
        {
            var editGroup = new DictionaryEditGroup(_dictionary) { };
            try
            {
                List<string> keys = KeysContaining(oldValue);
                foreach (var key in keys)
                {
                    var replaced = _dictionary[key].Replace(oldValue, newValue);
                    EditSingleValueInternal(key, newValue, editGroup);
                }
            }
            catch (Exception e)
            {
                // Undoing any edits so far to avoid the application of a partial global edit.
                editGroup.Undo();
                Debug.WriteLine(Complaints.PROBLEM_WITH_GLOBAL_REPLACEMENT + e.Message);
                return;
            }
            // If there is an error, this history entry is not added.
            _historyManager.Add(editGroup);
        }

        private List<string> KeysContaining(string oldValue)
        {
            return _dictionary.Where(kv => kv.Value.Contains(oldValue)).Select(kv => kv.Key).ToList();
        }

        private void ValidateGlobalReplacement(string oldValue, string newValue)
        {
            Complain.IfNull(_dictionary);
            Complain.IfNull(oldValue);
            Complain.IfNull(newValue);
            Complain.If(oldValue == newValue, Complaints.CANNOT_REPLACE_WITH_IDENTICAL_VALUE, By.ThrowingAnException);
        }

        private void EditSingleValueInternal(string key, string newValue)
        {
            _historyManager.Add(key, _dictionary[key], newValue);
            _dictionary[key] = newValue;
        }

        private void EditSingleValueInternal(string key, string newValue, DictionaryEditGroup dictionaryEditGroup)
        {
            dictionaryEditGroup.Add(key, _dictionary[key], newValue);
            _dictionary[key] = newValue;
        }
    }

    public class DictionaryEditGroup
    {
        public IList<DictionaryEdit> Collection;
        private IDictionary<string, string> _dictionary;

        public DictionaryEditGroup(IDictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
            Collection = new List<DictionaryEdit>() { };
        }

        public DictionaryEditGroup(IDictionary<string, string> dictionary, DictionaryEdit dictionaryEdit)
        {
            _dictionary = dictionary;
            Collection = new List<DictionaryEdit>() { };
            Collection.Add(dictionaryEdit);
        }

        public DictionaryEditGroup(IDictionary<string, string> dictionary, string key, string previousValue, string newValue)
        {
            _dictionary = dictionary;
            Collection = new List<DictionaryEdit>() { };
            Collection.Add(new DictionaryEdit(_dictionary, key, previousValue, newValue));
        }

        public DictionaryEditGroup Add(string key, string previousValue, string newValue)
        {
            Collection.Add(new DictionaryEdit(_dictionary, key, previousValue, newValue));
            return this;
        }

        public DictionaryEditGroup Add(DictionaryEdit dictionaryEdit)
        {
            Collection.Add(dictionaryEdit);
            return this;
        }

        public DictionaryEditGroup Undo()
        {
            foreach (var edit in Collection)
            {
                edit.Undo();
            }
            return this;
        }

        public DictionaryEditGroup Redo()
        {
            foreach (var edit in Collection)
            {
                edit.Apply();
            }
            return this;
        }

    }

    public class DictionaryEdit
    {

        private IDictionary<string, string> _dictionary;
        private string _keyAffected;
        private string _previousValue;
        private string _newValue;

        public DictionaryEdit(IDictionary<string, string> dictionary, string keyAffected, string previousValue, string newValue)
        {
            _dictionary = dictionary;
            _keyAffected = keyAffected;
            _previousValue = previousValue;
            _newValue = newValue;
        }

        public DictionaryEdit Undo()
        {
            _dictionary[_keyAffected] = _previousValue;
            return this;
        }

        public DictionaryEdit Apply()
        {
            _dictionary[_keyAffected] = _newValue;
            return this;
        }
    }
}

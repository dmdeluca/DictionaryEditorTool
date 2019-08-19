using KeyTranslation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;

namespace DictionaryTools
{
    public class DictionaryEditor : IDictionaryEditor
    {

        private IDictionary<string, string> _dictionary { get; set; }
        private List<DictionaryEditGroup> _dictionaryHistory;

        public DictionaryEditor(IDictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
            _dictionaryHistory = new List<DictionaryEditGroup>() { };
        }

        public DictionaryEditor()
        {
            _dictionary = null;
            _dictionaryHistory = new List<DictionaryEditGroup>() { };
        }

        public IDictionary<string, string> LoadDictionary(IDictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
            return _dictionary;
        }

        public IDictionary<string, string> GetDictionary()
        {
            return _dictionary;
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

        public IDictionary<string, string> LoadDictionary(string filepath)
        {
            ValidateLoad(filepath);
            var readDict = new Dictionary<string, string>();
            var allLines = File.ReadAllText(filepath).Split("\r\n");
            for (int i = 0; i < allLines.Length; i++)
            {
                var lineValues = allLines[i].Split('\t');
                Complain.If(lineValues.Count() != 2, Constants.Complaints.MUST_BE_TWO_VALUES_PER_LINE + allLines[i]);
                readDict.Add(lineValues[0], lineValues[1]);
            }
            _dictionary = readDict;
            return _dictionary;
        }

        public void SaveDictionary(string filepath)
        {
            ValidateSave();
            File.WriteAllText(filepath, string.Join("\r\n", _dictionary.Select(kv => $"{kv.Key}\t{kv.Value}").ToList()));
        }

        private void ValidateSave()
        {
            Complain.IfNull(_dictionary);
        }

        private static void ValidateLoad(string filepath)
        {
            Complain.If(!File.Exists(filepath), Constants.Complaints.DICTIONARY_FILE_NOT_FOUND);
        }

        public void Undo()
        {
            ValidateUndo();
            _dictionaryHistory.Remove(_dictionaryHistory.Last().Undo());
        }

        private void ValidateUndo()
        {
            Complain.If(!_dictionaryHistory.Any(), Constants.Complaints.NOTHING_TO_UNDO, By.ThrowingAnException);
        }

        private void ValidateEdit(string key, string newValue)
        {
            Complain.IfNull(_dictionary);
            Complain.IfNull(key);
            Complain.IfNull(newValue);
            Complain.If(!_dictionary.ContainsKey(key), Constants.Complaints.KEY_NOT_FOUND);
        }

        private void ReplaceValueGloballyInternal(string oldValue, string newValue)
        {
            var editGroup = new DictionaryEditGroup(_dictionary) { };
            try
            {
                var keys = _dictionary.Where(kv => kv.Value == oldValue).Select(kv => kv.Key).ToList();
                foreach (var key in keys)
                {
                    editGroup.Add(key, oldValue);
                    _dictionary[key] = newValue;
                }
            }
            catch (Exception e)
            {
                editGroup.Undo();
                Debug.WriteLine(Constants.Complaints.PROBLEM_WITH_GLOBAL_REPLACEMENT + e.Message);
                return;
            }
            _dictionaryHistory.Add(editGroup);
        }

        private void ValidateGlobalReplacement(string oldValue, string newValue)
        {
            Complain.IfNull(oldValue);
            Complain.IfNull(newValue);
            Complain.If(oldValue == newValue, Constants.Complaints.CANNOT_REPLACE_WITH_IDENTICAL_VALUE, By.ThrowingAnException);
        }

        private void EditSingleValueInternal(string key, string newValue)
        {
            _dictionaryHistory.Add(new DictionaryEditGroup(_dictionary).Add(key, _dictionary[key]));
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

        public DictionaryEditGroup Add(DictionaryEdit edit)
        {
            Collection.Add(edit);
            return this;
        }

        public DictionaryEditGroup Add(string keyAffected, string previousValue)
        {
            Collection.Add(new DictionaryEdit(_dictionary, keyAffected, previousValue));
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

    }

    public class DictionaryEdit
    {

        private IDictionary<string, string> _dictionary;
        private string _keyAffected;
        private string _previousValue;

        public DictionaryEdit(IDictionary<string, string> dictionary, string keyAffected, string previousValue)
        {
            _dictionary = dictionary;
            _keyAffected = keyAffected;
            _previousValue = previousValue;
        }

        public DictionaryEdit Undo()
        {
            _dictionary[_keyAffected] = _previousValue;
            return this;
        }
    }
}

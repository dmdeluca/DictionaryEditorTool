using DictionaryTools;
using KeyTranslation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TranslationTests.Utils;
using Xunit;

namespace TranslationTests
{
    public class DictionaryEditorTests
    {
        private IDictionaryEditor _dictionaryEditor;
        private IDictionary<string, string> _dictionary;

        public DictionaryEditorTests()
        {
            _dictionary = DictionaryTestUtils.GenerateTestDictionary(1000);
            _dictionaryEditor = new DictionaryEditor(_dictionary);
        }

        [Fact]
        public void EditValueSanity()
        {
            var key = "test_key";
            var value = "test_value";
            var newValue = "new_value";
            _dictionary.Add(key, value);
            _dictionaryEditor.EditValue(key, newValue);
            Assert.Equal(newValue, _dictionary[key]);
        }

        [Fact]
        public void EditMissingKeyThrowsException()
        {
            var key = "nonexistent123";
            var newValue = "new_value";
            var exception = Assert.Throws<Exception>(() => _dictionaryEditor.EditValue(key, newValue));
            Assert.Equal(Constants.Complaints.KEY_NOT_FOUND, exception.Message);
        }

        [Fact]
        public void EditNullNewValueThrowsException()
        {
            var key = "nonexistent123";
            string newValue = null;
            var value = "test_value";
            _dictionary.Add(key, value);
            var exception = Assert.Throws<Exception>(() => _dictionaryEditor.EditValue(key, newValue));
            Assert.Contains(Constants.Complaints.ARGUMENT_CANNOT_BE_NULL, exception.Message);
        }

        [Fact]
        public void EditNullKeyThrowsException()
        {
            string key = null;
            string newValue = "notnull";
            var exception = Assert.Throws<Exception>(() => _dictionaryEditor.EditValue(key, newValue));
            Assert.Contains(Constants.Complaints.ARGUMENT_CANNOT_BE_NULL, exception.Message);
        }

        [Fact]
        public void UndoSanityTest()
        {
            string key = "added_key";
            string value = "value";
            _dictionary.Add(key, value);
            string newValue = "newValue";
            _dictionaryEditor.EditValue(key, newValue);
            Assert.Equal(newValue, _dictionary[key]);
            _dictionaryEditor.Undo();
            Assert.Equal(value, _dictionary[key]);
        }

        [Fact]
        public void GlobalReplacementUndoSanityTest()
        {
            var oldValue = "value";
            var kvs = new List<(string, string)>{
                ("key1",oldValue),
                ("key2",oldValue),
                ("key3",oldValue)
            };
            var newValue = "newKeyValue";
            kvs.ForEach(kv => _dictionary.Add(kv.Item1, kv.Item2));

            _dictionaryEditor.ReplaceValueGlobally(oldValue, newValue);
            kvs.ForEach(kv => Assert.Equal(newValue, _dictionary[kv.Item1]));

            _dictionaryEditor.Undo();
            kvs.ForEach(kv => Assert.Equal(oldValue, _dictionary[kv.Item1]));
        }

        [Fact]
        public void SaveLoadSanityTest()
        {
            var filename = "dict.txt";
            _dictionaryEditor.SaveDictionary(filename);
            var loaded = _dictionaryEditor.LoadDictionary(filename);

            Assert.Equal(_dictionary.Count(), loaded.Count());
            foreach(var kv in loaded)
            {
                Assert.Contains(kv, _dictionary);
            }
        }

        [Fact]
        public void LoadMissingDictionaryThrowsException()
        {
            var exception = Assert.Throws<Exception>(() => _dictionaryEditor.LoadDictionary("missing.txt"));

            Assert.NotNull(exception);
            Assert.Equal(Constants.Complaints.DICTIONARY_FILE_NOT_FOUND, exception.Message);
        }
    }
}

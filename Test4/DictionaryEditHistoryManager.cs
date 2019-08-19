using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DictionaryTools
{
    public class DictionaryEditHistoryManager : IDictionaryEditHistoryManager
    {
        private Dictionary<string, string> _dictionary;
        private Stack<DictionaryEditGroup> _undoableEdits;
        private Stack<DictionaryEditGroup> _redoableEdits;
        private Stack<DictionaryEditGroup> _unsavedChanges;

        public DictionaryEditHistoryManager(Dictionary<string, string> dictionary)
        {
            _undoableEdits = new Stack<DictionaryEditGroup>() { };
            _redoableEdits = new Stack<DictionaryEditGroup>() { };
            _unsavedChanges = new Stack<DictionaryEditGroup>() { };
            _dictionary = dictionary;
        }

        public void Watch(Dictionary<string,string> dictionary)
        {
            _dictionary = dictionary;
        }
        public void Undo()
        {
            ValidateUndo();
            var edit = _undoableEdits.Peek().Undo();
            _undoableEdits.Pop();
            _unsavedChanges.Pop();
            _redoableEdits.Push(edit);
        }

        public void Redo()
        {
            ValidateRedo();
            var edit = _redoableEdits.Peek().Redo();
            _redoableEdits.Pop();
            _undoableEdits.Push(edit);
            _unsavedChanges.Push(edit);
        }

        public void Clear()
        {
            _redoableEdits.Clear();
            _undoableEdits.Clear();
            _unsavedChanges.Clear();
        }

        public void RegisterSave()
        {
            _unsavedChanges.Clear();
        }

        public bool HasUnsavedChanges()
        {
            return _unsavedChanges.Count() > 0;
        }

        public void Add(string key, string oldValue, string newValue)
        {
            var dictionaryEditGroup = new DictionaryEditGroup(_dictionary);
            dictionaryEditGroup.Add(new DictionaryEdit(_dictionary, key, oldValue, newValue));
            RegisterEdit(dictionaryEditGroup);
        }

        public void Add(DictionaryEdit dictionaryEdit)
        {
            var dictionaryEditGroup = new DictionaryEditGroup(_dictionary);
            dictionaryEditGroup.Add(dictionaryEdit);
            RegisterEdit(dictionaryEditGroup);
        }

        public void Add(DictionaryEditGroup dictionaryEditGroup)
        {
            RegisterEdit(dictionaryEditGroup);
        }

        private void RegisterEdit(DictionaryEditGroup dictionaryEditGroup)
        {
            _undoableEdits.Push(dictionaryEditGroup);
            _unsavedChanges.Push(dictionaryEditGroup);
        }

        private void ValidateUndo()
        {
            Complain.If(_undoableEdits.Count() == 0, Complaints.NOTHING_TO_UNDO);
        }

        private void ValidateRedo()
        {
            Complain.If(_redoableEdits.Count() == 0, Complaints.NOTHING_TO_REDO);
        }
    }
}

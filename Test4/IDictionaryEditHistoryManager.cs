using System.Collections.Generic;

namespace DictionaryTools
{
    public interface IDictionaryEditHistoryManager
    {
        void Add(DictionaryEdit dictionaryEdit);
        void Add(DictionaryEditGroup dictionaryEditGroup);
        void Add(string key, string oldValue, string newValue);
        void Watch(Dictionary<string, string> dictionary);
        void Clear();
        bool HasUnsavedChanges();
        void Redo();
        void RegisterSave();
        void Undo();
    }
}
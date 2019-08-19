using System.Collections.Generic;

namespace DictionaryTools
{
    public interface IDictionaryEditor
    {
        void EditValue(string key, string newValue);
        Dictionary<string, string> GetDictionary();
        Dictionary<string, string> LoadDictionary(Dictionary<string, string> dictionary);
        Dictionary<string, string> LoadDictionary(string filepath);
        void ReplaceValueGlobally(string oldValue, string newValue);
        void SaveDictionary(string filepath);
        void Undo();
        void Redo();
    }
}
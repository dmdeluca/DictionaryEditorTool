using System.Collections.Generic;

namespace DictionaryTools
{
    public interface IDictionaryEditor
    {
        void EditValue(string key, string newValue);
        IDictionary<string, string> GetDictionary();
        IDictionary<string, string> LoadDictionary(IDictionary<string, string> dictionary);
        IDictionary<string, string> LoadDictionary(string filepath);
        void ReplaceValueGlobally(string oldValue, string newValue);
        void SaveDictionary(string filepath);
        void Undo();
    }
}
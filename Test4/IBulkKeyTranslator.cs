using System.Collections.Generic;

namespace FriendlyLocals
{
    public interface IBulkKeyTranslator
    {
        Dictionary<string, string> TranslateKeys(Dictionary<string, string> originalKeyValues, string originalLanguage, string targetLanguage);
    }
}
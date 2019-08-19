using Google.Cloud.Translation.V2;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
namespace FriendlyLocals
{
    public class BulkKeyTranslator : IBulkKeyTranslator
    {
        private const char WORD_SEPARATOR = '\n';
        private TranslationClient _translationClient;

        public BulkKeyTranslator(TranslationModel model = TranslationModel.ServiceDefault)
        {
            _translationClient = TranslationClient.Create(model: model);
        }

        /// <summary>
        /// Map a set of keys to new values in a different language.
        /// </summary>
        /// <param name="originalKeyValues">A dictionary of key-value pairs whose values you would like to translate.</param>
        /// <param name="originalLanguage">The original language of the values in the key-value pairs.</param>
        /// <param name="targetLanguage">The language into which you would like to translate the values.</param>
        /// <returns>Returns a new dictionary in which the original keys are mapped to translated values.</returns>
        public Dictionary<string, string> TranslateKeys(Dictionary<string, string> originalKeyValues, string originalLanguage, string targetLanguage)
        {
            ValidateTranslation(originalKeyValues, originalLanguage, targetLanguage);
            var words = originalKeyValues.Values.ToList();
            var translations = TranslateWordListAsync(words, originalLanguage, targetLanguage, WORD_SEPARATOR);
            return MapLocalizationKeys(originalKeyValues, translations);
        }

        private void ValidateTranslation(Dictionary<string, string> originalKeyValues, string originalLanguage, string targetLanguage)
        {
            Complain.IfNull(originalKeyValues);
            Complain.IfStringIsNullOrWhitespace(originalLanguage);
            Complain.IfStringIsNullOrWhitespace(targetLanguage);
            Complain.If(!_translationClient.ListLanguages().Select(l => l.Code).ToList().Contains(originalLanguage), $"{Constants.ORIGINAL_LANGUAGE_CODE} '{originalLanguage}' is not in the list of {_translationClient.GetType().ToString()}'s translatable languages. Please double-check the code or try a different language.", By.ThrowingAnException);
            Complain.If(!_translationClient.ListLanguages().Select(l => l.Code).ToList().Contains(targetLanguage), $"Target language code '{targetLanguage}' is not in the list of {_translationClient.GetType().ToString()}'s translatable languages. Please double-check the code or try a different language.", By.ThrowingAnException);
        }

        private Dictionary<string, string> TranslateWordListAsync(List<string> originalWords, string originalLanguage, string targetLanguage, char separator)
        {
            // Asynchronous translation
            var translationTasks = new List<Task<TranslationResult>>() { };
            var characterLimit = 5000;
            string packet = "";

            for (int i = 0; i < originalWords.Count(); i++)
            {
                string word = originalWords[i];
                if (packet.Length + word.Length + 1 < characterLimit)
                {
                    packet += word + separator;
                }
                else
                {
                    translationTasks.Add(_translationClient.TranslateTextAsync(packet.Trim(), targetLanguage, originalLanguage));
                    packet = word + separator;
                }
            }
            // Send the last packet
            if (packet.Any())
            {
                translationTasks.Add(_translationClient.TranslateTextAsync(packet.Trim(), targetLanguage, originalLanguage));
            }

            // Assemble results
            Task.WaitAll(translationTasks.ToArray());
            return ExtractTranslations(originalWords, separator, translationTasks);
        }

        private static Dictionary<string, string> ExtractTranslations(List<string> originalWords, char separator, List<Task<TranslationResult>> translationTasks)
        {
            var returnDictionary = new Dictionary<string, string>();
            int originalWordIndex = 0;
            for (int taskIndex = 0; taskIndex < translationTasks.Count(); taskIndex++)
            {
                var translatedPacket = translationTasks[taskIndex].Result.TranslatedText.Split(separator);
                for (int packetIndex = 0; packetIndex < translatedPacket.Count(); packetIndex++)
                {
                    var originalWord = originalWords[originalWordIndex];
                    returnDictionary.Add(originalWord, translatedPacket[packetIndex]);
                    originalWordIndex++;
                }
            }
            return returnDictionary;
        }

        private Dictionary<string, string> MapLocalizationKeys(Dictionary<string, string> originalKeyValues, Dictionary<string, string> translations)
        {
            var mappings = new Dictionary<string, string>();
            foreach (var keyValuePair in originalKeyValues)
            {
                mappings.Add(keyValuePair.Key, translations[keyValuePair.Value]);
            }
            return mappings;
        }
    }
}
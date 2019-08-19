using FriendlyLocals;
using Google.Cloud.Translation.V2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TranslationTests.Utils;
using Xunit;

namespace TranslationTests
{
    public class BulkTranslatorTests
    {
        private Dictionary<string, string> _testOriginalKeyValues;
        private IBulkKeyTranslator _bulkKeyTranslator;

        public BulkTranslatorTests()
        {
            _testOriginalKeyValues = DictionaryTestUtils.GenerateTestDictionary(1000);
            _bulkKeyTranslator = new BulkKeyTranslator(TranslationModel.NeuralMachineTranslation);
        }

        [Fact]
        public void RegexRemoveReferencesTest()
        {
            var str = @"The Himalayas are inhabited by 52.7 million people,[5] and are spread across five countries: Bhutan, China, India, Nepal and Pakistan. The Hindu Kush range in Afghanistan[6] and Hkakabo Razi in Myanmar are normally not included, but they are both (with the addition of Bangladesh) part of the greater Hindu Kush Himalayan (HKH) river system;[7] some of the world's major rivers – the Indus, the Ganges and the Tsangpo-Brahmaputra – rise in the Himalayas, and their combined drainage basin is home to roughly 600 million people. The Himalayas have a profound effect on the climate of the region, helping to keep the monsoon rains on the Indian plain and limiting rainfall on the Tibetan plateau. The Himalayas have profoundly shaped the cultures of the Indian subcontinent, with many Himalayan peaks considered sacred in Hinduism and Buddhism.";
            var removed = str.RemoveWikipediaReferences();
            Assert.False(Regex.Match(removed, @"(\[\d+\])").Success);
        }

        [Fact]
        public void BulkTranslatorTest()
        {
            // Arrange
            var startLang = "en";
            var endLang = "es";

            // Act
            var startTime = DateTime.Now;
            var test = _bulkKeyTranslator.TranslateKeys(_testOriginalKeyValues, startLang, endLang);

            // Report
            var keysPerSecond = Math.Round(((float)test.Count()) / ((DateTime.Now - startTime).TotalSeconds), 2);
            File.AppendAllText("output.txt", $"\r\n[{DateTime.Now.ToLongTimeString()}] Translated {_testOriginalKeyValues.Count()} keys from {startLang} to {endLang} at {keysPerSecond} k/s.");
            test.Print("output.txt", _testOriginalKeyValues);

            // Assert
            Assert.Equal(_testOriginalKeyValues.Count(), test.Count());
            foreach(var keyValuePair in _testOriginalKeyValues)
                Assert.True(test.ContainsKey(keyValuePair.Key));
        }
    }
}

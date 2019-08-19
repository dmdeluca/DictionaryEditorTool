using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TranslationTests.Utils
{
    public class DictionaryTestUtils
    {
        public static Dictionary<string, string> ConstructDictionaryWithNumericKeys(List<string> words)
        {
            var dict = new Dictionary<string, string>();
            var random = new Random();
            var entries = Enumerable.Range(0, words.Count()).Select(j => ((1000 + j).ToString(), words[j % words.Count()].ToString())).ToList();
            foreach (var entry in entries)
            {
                dict.Add(entry.Item1, entry.Item2);
            }
            return dict;
        }

        public static List<string> ExtractAcceptableWords(string filename)
        {
            return new string(File
                .ReadAllText(filename)
                .ToLower()
                .RemoveWikipediaReferences()
                .Where(c => !char.IsPunctuation(c) || c == '-')
                .Select(c => char.IsWhiteSpace(c) ? ' ' : c)
                .ToArray())
                .Split(' ')
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(s => s.Trim())
                .Distinct()
                .ToList();
        }

        public static Dictionary<string, string> GenerateTestDictionary(int number)
        {
            var filename = @"C:\Users\David DeLuca\source\repos\Test4\TranslationTests\TextFile1.txt";
            List<string> words = ExtractAcceptableWords(filename);
            return ConstructDictionaryWithNumericKeys(words.Take(Math.Min(number, words.Count())).ToList());
        }
    }

    public static class WikiProcessing
    {
        public static string RemoveWikipediaReferences(this string s)
        {
            return Regex.Replace(s, @"(\[\d+\])", "");
        }
    }

    public static class DictionaryExtensions
    {
        public static void Print(this Dictionary<string, string> d, string filepath, Dictionary<string, string> compare = null)
        {
            foreach (var kv in d)
            {
                if (compare != null)
                    File.AppendAllText(filepath, $"\t{kv.Key} - {kv.Value} (mapped to {compare[kv.Key]})\r\n");
                else
                    File.AppendAllText(filepath, $"\t{kv.Key} - {kv.Value}\r\n");
            }
        }
    }


}

using System;
using System.Collections.Generic;
using System.Text;

namespace KeyTranslation
{
    public static class Constants
    {
        public const string ORIGINAL_LANGUAGE_CODE = "Original language code";
        public class Complaints
        {
            public const string DEFAULT = "Some necessary precondition wasn't met, and the developer didn't add a message to explain why.";
            public const string STRING_WAS_NULL_OR_WHITESPACE = "A string was null or whitespace when it shouldn't have been.";
            public const string ARGUMENT_CANNOT_BE_NULL = "Argument cannot be null.";
            public const string KEY_NOT_FOUND = "Key was not found in dictionary.";
            public const string NOTHING_TO_UNDO = "There is nothing to undo.";
            public const string PROBLEM_WITH_GLOBAL_REPLACEMENT = "There was a problem doing the global replacement operation. Any replacements have been reverted.";
            public const string CANNOT_REPLACE_WITH_IDENTICAL_VALUE = "Cannot replace with identical value.";
            public const string DICTIONARY_FILE_NOT_FOUND = "Dictionary file was not found.";
            public const string MUST_BE_TWO_VALUES_PER_LINE = "Error parsing line in saved dictionary. Parser was expecting a key and a value separated by a tab character but encountered: ";
        }
    }
}

using System;
using System.Diagnostics;

namespace Common
{
    public static class Complain
    {
        public static void If(bool condition, string message = Complaints.DEFAULT, By complaintType = By.ThrowingAnException)
        {
            if (condition)
            {
                switch (complaintType)
                {
                    case By.ThrowingAnException:
                        throw new Exception(message);
                    case By.LoggingADebugMessage:
                        Debug.WriteLine(message);
                        break;
                    case By.ConsoleOutput:
                    default:
                        Console.WriteLine(message);
                        break;
                }
            }
        }

        public static void IfNull(object input, string message = null, By complaintType = By.ThrowingAnException)
        {
            If(input == null, $"{Complaints.ARGUMENT_CANNOT_BE_NULL}", complaintType);
        }

        public static void IfStringIsNullOrWhitespace(string input, string message = Complaints.STRING_WAS_NULL_OR_WHITESPACE, By complaintType = By.ThrowingAnException)
        {
            If(string.IsNullOrWhiteSpace(input), message, complaintType);
        }
    }

    public enum By
    {
        ThrowingAnException,
        LoggingADebugMessage,
        ConsoleOutput
    }
}

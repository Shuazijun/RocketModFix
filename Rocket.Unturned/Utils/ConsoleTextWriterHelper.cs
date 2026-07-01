using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace Rocket.Unturned.Utils
{
    internal static class ConsoleTextWriterHelper
    {
        public static TextWriter Unwrap(TextWriter? writer)
        {
            if (writer == null)
            {
                return TextWriter.Null;
            }

            if (writer is FilteredConsoleWriter filtered)
            {
                return filtered;
            }

            Type type = writer.GetType();
            if (type.FullName == "System.IO.TextWriter+SyncTextWriter")
            {
                FieldInfo? inner = AccessTools.Field(type, "_out") ?? AccessTools.Field(type, "out");
                if (inner?.GetValue(writer) is TextWriter nested)
                {
                    return Unwrap(nested);
                }
            }

            return writer;
        }

        public static bool ContainsFilteredWriter(TextWriter? writer)
        {
            if (writer == null)
            {
                return false;
            }

            if (writer is FilteredConsoleWriter)
            {
                return true;
            }

            Type type = writer.GetType();
            if (type.FullName == "System.IO.TextWriter+SyncTextWriter")
            {
                FieldInfo? inner = AccessTools.Field(type, "_out") ?? AccessTools.Field(type, "out");
                if (inner?.GetValue(writer) is TextWriter nested)
                {
                    return ContainsFilteredWriter(nested);
                }
            }

            return false;
        }

        public static bool ReferencesWriter(TextWriter? outer, TextWriter? target)
        {
            if (outer == null || target == null)
            {
                return false;
            }

            if (ReferenceEquals(outer, target))
            {
                return true;
            }

            return ReferenceEquals(Unwrap(outer), target) || ReferenceEquals(Unwrap(outer), Unwrap(target));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Object
{
    public static class ObjectDataValidator
    {
        public static bool Validate(Stream stream, ISet<int> ids, ISet<int> properties, bool withLevelData)
        {
            var haveValidationErrors = false;
            var unknownRawcodes = new Dictionary<int, int>();
            var unknownPropertyRawcodes = new Dictionary<int, int>();
            var conflictingRawcodes = new Dictionary<int, int>();

            using var reader = new BinaryReader(stream);

            var version = reader.ReadInt32();
            if (version != 2)
            {
                throw new InvalidDataException();
            }

            var amountModified = reader.ReadInt32();
            for (var i = 0; i < amountModified; i++)
            {
                var id = reader.ReadInt32();
                if (!ids.Contains(id))
                {
                    if (unknownRawcodes.ContainsKey(id))
                    {
                        unknownRawcodes[id]++;
                    }
                    else
                    {
                        unknownRawcodes.Add(id, 1);
                    }

                    // Console.WriteLine($"(MODIFIED UNIT) Found unknown rawcode: {id.ToRawcode()}");
                    haveValidationErrors = true;
                }

                var unused = reader.ReadInt32();
                if (unused != 0)
                {
                    throw new Exception();
                }

                var modCount = reader.ReadInt32();
                for (var m = 0; m < modCount; m++)
                {
                    var modId = reader.ReadInt32();
                    if (!properties.Contains(modId))
                    {
                        if (unknownPropertyRawcodes.ContainsKey(modId))
                        {
                            unknownPropertyRawcodes[modId]++;
                        }
                        else
                        {
                            unknownPropertyRawcodes.Add(modId, 1);
                        }

                        // Console.WriteLine($"(MODIFIED UNIT PROPERTY) Found unknown rawcode: {modId.ToRawcode()}");
                        haveValidationErrors = true;
                    }

                    var modType = reader.ReadInt32();

                    if (withLevelData)
                    {
                        var level = reader.ReadInt32();
                        var pointer = reader.ReadInt32();
                    }

                    switch (modType)
                    {
                        case 0:
                            reader.ReadInt32();
                            break;
                        case 1:
                        case 2:
                            reader.ReadSingle();
                            break;
                        case 3:
                            reader.ReadChars();
                            break;
                        case 4:
                            reader.ReadBoolean();
                            break;
                        case 5:
                            reader.ReadChar();
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    var sanityCheck = reader.ReadInt32();
                    if (sanityCheck != 0 && sanityCheck != id)
                    {
                        throw new Exception();
                    }
                }
            }

            var amountNew = reader.ReadInt32();
            for (var i = 0; i < amountNew; i++)
            {
                var oldId = reader.ReadInt32();
                if (!ids.Contains(oldId))
                {
                    if (unknownRawcodes.ContainsKey(oldId))
                    {
                        unknownRawcodes[oldId]++;
                    }
                    else
                    {
                        unknownRawcodes.Add(oldId, 1);
                    }

                    // Console.WriteLine($"(NEW UNIT) Found unknown rawcode: {oldId.ToRawcode()}");
                    haveValidationErrors = true;
                }

                var newId = reader.ReadInt32();
                if (ids.Contains(newId))
                {
                    if (conflictingRawcodes.ContainsKey(newId))
                    {
                        conflictingRawcodes[newId]++;
                    }
                    else
                    {
                        conflictingRawcodes.Add(newId, 1);
                    }

                    // Console.WriteLine($"(NEW UNIT) Found conflicting known rawcode: {newId.ToRawcode()}");
                    haveValidationErrors = true;
                }

                var modCount = reader.ReadInt32();
                for (var m = 0; m < modCount; m++)
                {
                    var modId = reader.ReadInt32();
                    if (!properties.Contains(modId))
                    {
                        if (unknownPropertyRawcodes.ContainsKey(modId))
                        {
                            unknownPropertyRawcodes[modId]++;
                        }
                        else
                        {
                            unknownPropertyRawcodes.Add(modId, 1);
                        }

                        // Console.WriteLine($"(MODIFIED UNIT PROPERTY) Found unknown rawcode: {modId.ToRawcode()}");
                        haveValidationErrors = true;
                    }

                    var modType = reader.ReadInt32();

                    if (withLevelData)
                    {
                        var level = reader.ReadInt32();
                        var pointer = reader.ReadInt32();
                    }

                    switch (modType)
                    {
                        case 0:
                            reader.ReadInt32();
                            break;
                        case 1:
                        case 2:
                            reader.ReadSingle();
                            break;
                        case 3:
                            reader.ReadChars();
                            break;
                        case 4:
                            reader.ReadBoolean();
                            break;
                        case 5:
                            reader.ReadChar();
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    var sanityCheck = reader.ReadInt32();
                    if (sanityCheck != 0 && sanityCheck != oldId && sanityCheck != newId)
                    {
                        throw new Exception();
                    }
                }
            }

            if (haveValidationErrors)
            {
                if (unknownRawcodes.Any())
                {
                    Console.WriteLine("Found unknown rawcodes:");
                    foreach (var rawcode in unknownRawcodes)
                    {
                        Console.WriteLine($"  {rawcode.Key.ToRawcode()} ({rawcode.Value}x)");
                    }
                }

                if (unknownPropertyRawcodes.Any())
                {
                    Console.WriteLine("Found unknown property rawcodes:");
                    foreach (var rawcode in unknownPropertyRawcodes)
                    {
                        Console.WriteLine($"  {rawcode.Key.ToRawcode()} ({rawcode.Value}x)");
                    }
                }

                if (conflictingRawcodes.Any())
                {
                    Console.WriteLine("Found conflicting rawcodes:");
                    foreach (var rawcode in conflictingRawcodes)
                    {
                        Console.WriteLine($"  {rawcode.Key.ToRawcode()} ({rawcode.Value}x)");
                    }
                }
            }

            return !haveValidationErrors;
        }

        public static bool Downgrade(Stream input, Stream output, ISet<int> ids, ISet<int> properties, bool withLevelData)
        {
            using var reader = new BinaryReader(input);
            using var writer = new BinaryWriter(output, Encoding.UTF8, true);

            var version = reader.ReadInt32();
            if (version != 2)
            {
                throw new InvalidDataException();
            }

            writer.Write(version);

            var foundAnyIncompatibleProperty = false;
            bool IsPropertyCompatible(int id)
            {
                if (!properties.Contains(id))
                {
                    foundAnyIncompatibleProperty = true;
                    return false;
                }

                return true;
            }

            var amountModified = reader.ReadInt32();
            var amountModifiedOffset = output.Position;
            var amountModifiedDowngrade = 0;
            writer.Write(0);

            for (var i = 0; i < amountModified; i++)
            {
                var id = reader.ReadInt32();
                var isCompatible = ids.Contains(id);

                var unused = reader.ReadInt32();
                if (unused != 0)
                {
                    throw new Exception();
                }

                if (isCompatible)
                {
                    amountModifiedDowngrade++;

                    writer.Write(id);
                    writer.Write(0);
                }

                var modCount = reader.ReadInt32();
                var modCountOffset = output.Position;
                var modCountDowngrade = 0;
                if (isCompatible)
                {
                    writer.Write(0);
                }

                for (var m = 0; m < modCount; m++)
                {
                    var modId = reader.ReadInt32();
                    var isCompatibleProperty = isCompatible && IsPropertyCompatible(modId);

                    var modType = reader.ReadInt32();

                    if (isCompatibleProperty)
                    {
                        modCountDowngrade++;

                        writer.Write(modId);
                        writer.Write(modType);
                    }

                    if (withLevelData)
                    {
                        var level = reader.ReadInt32();
                        var pointer = reader.ReadInt32();

                        if (isCompatibleProperty)
                        {
                            writer.Write(level);
                            writer.Write(pointer);
                        }
                    }

                    switch (modType)
                    {
                        case 0:
                            var valueInt = reader.ReadInt32();
                            if (isCompatibleProperty)
                            {
                                writer.Write(valueInt);
                            }
                            break;
                        case 1:
                        case 2:
                            var valueFloat = reader.ReadSingle();
                            if (isCompatibleProperty)
                            {
                                writer.Write(valueFloat);
                            }
                            break;
                        case 3:
                            var valueString = reader.ReadChars();
                            if (isCompatibleProperty)
                            {
                                writer.WriteString(valueString);
                            }
                            break;
                        case 4:
                            var valueBool = reader.ReadBoolean();
                            if (isCompatibleProperty)
                            {
                                writer.Write(valueBool);
                            }
                            break;
                        case 5:
                            var valueChar = reader.ReadChar();
                            if (isCompatibleProperty)
                            {
                                writer.Write(valueChar);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    var sanityCheck = reader.ReadInt32();
                    if (isCompatibleProperty)
                    {
                        writer.Write(sanityCheck);
                    }
                }

                // Go back and write the final amount of modified properties.
                output.Position = modCountOffset;
                writer.Write(modCountDowngrade);
                output.Position = output.Length;
            }

            // Go back and write the final amount of modified objects.
            output.Position = amountModifiedOffset;
            writer.Write(amountModifiedDowngrade);
            output.Position = output.Length;



            var amountNew = reader.ReadInt32();
            var amountNewOffset = output.Position;
            var amountNewDowngrade = 0;
            writer.Write(0);

            for (var i = 0; i < amountNew; i++)
            {
                var oldId = reader.ReadInt32();
                var newId = reader.ReadInt32();
                var isCompatible = ids.Contains(oldId) && !ids.Contains(newId);

                if (isCompatible)
                {
                    amountNewDowngrade++;

                    writer.Write(oldId);
                    writer.Write(newId);
                }

                var modCount = reader.ReadInt32();
                var modCountOffset = output.Position;
                var modCountDowngrade = 0;
                if (isCompatible)
                {
                    writer.Write(0);
                }

                for (var m = 0; m < modCount; m++)
                {
                    var modId = reader.ReadInt32();
                    var isCompatibleProperty = isCompatible && IsPropertyCompatible(modId);

                    var modType = reader.ReadInt32();

                    if (isCompatibleProperty)
                    {
                        modCountDowngrade++;

                        writer.Write(modId);
                        writer.Write(modType);
                    }

                    if (withLevelData)
                    {
                        var level = reader.ReadInt32();
                        var pointer = reader.ReadInt32();

                        if (isCompatibleProperty)
                        {
                            writer.Write(level);
                            writer.Write(pointer);
                        }
                    }

                    switch (modType)
                    {
                        case 0:
                            var valueInt = reader.ReadInt32();
                            if (isCompatibleProperty)
                            {
                                writer.Write(valueInt);
                            }
                            break;
                        case 1:
                        case 2:
                            var valueFloat = reader.ReadSingle();
                            if (isCompatibleProperty)
                            {
                                writer.Write(valueFloat);
                            }
                            break;
                        case 3:
                            var valueString = reader.ReadChars();
                            if (isCompatibleProperty)
                            {
                                writer.WriteString(valueString);
                            }
                            break;
                        case 4:
                            var valueBool = reader.ReadBoolean();
                            if (isCompatibleProperty)
                            {
                                writer.Write(valueBool);
                            }
                            break;
                        case 5:
                            var valueChar = reader.ReadChar();
                            if (isCompatibleProperty)
                            {
                                writer.Write(valueChar);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    var sanityCheck = reader.ReadInt32();
                    if (isCompatibleProperty)
                    {
                        writer.Write(sanityCheck);
                    }
                }

                // Go back and write the final amount of modified properties.
                output.Position = modCountOffset;
                writer.Write(modCountDowngrade);
                output.Position = output.Length;
            }

            // Go back and write the final amount of new objects.
            output.Position = amountNewOffset;
            writer.Write(amountNewDowngrade);
            output.Position = output.Length;

            return amountModifiedDowngrade < amountModified || amountNewDowngrade < amountNew || foundAnyIncompatibleProperty;
        }

        private static string ToRawcode(this int value)
        {
            return new string(new[]
            {
                (char)(value & 0x000000FF),
                (char)((value & 0x0000FF00) >> 8),
                (char)((value & 0x00FF0000) >> 16),
                (char)((value & 0xFF000000) >> 24),
            });
        }
    }
}
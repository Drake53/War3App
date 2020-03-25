using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using War3Net.Common.Extensions;

namespace War3App.MapDowngrader
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
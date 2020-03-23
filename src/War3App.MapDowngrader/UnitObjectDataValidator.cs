using System;
using System.Collections.Generic;
using System.IO;

using War3Net.Build.Common;
using War3Net.Common.Extensions;

namespace War3App.MapDowngrader
{
    public static class UnitObjectDataValidator
    {
        public static bool TryValidate(Stream stream, GamePatch targetPatch)
        {
            try
            {
                return Validate(stream, targetPatch);
            }
            catch
            {
                return false;
            }
        }

        public static bool Validate(Stream stream, GamePatch targetPatch)
        {
            var haveValidationErrors = false;

            using var reader = new BinaryReader(stream);

            var targetKnownRawcodes = new HashSet<int>();
            var targetKnownPropertyRawcodes = new HashSet<int>();

            targetKnownRawcodes.UnionWith(UnitObjectDataProvider.GetRawcodes(targetPatch));
            targetKnownPropertyRawcodes.UnionWith(UnitObjectDataProvider.GetPropertyRawcodes(targetPatch));

            var version = reader.ReadInt32();
            if (version != 2)
            {
                throw new InvalidDataException();
            }

            var amountModified = reader.ReadInt32();
            for (var i = 0; i < amountModified; i++)
            {
                var id = reader.ReadInt32();
                if (!targetKnownRawcodes.Contains(id))
                {
                    Console.WriteLine($"(MODIFIED UNIT) Found unknown rawcode: {id.ToRawcode()}");
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
                    if (!targetKnownPropertyRawcodes.Contains(modId))
                    {
                        Console.WriteLine($"(MODIFIED UNIT PROPERTY) Found unknown rawcode: {modId.ToRawcode()}");
                        haveValidationErrors = true;
                    }

                    var modType = reader.ReadInt32();
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
                if (!targetKnownRawcodes.Contains(oldId))
                {
                    Console.WriteLine($"(NEW UNIT) Found unknown rawcode: {oldId.ToRawcode()}");
                    haveValidationErrors = true;
                }

                var newId = reader.ReadInt32();
                if (targetKnownRawcodes.Contains(newId))
                {
                    Console.WriteLine($"(NEW UNIT) Found conflicting known rawcode: {newId.ToRawcode()}");
                    haveValidationErrors = true;
                }

                var modCount = reader.ReadInt32();
                for (var m = 0; m < modCount; m++)
                {
                    var modId = reader.ReadInt32();
                    if (!targetKnownPropertyRawcodes.Contains(modId))
                    {
                        Console.WriteLine($"(MODIFIED UNIT PROPERTY) Found unknown rawcode: {modId.ToRawcode()}");
                        haveValidationErrors = true;
                    }

                    var modType = reader.ReadInt32();
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

            return !haveValidationErrors;
        }
    }
}
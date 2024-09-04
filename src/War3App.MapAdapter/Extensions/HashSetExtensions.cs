using System.Collections.Generic;
using System.IO;
using System.Linq;

using War3Net.Common.Extensions;
using War3Net.IO.Slk;

namespace War3App.MapAdapter.Extensions
{
    public static class HashSetExtensions
    {
        public static void AddItemsFromSylkTable(this HashSet<int> set, string sylkTableFilePath, params string[] knownKeyColumnNames)
        {
            using (var dataTableFileStream = File.OpenRead(sylkTableFilePath))
            {
                var table = new SylkParser().Parse(dataTableFileStream);
                var tableKeyColumn = knownKeyColumnNames.SelectMany(keyColumnName => table[keyColumnName]).Single();
                
                for (var row = 1; row <= table.Rows; row++)
                {
                    var key = (string)table[tableKeyColumn, row];
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    set.Add(key.FromRawcode());
                }
            }
        }
    }
}
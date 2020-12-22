/*
 *  Copyright 2020 Chosen Few Software
 *  This file is part of NuXtractor.
 *
 *  NuXtractor is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  NuXtractor is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with NuXtractor.  If not, see <https://www.gnu.org/licenses/>.
 */

using MightyStruct;
using MightyStruct.Runtime;

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NuXtractor
{
    public class FileFormats
    {
        private static Dictionary<string, IType> CachedTypes { get; }

        public static IType GetFormat(string name)
        {
            IType format;
            if (CachedTypes.ContainsKey(name))
            {
                format = CachedTypes[name];
            }
            else
            {
                using (var formatStream = File.OpenRead($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Mighty\\{name}.xml"))
                {
                    format = Parser.ParseFromStream(formatStream);
                    CachedTypes.Add(name, format);
                }
            }

            return format;
        }
    }
}

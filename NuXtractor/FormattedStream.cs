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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NuXtractor
{
    public class FormattedStream : IDisposable
    {
        private static Dictionary<string, IType> CachedTypes { get; }
        static FormattedStream()
        {
            CachedTypes = new Dictionary<string, IType>();
        }

        public string FormatName { get; }
        public Stream Stream { get; }

        public dynamic data { get; private set; }

        public FormattedStream(string formatName, Stream stream)
        {
            FormatName = formatName;
            Stream = stream;
        }

        public async Task LoadAsync()
        {
            IType format;
            if (CachedTypes.ContainsKey(FormatName))
            {
                format = CachedTypes[FormatName];
            }
            else
            {
                using (var formatStream = File.OpenRead($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Mighty\\{FormatName}.xml"))
                {
                    format = Parser.ParseFromStream(formatStream);
                    CachedTypes.Add(FormatName, format);
                }
            }

            data = await format.Resolve(new Context(new Segment(Stream)));
            await data.ParseAsync();

            await OnLoadAsync();
        }

        protected virtual Task OnLoadAsync()
        {
            return Task.CompletedTask;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stream.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}

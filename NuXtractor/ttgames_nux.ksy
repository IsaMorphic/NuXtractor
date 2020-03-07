#  Copyright 2020 Chosen Few Software
#  This file is part of NuXtractor.
#
#  NuXtractor is free software: you can redistribute it and/or modify
#  it under the terms of the GNU General Public License as published by
#  the Free Software Foundation, either version 3 of the License, or
#  (at your option) any later version.
#
#  NuXtractor is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with NuXtractor.  If not, see <https://www.gnu.org/licenses/>.

meta:
  id: nux_file
  file-extension: nux
  endian: le
seq:
  - id: main_header
    type: file_header
types:
  file_header: # contains data that relates to when certain sections of data in the file end
    seq:
      - id: unk000
        type: u4
      - id: string_offset
        type: u4
      - id: texture_offset # texture data starts 4160 bytes after this amount
        type: u4
      - id: material_offset
        type: u4
      - id: unk001
        type: u4
      - id: vertex_offset
        type: u4
      - id: model_offset
        type: u4
      - id: object_offset # string section ends this amount after the 64th byte in the file
        type: u4
  texture_index:
    seq:
      - id: data_offset
        type: u4
      - id: first_entry
        type: texture_entry
      - id: unk003
        type: u4
      - id: entries
        type: texture_entry
        repeat: until
        repeat-until: _.sizes_next.array.max > 1024
  texture_entry:
    seq:
      - id: offset # describes offset to start of next section of textures
        type: u4
      - id: sizes_next # an array of ints that describes the width of each texture in a section in order
        type: texture_sizes
  texture_sizes:
    seq:
      - id: array
        type: u4
        repeat: expr
        repeat-expr: 4
  texture_data:
    params:
      - id: length
        type: u4
      - id: zizes
        type: struct
    seq:
      - id: data
        size: length
    instances:
      sizes:
        value: zizes.as<texture_sizes>.array
  texture_array:
    seq:
      - id: first_texture
        type: texture_data(entries[1].offset - entries[0].offset, first_entry.sizes_next)
      - id: textures
        type: texture_data(entries[_index+2].offset - entries[_index+1].offset, entries[_index].sizes_next)
        repeat: expr
        repeat-expr: entries.size-2
      - id: last_texture
        type: texture_data(entries[entries.size-1].offset - entries[entries.size-2].offset, entries[entries.size-2].sizes_next)
    instances:
      first_entry:
        value: _root.texture_header.first_entry
      entries:
        value: _root.texture_header.entries
instances: 
  texture_header: # textures start here
    type: texture_index
    pos: main_header.texture_offset + 64
  textures:
    type: texture_array
    pos: 64 + main_header.texture_offset + 4096
    
    
        

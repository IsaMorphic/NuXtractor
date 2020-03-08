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
      - id: last_offset
        type: u4
      - id: unk002
        type: u4
      - id: entries
        type: texture_entry
        repeat: until
        repeat-until: _.offset > last_offset
  texture_entry:
    seq:
      - id: width
        type: u4
      - id: height
        type: u4
      - id: unk003
        type: u4
      - id: unk004
        type: u4
      - id: offset # describes offset to start of next section of textures
        type: u4
  texture_data:
    params:
      - id: entry
        type: struct
      - id: next_entry
        type: struct
    seq:
      - id: data
        size: -entry.as<texture_entry>.offset + [_parent.texture_header.last_offset, next_entry.as<texture_entry>.offset].min
    instances:
      width:
        value: entry.as<texture_entry>.width
      height: 
        value: entry.as<texture_entry>.height
instances: 
  texture_header: # textures start here
    type: texture_index
    pos: main_header.texture_offset + 64
  textures:
    type: texture_data(texture_header.entries[_index], texture_header.entries[_index + 1])
    repeat: expr
    repeat-expr: texture_header.entries.size-1
    pos: 64 + main_header.texture_offset + 4096

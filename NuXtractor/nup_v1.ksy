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
  id: nup_v1
  file-extension: nup
  endian: le
seq:
  - id: main_header
    type: file_header
types:
  file_header:
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
    - id: header
      type: texture_header
    - id: entries
      type: texture_entry
      repeat: expr
      repeat-expr: header.num_entries
    - id: padding
      size: header.data_offset - entries.size * 20
    - id: data
      type: texture_data(_index)
      repeat: expr
      repeat-expr: entries.size
  texture_header:
    seq:
      - id: data_offset
        type: u4
      - id: last_offset
        type: u4
      - id: num_entries
        type: u4
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
      - id: index
        type: s4
    seq:
      - id: data
        size: '(index < _parent.entries.size - 1 ? _parent.entries[index+1].offset : _parent.header.last_offset) - _parent.entries[index].offset'
    instances:
      width:
        value: _parent.entries[index].width
      height: 
        value: _parent.entries[index].height
instances: 
  textures:
    type: texture_index
    pos: main_header.texture_offset + 64
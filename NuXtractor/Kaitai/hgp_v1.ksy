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
  id: hgp_v1
  file-extension: hgp_v1
  endian: le
types:
  file_header:
    seq:
    - id: unk000
      type: u4
    - id: str_offset
      type: u4
    - id: tex_offset
      type: u4
    - id: unk001
      type: u4
    - id: unk002
      type: u4
    - id: mdl_offset
      type: u4
  texture_index:
    seq:
    - id: data_offset
      type: u4
    - id: unk000
      type: u4
    - id: num_entries
      type: u4
    - id: entries
      type: texture_entry
      repeat: expr
      repeat-expr: num_entries
    - id: padding
      size: data_offset - entries.size * 20
    - id: data
      type: texture_data(_index)
      repeat: expr
      repeat-expr: entries.size
  texture_entry:
    seq:
    - id: width
      type: u4
    - id: height
      type: u4
    - id: unk000
      type: u4
    - id: unk001
      type: u4
    - id: offset
      type: u4
  texture_data:
    params:
    - id: index
      type: s4
    seq:
    - id: data
      size: '((index + 1 > _parent.entries.size - 1) ? (_root.header.mdl_offset - (_root.header.tex_offset + _parent.data_offset + 0x0C)) : _parent.entries[index+1].offset) - _parent.entries[index].offset'
    instances:
      width:
        value: _parent.entries[index].width
      height:
        value: _parent.entries[index].height
seq:
- id: header
  type: file_header
instances:
  tex_index:
    pos: header.tex_offset + 0x30
    type: texture_index

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
  id: csc_gc
  file-extension: csc
  endian: be
seq:
  - id: header
    type: main_header
  - id: sections
    type: data_section
    repeat: eos
types:
  main_header:
    seq:
      - id: magic
        contents: "02UN"
      - id: unk000
        type: u4
        repeat: expr
        repeat-expr: 3
  texture_index:
    seq:
      - id: header
        type: texture_header
      - id: entries
        type: texture_entry
        repeat: expr
        repeat-expr: header.num_entries
      - id: padding
        size: '((header.num_entries * 60 + 20) & -32) - (header.num_entries * 60) + 4'
      - id: data
        type: texture_data(_index)
        repeat: expr
        repeat-expr: entries.size
  texture_header:
    seq:
      - id: num_entries
        type: u4
  texture_entry:
      seq:
        - id: unk001
          type: u2
        - id: unk002
          type: u2
        - id: width
          type: u2
        - id: height
          type: u2
        - id: type
          type: u2
        - id: num_mipmaps
          type: u2
        - id: offset
          type: u4
        - id: padding
          size: 44
  texture_data:
    params:
      - id: index
        type: s4
    seq:
      - id: levels
        size: '(width * height >> (_index * 2)) / (type == 9 ? 1 : 2)'
        repeat: expr
        repeat-expr: _parent.entries[index].num_mipmaps
      - id: palette
        size: '(type == 8 ? 16 : 256) * 2'
        if: 'type < 0x0E'
    instances:
      width:
        value: _parent.entries[index].width
      height: 
        value: _parent.entries[index].height
      type:
        value: _parent.entries[index].type
  data_section:
    seq:
      - id: type
        type: str
        encoding: ascii
        size: 4
      - id: length
        type: u4
      - id: data
        type: 
          switch-on: type
          cases:
            '"0TST"': texture_index
        size: length - 8


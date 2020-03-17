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
  id: gsc_ps2
  file-extension: gsc
  endian: le
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
        contents: "NU20"
      - id: unk000
        type: u4
        repeat: expr
        repeat-expr: 3
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
            '"TST0"': texture_index
        size: length - 8
  texture_index:
    seq:
      - id: header
        type: texture_header
      - id: entries
        type: texture_entry
        repeat: expr
        repeat-expr: header.num_entries
  texture_header:
    seq:
      - id: num_entries
        type: u4
      - id: unk000
        type: u4
  texture_entry:
    seq:
      - id: data_length
        type: u4
      - id: padding_length
        type: u4
      - id: padding
        size: padding_length
      - id: texture
        type: texture_data
        size: data_length
  texture_data:
    instances:
      raw:
        pos: 0
        size-eos: true
    seq:
      - id: width
        type: u2
      - id: unk001
        type: u2
      - id: height
        type: u2
      - id: unk002
        type: u2
      - id: unk003
        type: u4
      - id: flag
        type: u4
      - id: palette_length
        type: u4
      - id: palette
        type: texture_palette
        size: palette_length - 4
      - id: pixels_length
        type: u4
      - id: pixels
        type: texture_pixels
        size: pixels_length - 4
  texture_palette:
    seq:
      - id: garbage
        type: texture_garbage
      - id: colors
        type: color
        repeat: expr
        repeat-expr: '_parent.flag & 1 == 1 ? 256 : 16'
  color:
    seq:
      - id: r
        type: u1
      - id: g
        type: u1
      - id: b
        type: u1
      - id: a
        type: u1
  texture_pixels:
    seq:
      - id: garbage
        type: texture_garbage
      - id: data
        size: '_parent.flag & 1 == 1 ? _parent.width * _parent.height : _parent.width * _parent.height / 2'
  texture_garbage:
    seq:
      - id: unk004
        type: u4
        repeat: until
        repeat-until: _ == 134217728
      - id: unk005
        type: u4
        repeat: expr
        repeat-expr: 2
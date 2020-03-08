# NuXtractor

NuXtractor is a tool created for the purpose of extracing textures from Lego games.  Currently the only officially supported game is Lego Star Wars: The Video Game (PC and Xbox).  

NuXtractor can only be used via the command line (for now).  Although this makes it harder for the average user, it allows for powerful features such as bluk file processing (through the use of batch or bash scripts).  

### Extracting textures from PC files (.nup)

Open a command prompt (in the same directory as NuXtractor) and run the following command:

`nuxtractor -i <path to nup file here> -m DDS`

The tool will run and output the extracted textures to the same directory as the specified .nup file in a folder called `<filename here>.nup.textures`.  You may then browse through the textures and view them using any image viewer that supports the DDS (Direct Draw Surface) format.  

### Extracting textures from Xbox files (.nux)

Open a command prompt (in the same directory as NuXtractor) and run the following command:

`nuxtractor -i <path to nux file here> -m DXT1`

The tool will run and output the extracted textures to the same directory as the specified .nux file in a folder called `<filename here>.nux.textures`.  The folder will contain both .DXT1 (raw DXT1 data) files and .PNG (converted and viewable) files.  

### Compiling

To compile NuXtractor, make sure you have both the [.NET Core SDK](https://dotnet.microsoft.com/download) and [Kaitai Struct compiler](http://kaitai.io/#download) installed.  

Afterwards, navigate to the `./NuXtractor` subdirectory and run the following command:

`dotnet build -c Release`

Once you've done that the executable can be located in the `/bin` folder.  

### License

NuXtractor is licensed under the GPL and is free for everyone to use.  The only thing I ask of my users is to provide credit for any public use of textures extracted by the tool.  
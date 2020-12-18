# SVG Icon Compressor
This is for monochrome icons only. It removes everything and reduces the image to a single path object in the most compact form possible.

There are two outputs:

* Normal SVG file, monochrome with a single path object, data is formatted to be readable.
* Embed string, monochrome with a single path object, data is compressed as much as possible.

## Usage
```
dotnet SvgIconCompressor.dll [size] [source] [destination]
```

* [size]  Resize canvas to this size. Image will be centered.
* [source]  Optional. Source folder or file, default is current working directory.
* [target]  Optional. Default is '[source]/result'. If defined to be same as source, it will always create a subdirectory.

## License
[MIT](https://choosealicense.com/licenses/mit/)
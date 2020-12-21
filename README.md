# SVG Icon Compressor
This is for monochrome icons only. It removes everything and reduces the image to a single path object in the most compact form possible.

There are 3 outputs:

- Regular SVG file, monochrome, with a single path object.
- An HTML file containing the embedded variants:
-- Embedded SVG for HTML
-- Embedded SVG for CSS

## Examples on Output

### HTML Embedding
```html
<div>
    <svg viewBox="0 0 512 512"><path d="M430 256l-256 256-92-92 164-164-164-164 92-92z"/></svg>
</div>
```
### CSS Embedding
```css
div {
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 512 512'%3E%3Cpath d='M430 256l-256 256-92-92 164-164-164-164 92-92z'/%3E%3C/svg%3E");
}
```
This works in all major browsers, even in Internet Explorer. [More on that...](https://codepen.io/tigt/post/optimizing-svgs-in-data-uris)

## Usage
```batchfile
dotnet SvgIconCompressor.dll [size] [decimal-places] [source] [destination]
```

- [size] Optional. Resize canvas to this size. Image will be centered and scaled to fit. Default value is 512.
- [decimal-places] Optional. Vector scalars are rounded with specified number of decimal places. Default is 2.
- [source] Optional. Source folder or file. Default is current working directory.
- [target] Optional. Default is "[source]/result". If defined to be same as source, it will always create a subdirectory.

## License
[MIT](https://choosealicense.com/licenses/mit/)
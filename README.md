# Furthermark

> A premier Markdown editor for Windows 10 UWP

<a href='//www.microsoft.com/store/apps/9NZKTMNQD014?ocid=badge'><img src='https://assets.windowsphone.com/85864462-9c82-451e-9355-a3d5f874397a/English_get-it-from-MS_InvariantCulture_Default.png' alt='English badge' width='150'/></a>

[Official Website](http://furthermark.com/)

**NOTICE:** Furthermark is currently in early stages of development and bugs may occur.

Furthermark is a markdown editor written for Windows 10 UWP in Visual Basic.NET. Furthermark was designed to be simple to use, open, fast, and take advantage of the features available to the UWP platform.

Furthermark is a simple, single document editor allowing for an editing pane and a preview pane that renders the Markdown document to be seen in real-time.

Furthermark uses all native UWP controls, including the WebView control to render the preview.

## Features

* Live preview of the rendered document
* Basic syntax highlighting in the editor window
* Support for light and dark themes
* Ability to customize CSS for preview
* Customized font in the editor pane
* Ability to autosave document changes
* Status bar with word/character counts
* Ability to copy entire markdown document's raw HTML

## Known Issues

* Syntax highlighting in the editor pane sometimes gets "confused" and will get stuck highlighting everything as a header, code block, hyperlink, etc.
* Switching between themes and loading of themes is unstable
* Many functions of the web preview rely on specially-timed JavaScript calls; this has caused some instability
* Scrolling is not synced as well as it could be
* Right-clicking in the editor pane sometimes causes a formatting bar to appear, which should not happen

## Planned New Features

* Support tabs to allow multiple documents open at once
* Support multiple copy options
    * Rich formatting
    * Copy snippet as HTML
* Allow printing and exporting as PDF
* Support language syntax highlighting in preview
* Auto-bulleting lists

## Special Thanks

A special thanks to [Markdig](https://github.com/lunet-io/markdig) for use of their wonderful Markdown processor.

# Unity Universal File Browser

The goal of this project is to provide a collection of libraries and a wrapper to access the native file browser on all platforms compatible with Unity.

Supported platforms: 
- Windows: Supported
- Linux: Not Implemented
- MacOS: Not Implemented
- iOS: Not Implemented
- Android: Not Implemented
- UWP: Not Implemented
- WebGL: Not Implemented

Supported configurations: 
- Scripting Backend: Mono/IL2CPP
- API Compatibility Level: .NET Standard/.NET Framework
- Managed Stripping Level: Disabled/Low/Medium/High

Any help or suggestion is welcome.

[Download Package](https://github.com/Profy/Unity-UniversalFileBrowser/releases/download/v0.1.0/UniversalFileBrowser.v0.1.0.unitypackage)

## Features
- File dialog (single select, multi select, filters)
- Folder dialog (single select, multi select)
- Save dialog (overwrite, filters)
- Open file browser
- Open file with default program
- Start an external process

## Usage
#### File dialog
    string path = UniversalFileBrowser.SingleFileDialog("Title", directory: null, filters);
	UnityEngine.Debug.Log(path == null ? "Dialog cancelled" : path);
#### Folder dialog
    string path = UniversalFileBrowser.SingleFolderDialog("Title", directory: null);
	UnityEngine.Debug.Log(path == null ? "Dialog cancelled" : path);
#### Save dialog
    string path = UniversalFileBrowser.SingleFolderDialog("Title", directory: null, filters);
	UnityEngine.Debug.Log(path == null ? "Dialog cancelled" : path);
#### Filters
    Filter[] filters = new[]
    {
        new Filter("JPEG", "jpg", "jpeg" ),
        new Filter("PNG", "png" ),
        new Filter("All Files", "*" )
    };
#### File browser
    UniversalFileBrowser.Browser(directory: Application.dataPath);
#### Open file
    string path = UniversalFileBrowser.SingleFileDialog("Title", directory: null);
    UniversalFileBrowser.Open(path);
#### External process
    string path = UniversalFileBrowser.SingleFileDialog("Title", directory: null);
    UniversalFileBrowser.StartProcess(path);

## Screenshots

## Special thanks
- [gkngkc](https://github.com/gkngkc/UnityStandaloneFileBrowser)
- [Jehoel](https://github.com/Jehoel/ShellFileDialogs)
- [josh4364](https://github.com/josh4364/IL2cppStartProcess)

# find-similar-images
This console program, FSI, finds similar images in provided path(s),
and outputs results in JSON format.

The program is inspired and influenced by [difPy][difPy].

## Features
- Set various options through arguments.
- Recursive image file search in subdirectory.
- Two folder path comparision.
- Output in JSON formatted file.

## TODO
- Support rotation.

## How to Execute
⚠️ FSI requires **.NET 6.0 Runtime** installed to run.

```shell
./find-similar-images {PATH} [{SECOND_PATH}] [-r|--recursive {RECURSIVE}] [-c|comp {COMPARATOR}] [-t|--threshold {THRESHOLD}]
```
For example, if you want to compare images within a directory,
```shell
./find-similar-images ~/images
```
If you want to compare images between two directories with NCC comparator, do this:
```shell
./find-similar-images ~/images-1 ~/images-2 -c ncc
```

## Output
FSI outputs the result in JSON format and it will write the result to a file if you specify the file name,
or write to console.
The format of the JSON file is the following:
```json
{
    "comparator": "COMPARATOR",
    "threshold": "THRESHOLD",
    "result": {
        "IMAGE_FILE_PATH": {
            "name": "IMAGE_FILE_NAME",
            "path": "IMAGE_FILE_PATH",
            "similarImages": [
                {
                    "name": "IMAGE_FILE_NAME",
                    "path": "IMAGE_FILE_PATH",
                    "similarity": 0.1
                }
            ]
        },
        "IMAGE_FILE_PATH": {
            "name": "IMAGE_FILE_NAME",
            "path": "IMAGE_FILE_PATH",
            "similarImages": [
                {
                    "name": "IMAGE_FILE_NAME",
                    "path": "IMAGE_FILE_PATH",
                    "similarity": 0.2
                },
                {
                    "name": "IMAGE_FILE_NAME",
                    "path": "IMAGE_FILE_PATH",
                    "similarity": 1
                },
            ]
        }
    }
}
```

Here are a few things to note:
1. If computed similarity is below the threshold, it will not be written in the result.
That means if none of images are considered to be similar, the result is an empty array.
2. Higher similarity value does not necessarily means the images are considered similar.
Similarity is dependent on comparators. Also, the similarity range varies according to what comparator is used.

## Develop
### How to Build Release Version
```shell
dotnet build -c Release --runtime win-x64 --self-contained false
```

### Naming a Build Output Zip File
```text
find-similar-images-v{VERSION}-{OS-ARCH}.zip
```


[difPy]: https://github.com/elisemercury/Duplicate-Image-Finder

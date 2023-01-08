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

## Comparators
A comparator compares two images in its own algorithms and their behaviors vary significantly
that one may judge the two images are similar while another may not.
You can select which comparator to use through an argument,
and it is your strategy what to use considering the natures of the target images.

### MSE
`mse` compator calculates the mean squared error sum of pixels from images.
the bigger MSE is, the more the two images are different.

Cons of the comparator is that even if two images originate from a same image but
have been malformed such as skewed, adjustment of brightness or contrast,
they may be reckoned to be different.

### NCC
`ncc` comparator makes use of Normalized Cross Correlation to compare images.
It compares images on the basis concept that images have their own shape of frequency (of pixel values)
and if the two frequencies match well, that means the images of those frequencies are to be equal.
They are not vulnerable to image manipulation as long as
the frequencies of the images did not change, unlike mse compator.

However, since they only care about the shape of image frequency, they may judge one complete black image
and one complete white image to be equal, since their frequencies are flat.

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
            "absPath": "IMAGE_FILE_ABSOLUTE_PATH"
            "similarImages": [
                {
                    "name": "IMAGE_FILE_NAME",
                    "path": "IMAGE_FILE_PATH",
                    "absPath": "IMAGE_FILE_ABSOLUTE_PATH",
                    "similarity": 0.1
                }
            ]
        },
        "IMAGE_FILE_PATH": {
            "name": "IMAGE_FILE_NAME",
            "path": "IMAGE_FILE_PATH",
            "absPath": "IMAGE_FILE_ABSOLUTE_PATH"
            "similarImages": [
                {
                    "name": "IMAGE_FILE_NAME",
                    "path": "IMAGE_FILE_PATH",
                    "absPath": "IMAGE_FILE_ABSOLUTE_PATH",
                    "similarity": 0.2
                },
                {
                    "name": "IMAGE_FILE_NAME",
                    "path": "IMAGE_FILE_PATH",
                    "absPath": "IMAGE_FILE_ABSOLUTE_PATH",
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
2. Higher similarity value does not necessarily mean the images are considered similar.
Lower similarity may imply that the two images are deemed to be similar.
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

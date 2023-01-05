using OpenCvSharp;
using CommandLine;

public enum Thresholds {
    low,
    medium,
    high
}

class MainProgram {
    const string defaultComparator = "mse";
    private static string homeDirectory = string.Empty;

    static int Main(string[] args) {
        var parsedArgs = Parser.Default.ParseArguments<Arguments>(args);
        if (parsedArgs.Errors.Any()) {
            return -1;
        }
        homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var inputPath1 = parsedArgs.Value.Path1;
        var inputPath2 = parsedArgs.Value.Path2;
        var thresholdSpecifier = parsedArgs.Value.Threshold;
        var threshold = GetThresholds(thresholdSpecifier);
        var recursive = parsedArgs.Value.Recursive;
        var compSpecifier = parsedArgs.Value.Comp ?? defaultComparator;
        var comparator = InitComparator(compSpecifier);
        var outputter = new JsonOutputter(compSpecifier, thresholdSpecifier);
        var similarPairCnt = 0;

        var watch = System.Diagnostics.Stopwatch.StartNew();
        if (inputPath2 == null) {
            var path = ResolvePath(inputPath1);
            if (!Directory.Exists(path)) {
                Console.WriteLine($"Given path does not exist or cannot be accessed: {path}");
                return -1;
            }
            var imgPaths = ImgGetter.GetImgPaths(path, recursive ?? true);
            var exitFlag = ConsoleLogNumImgs(imgPaths.Length, path, true);
            if (exitFlag) {
                return 0;
            }

            Console.WriteLine($"Reading {imgPaths.Length} images in memory...");
            Mat[] imgs;
            try {
                imgs = ReadAndProcessImgs(imgPaths, comparator.ProcessImg);
            } catch {
                return -1;
            }

            var totalPairCnt = imgPaths.Length * (imgPaths.Length - 1) / 2;
            var progressBar = new ProgressBar("Comparing image pairs: ", totalPairCnt, 100);
            var computedPairCnt = 0;

            Console.WriteLine($"Comparing {imgPaths.Length} images...");
            for (int i = 0; i < imgPaths.Length; ++i) {
                var img1 = imgs[i];
                var img1AbsPath = imgPaths[i];
                var img1OriginalPath = GetOriginalPath(img1AbsPath, inputPath1, path);
                for (int j = i + 1; j < imgPaths.Length; ++j) {
                    var img2 = imgs[j];
                    var similarity = comparator.Compare(img1, img2);
                    if (comparator.IsSimilar(similarity, threshold)) {
                        var img2AbsPath = imgPaths[j];
                        var img2OriginalPath = GetOriginalPath(img2AbsPath, inputPath1, path);
                        outputter.AppendSimilarPair(img1OriginalPath, img2OriginalPath, img1AbsPath, img2AbsPath, similarity);
                        ++similarPairCnt;
                    }
                    progressBar.WriteProgress(++computedPairCnt, computedPairCnt == totalPairCnt);
                }
            }
        } else {
            var path1 = ResolvePath(inputPath1);
            var path2 = ResolvePath(inputPath2);
            var path1Exists = Directory.Exists(path1);
            var path2Exists = Directory.Exists(path2);
            if (!(path1Exists && path2Exists)) {
                if (!path1Exists) {
                    Console.WriteLine($"Given path does not exist or cannot be accessed: {inputPath1}");
                }
                if (!path2Exists) {
                    Console.WriteLine($"Given path does not exist or cannot be accessed: {inputPath2}");
                }
                return -1;
            }

            var imgPaths1 = ImgGetter.GetImgPaths(path1, recursive ?? true);
            var imgPaths2 = ImgGetter.GetImgPaths(path2, recursive ?? true);
            var exitFlag1 = ConsoleLogNumImgs(imgPaths1.Length, path1, false);
            var exitFlag2 = ConsoleLogNumImgs(imgPaths2.Length, path2, false);
            if (exitFlag1 || exitFlag2) {
                return 0;
            }

            Console.WriteLine($"Reading {imgPaths1.Length + imgPaths2.Length} images in memory...");
            Mat[] imgs1, imgs2;
            try {
                imgs1 = ReadAndProcessImgs(imgPaths1, comparator.ProcessImg);
                imgs2 = ReadAndProcessImgs(imgPaths2, comparator.ProcessImg);
            } catch {
                return -1;
            }

            var totalPairCnt = imgPaths1.Length * imgPaths2.Length;
            var progressBar = new ProgressBar("Comparing image pairs: ", totalPairCnt, 100);
            var computedPairCnt = 0;

            Console.WriteLine($"Comparing {imgs1.Length} and {imgs2.Length} images...");
            for (int i = 0; i < imgs1.Length; ++i) {
                var img1 = imgs1[i];
                var img1AbsPath = imgPaths1[i];
                var img1OriginalPath = GetOriginalPath(img1AbsPath, inputPath1, path1);
                for (int j = 0; j < imgs2.Length; ++j) {
                    var img2 = imgs2[j];
                    try {
                        var similarity = comparator.Compare(img1, img2);
                        if (comparator.IsSimilar(similarity, threshold)) {
                            var img2AbsPath = imgPaths2[j];
                            var img2OriginalPath = GetOriginalPath(img2AbsPath, inputPath2, path2);
                            outputter.AppendSimilarPair(img1OriginalPath, img2OriginalPath, img1AbsPath, img2AbsPath, similarity);
                            ++similarPairCnt;
                        }
                        progressBar.WriteProgress(++computedPairCnt, computedPairCnt == totalPairCnt);
                    } catch (Exception e) {
                        Console.WriteLine($"Comparing images failed: {imgPaths1[i]} {imgPaths2[j]}");
                        Console.WriteLine(e.ToString());
                        return -1;
                    }
                }
            }
        }

        watch.Stop();
        ProgressBar.ClearConsoleLastLine();
        Console.WriteLine();
        Console.WriteLine($"{FormatTimeSpan(watch.Elapsed)} elapsed.");
        Console.WriteLine($"Number of similar pairs: {similarPairCnt} pairs\n");
        if (parsedArgs.Value.Output != null) {
            string? resolvedOutputPath = null;
            try {
                resolvedOutputPath = ResolvePath(parsedArgs.Value.Output);
            } catch (Exception) {
                Console.WriteLine($"Could not resolve the given path: {parsedArgs.Value.Output}");
            }
            if (resolvedOutputPath != null && outputter.WriteResultToFile(resolvedOutputPath)) {
                Console.WriteLine($"Output file has been written at the path: {resolvedOutputPath}");
                return 0;
            }
        }
        outputter.WriteResultToConsole();
        return 0;
    }

    private static bool ConsoleLogNumImgs(int numImgs, string path, bool exitIfOneImg) {
        if (numImgs == 0) {
            Console.WriteLine($"No image exists in the given path: {path}");
            return true;
        }
        if (exitIfOneImg && numImgs == 1) {
            Console.WriteLine($"Single image file has been found in the given path: {path}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Read images located at imgPaths array and process the images.
    /// </summary>
    private static Mat[] ReadAndProcessImgs(string[] imgPaths, Func<Mat, Mat> processImg) {
        var imgs = new Mat[imgPaths.Length];
        for (int i = 0; i < imgPaths.Length; ++i) {
            try {
                imgs[i] = Cv2.ImRead(imgPaths[i], ImreadModes.Color);
                var tmp = processImg(imgs[i]);
                // Release right after process to reduce memory usage.
                imgs[i].Release();
                imgs[i] = tmp;
            } catch (Exception e) {
                Console.WriteLine($"Failed to read a image: {imgPaths[i]}");
                Console.WriteLine(e.ToString());
                throw;
            }
        }
        return imgs;
    }

    private static IComparator InitComparator(string compSpecifier) {
        if (compSpecifier == "ncc") {
            return new NccComparator();
        }
        return new MseComparator();
    }

    private static Thresholds GetThresholds(string thresholdSpecifier) {
        var thresholdMap = new Dictionary<string, Thresholds>(){
            {"high", Thresholds.high},
            {"medium", Thresholds.medium},
            {"low", Thresholds.low}
        };
        if (!thresholdMap.ContainsKey(thresholdSpecifier)) {
            Console.WriteLine($"Threshold argument is '{thresholdSpecifier}' which is not valid. Using 'high' instead.");
            return Thresholds.high;
        }
        return thresholdMap[thresholdSpecifier];
    }

    private static string FormatTimeSpan(TimeSpan timeSpan) {
        var milliSeconds = timeSpan.TotalMilliseconds;
        if (milliSeconds > 1e3) {
            return $"{milliSeconds / 1e3:0.0} seconds";
        }
        return $"{milliSeconds} milliseconds";
    }

    /// <summary>
    /// Resolve the given path to full path, replacing '~' if given at the start with user home directory.
    /// </summary>
    public static string ResolvePath(string path) {
        if (path.Length > 0 && path[0] == '~') {
            //Resolve tilde path to current user home directory.
            return string.Concat(homeDirectory, path.AsSpan(1));
        }
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Convert the resolved path relative to input path.
    /// That is, GetOriginalPath(ResolvePath(path), path) = path
    /// Also, GetOriginalPath(ResolvePath(path)+'/img.png', path) = path+'/img.png'
    /// </summary>
    public static string GetOriginalPath(string resolvedPath, string inputPath, string resolvedInputPath) {
        return resolvedPath.Replace(resolvedInputPath, inputPath);
    }

    class Arguments {
        [Value(0, Required = true, MetaName = "Path1", HelpText = "Target Path. If you specify one paths, compare images in the same directory.")]
        public string Path1 { get; set; } = string.Empty;

        [Value(1, Required = false, MetaName = "Path2", HelpText = "Target Path. If you specify two paths, compare images to those from the other directories.")]
        public string? Path2 { get; set; }

        // Define the bool nullable so that it can be true by default.
        // https://github.com/commandlineparser/commandline/wiki/CommandLine-Grammar#named-option-types
        [Option('r', "recursive", Default = true, Required = false, HelpText = "Include images in subdirectories.")]
        public bool? Recursive { get; set; }

        [Option('c', "comp", Default = defaultComparator, Required = false, HelpText = "Specify comparator. Possible values: mse, ncc.")]
        public string? Comp { get; set; }

        [Option('t', "threshold", Default = "high", Required = false, HelpText = "Specify similarity threshold. Higher threshold compares images more strictly. Possible values: high, medium, low.")]
        public string Threshold { get; set; } = string.Empty;

        [Option('o', "output", Default = null, Required = false, HelpText = "Specify output file name. Write to console if not given.")]
        public string? Output { get; set; }
    }
}

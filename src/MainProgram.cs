using OpenCvSharp;
using CommandLine;

public enum Thresholds {
    low,
    medium,
    high
}

class MainProgram {
    const string defaultComparator = "mse";
    static int Main(string[] args) {
        var parsedArgs = Parser.Default.ParseArguments<Arguments>(args);
        if (parsedArgs.Errors.Any()) {
            return -1;
        }
        var path1 = parsedArgs.Value.Path1;
        var path2 = parsedArgs.Value.Path2;
        var threshold = GetThresholds(parsedArgs.Value.Threshold);
        var comparator = InitComparator(parsedArgs.Value.Comp ?? defaultComparator);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        if (path2 == null) {
            var path = path1;
            if (!Directory.Exists(path)) {
                Console.WriteLine($"Given path does not exist or cannot be accessed: {path}");
                return -1;
            }
            var imgPaths = ImgGetter.GetImgPaths(path);
            var exitFlag = ConsoleLogNumImgs(imgPaths.Length, path);
            if (exitFlag) {
                return 0;
            }

            Console.WriteLine($"Reading {imgPaths.Length} images in memory...");
            var imgs = new Mat[imgPaths.Length];
            for (int i = 0; i < imgPaths.Length; ++i) {
                try {
                    imgs[i] = Cv2.ImRead(imgPaths[i], ImreadModes.Color);
                } catch (Exception e) {
                    Console.WriteLine($"Failed to read a image: {imgPaths[i]}");
                    Console.WriteLine(e.ToString());
                    return -1;
                }
            }

            Console.WriteLine($"Comparing {imgPaths.Length} images...");
            for (int i = 0; i < imgPaths.Length; ++i) {
                var img1 = imgs[i];
                for (int j = i + 1; j < imgPaths.Length; ++j) {
                    var img2 = imgs[j];
                    var similarity = comparator.Compare(img1, img2);
                    if (comparator.IsSimilar(similarity, threshold)) {
                        Console.WriteLine($"{imgPaths[i]} {imgPaths[j]} {similarity}");
                    }
                }
            }
        }

        watch.Stop();
        Console.WriteLine($"{watch.ElapsedMilliseconds} ms elapsed.");
        return 0;
    }

    private static bool ConsoleLogNumImgs(int numImgs, string path) {
        if (numImgs == 0) {
            Console.WriteLine($"No image exists in the given path: {path}");
            return true;
        }
        if (numImgs == 1) {
            Console.WriteLine($"Single image file has been found in the given path: {path}");
            return true;
        }
        return false;
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
    }
}

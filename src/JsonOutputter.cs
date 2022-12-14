using System.Text.Json;
using System.Text.Unicode;

class JsonOutputter {
    // In each class to be serialized, if not set getter, the property will not be serialized.
    class Output {
        public string Comparator { get; set; } = string.Empty;
        public string Threshold { get; set; } = string.Empty;
        public Dictionary<string, Image> Result { get; set; } = new();
    }

    class Image {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string AbsPath { get; set; } = string.Empty;
        public List<SimilarImage> SimilarImages { get; set; } = new();
        public Image(string name, string path, string absPath, SimilarImage similarImage) {
            Name = name;
            Path = path;
            AbsPath = absPath;
            SimilarImages.Add(similarImage);
        }
    }

    class SimilarImage {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string AbsPath { get; set; } = string.Empty;
        public double Similarity { get; set; }
        public SimilarImage(string name, string path, string absPath, double similarity) {
            Name = name;
            Path = path;
            AbsPath = absPath;
            Similarity = similarity;
        }
    }

    private readonly JsonSerializerOptions serializerOptions;
    private readonly Output MyOutput = new();

    public JsonOutputter(string comparator, string threshold) {
        MyOutput.Comparator = comparator;
        MyOutput.Threshold = threshold;
        serializerOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            // Unescape non-ASCII characters.
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All),
        };
    }

    public void AppendSimilarPair(string imgPath1, string imgPath2, string absImgPath1, string absImgPath2, double similarity) {
        imgPath1 = imgPath1.Replace('\\', '/');
        imgPath2 = imgPath2.Replace('\\', '/');
        absImgPath1 = absImgPath1.Replace('\\', '/');
        absImgPath2 = absImgPath2.Replace('\\', '/');
        var similarImg = new SimilarImage(Path.GetFileName(imgPath2), imgPath2, absImgPath2, similarity);
        if (MyOutput.Result.ContainsKey(imgPath1)) {
            MyOutput.Result[imgPath1].SimilarImages.Add(similarImg);
        } else {
            var img = new Image(Path.GetFileName(imgPath1), imgPath1, absImgPath1, similarImg);
            MyOutput.Result.Add(imgPath1, img);
        }
    }

    /// <summary>
    /// Write the output to console.
    /// </summary>
    public void WriteResultToConsole() {
        var serializedOutput = JsonSerializer.Serialize(MyOutput, serializerOptions);
        Console.WriteLine(serializedOutput);
    }

    /// <summary>
    /// Write the output to the given path.
    /// Returns a bool whether write process finished successfully or not.
    /// </summary>
    public bool WriteResultToFile(string path) {
        if (File.Exists(path) || Directory.Exists(path)) {
            Console.WriteLine($"Output file already exists, cannot overwrite: {path}");
            return false;
        }
        try {
            var serializedOutput = JsonSerializer.Serialize(MyOutput, serializerOptions);
            File.WriteAllText(path, serializedOutput);
            return true;
        } catch (Exception e) {
            Console.WriteLine($"Could not write the output at the path: {path}");
            Console.WriteLine(e.ToString());
            return false;
        }
    }
}

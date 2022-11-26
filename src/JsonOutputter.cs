using System.Text.Json;

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
        public List<SimilarImage> SimilarImages { get; set; } = new();
        public Image(string name, string path, SimilarImage similarImage) {
            Name = name;
            Path = path;
            SimilarImages.Add(similarImage);
        }
    }

    class SimilarImage {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public double Similarity { get; set; }
        public SimilarImage(string name, string path, double similarity) {
            Name = name;
            Path = path;
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
            WriteIndented = true
        };
    }

    public void AppendSimilarPair(string imgPath1, string imgPath2, double similarity) {
        var similarImg = new SimilarImage(Path.GetFileName(imgPath2), imgPath2, similarity);
        if (MyOutput.Result.ContainsKey(imgPath1)) {
            MyOutput.Result[imgPath1].SimilarImages.Add(similarImg);
        } else {
            var img = new Image(Path.GetFileName(imgPath1), imgPath1, similarImg);
            MyOutput.Result.Add(imgPath1, img);
        }
    }

    /// <Summary>
    /// Write the output to the given path.
    /// Returns a bool whether write process finished successfully or not.
    /// </Summary>
    public bool WriteSerializedResult(string path) {
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

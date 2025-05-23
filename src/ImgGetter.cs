namespace FSI;

public class ImgGetter {
    private static readonly string[] imgExts = {
        "jpeg", "jpg", "jfif", "png", "webp", "bmp", "tiff", "tif", "avif",
    };

    public static string[] FilterImgPaths(string[] filePaths) {
        var imgPaths = filePaths
            .Where(path => imgExts.Any(ext => Path.GetExtension(path)?.ToLower() == "." + ext))
            .ToArray();
        return imgPaths;
    }

    public static string[] GetImgPaths(string path, bool recursive = true) {
        try {
            var filePaths = Directory.GetFiles(path);
            var imgPaths = FilterImgPaths(filePaths);
            if (!recursive) {
                return imgPaths;
            }
            var dirPaths = Directory.GetDirectories(path);
            var tmpImgPaths = new List<string>(imgPaths);
            foreach (var dirPath in dirPaths) {
                tmpImgPaths.AddRange(GetImgPaths(dirPath, true));
            }
            return tmpImgPaths.ToArray();
        } catch {
            return Array.Empty<string>();
        }
    }
}

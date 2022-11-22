class ImgGetter {
    private static readonly string[] imgExts = {
        "jpeg", "jpg", "jfif", "png", "webp", "bmp"
    };

    private static string[] FilterImgPaths(string[] filePaths) {
        var imgPaths = filePaths.Where(path => imgExts.Any(ext => path.EndsWith("." + ext))).ToArray();
        return imgPaths;
    }

    public static string[] GetImgPaths(string path) {
        try {
            var filePaths = Directory.GetFiles(path);
            var imgPaths = FilterImgPaths(filePaths);
            return imgPaths;
        } catch {
            return Array.Empty<string>();
        }
    }

}

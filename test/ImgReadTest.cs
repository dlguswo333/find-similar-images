namespace test;

public class ImgReadTest
{
    private string TestImgFolderPath {
        get {
            var cwd = Directory.GetCurrentDirectory();
            return $"{cwd[..(cwd.LastIndexOf("find-similar-images") - 1)]}/find-similar-images/test/images/";
        }
    }

    [Fact]
    public void FilterImgPaths1()
    {
        string[] inputs = ["1.jpeg", "2.jpg", "3.jfif", "4.png", "5.bmp", "6.tiff", "7.tif", "8.avif"];
        var results = FSI.ImgGetter.FilterImgPaths(inputs);
        Assert.Equal(inputs, results);
    }

    [Fact]
    public void FilterImgPaths2()
    {
        string[] inputs = ["1.txt", "2.jpg.txt", "3.avif.", "4"];
        var results = FSI.ImgGetter.FilterImgPaths(inputs);
        Assert.Empty(results);
    }

    [Fact]
    public void ReadImg1()
    {
        var jpgPath = Path.Join([TestImgFolderPath, "shapes0.jpg"]);
        Assert.True(File.Exists(jpgPath));
        // [TODO] Rather than calling cv2 directly call from FSI.
        var results = OpenCvSharp.Cv2.ImRead(jpgPath, OpenCvSharp.ImreadModes.Color);
        Assert.Equal(800, results.Width);
        Assert.Equal(800, results.Height);
    }

    [Fact]
    public void ReadImg2()
    {
        var avifPath = Path.Join([TestImgFolderPath, "shapes0.avif"]);
        Assert.True(File.Exists(avifPath));
        // [TODO] Rather than calling cv2 directly call from FSI.
        var results = OpenCvSharp.Cv2.ImRead(avifPath, OpenCvSharp.ImreadModes.Color);
        Assert.Equal(800, results.Width);
        Assert.Equal(800, results.Height);
    }
}

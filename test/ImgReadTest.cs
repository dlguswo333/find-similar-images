namespace test;

public class ImgReadTest
{
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
}

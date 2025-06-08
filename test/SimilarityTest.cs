namespace test;

public class SimilarityTest
{
    private static string TestImgFolderPath {
        get {
            var cwd = Directory.GetCurrentDirectory();
            return $"{cwd[..(cwd.LastIndexOf("find-similar-images") - 1)]}/find-similar-images/test/images/";
        }
    }

    private static void CompareIdenticalImgs(FSI.IComparator comparator) {
        var img1Path = Path.Join([TestImgFolderPath, "shapes0.jpg"]);
        var img2Path = Path.Join([TestImgFolderPath, "shapes0.avif"]);
        Assert.True(File.Exists(img1Path));
        Assert.True(File.Exists(img2Path));

        var imgs = FSI.MainProgram.ReadAndProcessImgs([img1Path, img2Path], comparator.ProcessImg);
        var similarity = comparator.Compare(imgs[0], imgs[1]);
        Assert.True(comparator.IsSimilar(similarity, FSI.Thresholds.high));
    }

    private static void CompareDifferentImgs(FSI.IComparator comparator) {
        var img1Path = Path.Join([TestImgFolderPath, "shapes0.jpg"]);
        var img2Path = Path.Join([TestImgFolderPath, "shapes1.jpg"]);
        Assert.True(File.Exists(img1Path));
        Assert.True(File.Exists(img2Path));

        var imgs = FSI.MainProgram.ReadAndProcessImgs([img1Path, img2Path], comparator.ProcessImg);
        var similarity = comparator.Compare(imgs[0], imgs[1]);
        Assert.False(comparator.IsSimilar(similarity, FSI.Thresholds.low));
    }

    [Fact]
    public void TestMseComparator() {
        var comparator = new FSI.MseComparator();
        CompareIdenticalImgs(comparator);
        CompareDifferentImgs(comparator);
    }

    [Fact]
    public void TestNccComparator() {
        var comparator = new FSI.NccComparator();
        CompareIdenticalImgs(comparator);
        CompareDifferentImgs(comparator);
    }
}

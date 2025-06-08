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
        var jpgPath = Path.Join([TestImgFolderPath, "foxes.jpg"]);
        var avifPath = Path.Join([TestImgFolderPath, "foxes.avif"]);
        Assert.True(File.Exists(jpgPath));
        Assert.True(File.Exists(avifPath));

        var imgs = FSI.MainProgram.ReadAndProcessImgs([jpgPath, avifPath], comparator.ProcessImg);
        var similarity = comparator.Compare(imgs[0], imgs[1]);
        Assert.True(comparator.IsSimilar(similarity, FSI.Thresholds.high));
    }

    [Fact]
    public void TestMseComparator() {
        var comparator = new FSI.MseComparator();
        CompareIdenticalImgs(comparator);
    }

    [Fact]
    public void TestNccComparator() {
        var comparator = new FSI.NccComparator();
        CompareIdenticalImgs(comparator);
    }
}

using OpenCvSharp;


public class NccComparator : IComparator {
    /// <summary>
    /// Compare images in normalized cross correlation, averaging three BGR NCC.
    /// The result value ranges 0~1 and high value means high similarity.
    /// </summary>
    public NccComparator(int imgResizeValue = 64) {
        ImgResizeValue = imgResizeValue;
    }

    private int ImgResizeValue { get; }
    private readonly double HighSimilarity = 0.99;
    private readonly double MediumSimilarity = 0.97;
    private readonly double LowSimilarity = 0.9;

    public double Compare(Mat img1, Mat img2) {
        var pImg1 = ProcessImg(img1);
        var pImg2 = ProcessImg(img2);
        // Refer https://github.com/shimat/opencvsharp/wiki/Accessing-Pixel
        var indexer1 = pImg1.GetGenericIndexer<Vec3b>();
        var indexer2 = pImg2.GetGenericIndexer<Vec3b>();
        var ncc = new double[3];
        var img1SquaredSum = new long[3];
        var img2SquaredSum = new long[3];
        for (int i = 0; i < ImgResizeValue; ++i) {
            for (int j = 0; j < ImgResizeValue; ++j) {
                ncc[0] += indexer1[i, j][0] * indexer2[i, j][0];
                ncc[1] += indexer1[i, j][1] * indexer2[i, j][1];
                ncc[2] += indexer1[i, j][2] * indexer2[i, j][2];
                img1SquaredSum[0] += indexer1[i, j][0] * indexer1[i, j][0];
                img1SquaredSum[1] += indexer1[i, j][1] * indexer1[i, j][1];
                img1SquaredSum[2] += indexer1[i, j][2] * indexer1[i, j][2];
                img2SquaredSum[0] += indexer2[i, j][0] * indexer2[i, j][0];
                img2SquaredSum[1] += indexer2[i, j][1] * indexer2[i, j][1];
                img2SquaredSum[2] += indexer2[i, j][2] * indexer2[i, j][2];
            }
        }
        ncc[0] /= Math.Sqrt(img1SquaredSum[0]) * Math.Sqrt(img2SquaredSum[0]);
        ncc[1] /= Math.Sqrt(img1SquaredSum[1]) * Math.Sqrt(img2SquaredSum[1]);
        ncc[2] /= Math.Sqrt(img1SquaredSum[2]) * Math.Sqrt(img2SquaredSum[2]);
        var nccAvg = ncc.Average();
        return nccAvg;
    }

    public bool IsSimilar(double similarity, Thresholds threshold) {
        if (similarity >= HighSimilarity) {
            return true;
        }
        if (threshold == Thresholds.medium && similarity >= MediumSimilarity) {
            return true;
        }
        if (threshold == Thresholds.low && similarity >= LowSimilarity) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Process the given image in the Comparator's own way to optimize for comparing.
    /// </summary>
    private Mat ProcessImg(Mat img) {
        if (img.Size().Width == ImgResizeValue && img.Size().Height == ImgResizeValue) {
            return img;
        }
        var pImg = new Mat();
        Cv2.Resize(img, pImg, new Size(ImgResizeValue, ImgResizeValue), interpolation: InterpolationFlags.Area);
        return pImg;
    }
}

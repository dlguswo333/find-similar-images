using OpenCvSharp;

namespace FSI;

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
        try {
            var pImg1 = ProcessImg(img1);
            var pImg2 = ProcessImg(img2);
            // Use NCC, appears in the link below.
            // https://www.researchgate.net/publication/2378357_Fast_Normalized_Cross-Correlation
            pImg1 = pImg1 - Cv2.Mean(pImg1);
            pImg2 = pImg2 - Cv2.Mean(pImg2);
            var img1SquaredSum = Cv2.Sum(pImg1.Mul(pImg1));
            var img2SquaredSum = Cv2.Sum(pImg2.Mul(pImg2));
            var imgMul = Cv2.Sum(pImg1.Mul(pImg2));
            var nccSum = 0.0d;
            for (int i = 0; i < 3; ++i) {
                nccSum += imgMul[i] / Math.Sqrt(img1SquaredSum[i] * img2SquaredSum[i]);
            }
            return nccSum / 3;
        } catch {
            return LowSimilarity / 2;
        }
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
    public Mat ProcessImg(Mat img) {
        if (img.Size().Width == ImgResizeValue && img.Size().Height == ImgResizeValue) {
            return img;
        }
        var pImg = new Mat();
        if (img.Empty()) {
            return pImg;
        }
        Cv2.Resize(img, pImg, new Size(ImgResizeValue, ImgResizeValue), interpolation: InterpolationFlags.Area);
        pImg.ConvertTo(pImg, MatType.CV_32FC3);
        return pImg;
    }
}

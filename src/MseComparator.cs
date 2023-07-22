using OpenCvSharp;

namespace FSI;

public class MseComparator : IComparator {
    /// <summary>
    /// Compare images in mean squared error, averaging three BGR MSE.
    /// The result value ranges 0~255 and low value means high similarity.
    /// </summary>
    public MseComparator(int imgResizeValue = 64) {
        ImgResizeValue = imgResizeValue;
    }

    private int ImgResizeValue { get; }
    private readonly double HighSimilarity = 10;
    private readonly double MediumSimilarity = 100;
    private readonly double LowSimilarity = 300;

    public double Compare(Mat img1, Mat img2) {
        try {
            var pImg1 = ProcessImg(img1);
            var pImg2 = ProcessImg(img2);
            var diff = pImg1 - pImg2;
            // Cv2.Sum(diff.Mul(diff)) is still an array of [b, g, r].
            var squaredErrorSum = Cv2.Sum(Cv2.Sum(diff.Mul(diff))).ToDouble();
            var mse = squaredErrorSum / (ImgResizeValue * ImgResizeValue) / 3;
            return mse;
        } catch {
            return LowSimilarity * 2;
        }
    }

    public bool IsSimilar(double similarity, Thresholds threshold) {
        if (similarity <= HighSimilarity) {
            return true;
        }
        if (threshold == Thresholds.medium && similarity <= MediumSimilarity) {
            return true;
        }
        if (threshold == Thresholds.low && similarity <= LowSimilarity) {
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
        // Convert data type from byte to float so that subtracting will not underflow.
        pImg.ConvertTo(pImg, MatType.CV_32SC3);
        return pImg;
    }
}

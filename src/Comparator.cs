using OpenCvSharp;

namespace FSI;

public interface IComparator {
    public double Compare(Mat img1, Mat img2);

    public bool IsSimilar(double similarity, Thresholds threshold);

    /// <summary>
    /// Process a image in comparator's own way, but it must return a new Mat.
    /// </summary>
    public Mat ProcessImg(Mat img);
}

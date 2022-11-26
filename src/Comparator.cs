using OpenCvSharp;

interface IComparator {
    public double Compare(Mat img1, Mat img2);

    public bool IsSimilar(double similarity, Thresholds threshold);
}

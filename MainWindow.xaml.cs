
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Windows;

namespace VisionStudy
{
    public partial class MainWindow : System.Windows.Window
    {
        // 현재 검사 이미지
        private Mat? _grayImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        // 1. 이미지 불러오기
        private void LoadImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                // 이미지를 Grayscale로 읽기
                using Mat loaded = Cv2.ImRead(dialog.FileName, ImreadModes.Grayscale);

                if (loaded.Empty())
                {
                    MessageBox.Show("이미지 로드 실패");
                    return;
                }

                // 이전 이미지 자원 해제
                _grayImage?.Dispose();
                _grayImage = loaded.Clone();

                // 원본 이미지 표시
                OriginalImage.Source = _grayImage.ToBitmapSource();

                FileNameText.Text = System.IO.Path.GetFileName(dialog.FileName);

                // 영상처리 실행
                ApplyThreshold();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // 2. Threshold 변경 이벤트
        private void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded)
                return;

            ThresholdText.Text = ((int)ThresholdSlider.Value).ToString();

            ApplyThreshold();
        }

        // 3. 영상처리
        private void ApplyThreshold()
        {
            if (_grayImage == null)
                return;

            using Mat binary = new Mat();

            // 어두운 영역을 흰색으로 이진화
            Cv2.Threshold(_grayImage, binary, ThresholdSlider.Value, 255, ThresholdTypes.BinaryInv);

            // 이진화 결과 표시
            ProcessedImage.Source = binary.ToBitmapSource();

            // 4. 객체 검출
            Cv2.FindContours(
                binary,
                out OpenCvSharp.Point[][] contours,
                out HierarchyIndex[] hierarchy,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            int count = 0;

            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);

                // 면적 30 pixel 초과 객체만 검출
                if (area >= 30)
                    count++;
            }

            // 5. 검사 결과 표시
            string result = count > 0 ? "NG" : "OK";

            ResultText.Text = $"검출 영역 : {count}개 / 판정 : {result} (실습용)";
        }

        protected override void OnClosed(EventArgs e)
        {
            _grayImage?.Dispose();
            base.OnClosed(e);
        }
    }
}

# Ghi chú bộ 300 màn

Mô hình đề xuất học thống kê từ 500 màn khác nhau theo thứ tự TbLevelConfig1 của game gốc. Mỗi màn mới dùng một mục tiêu tương ứng trong 300 màn đầu; bộ sinh dựng lại vị trí và đồ thị liên kết mới, sau đó lọc bằng solver và các ngưỡng thiết kế. Đây là mô hình phân bố thực nghiệm kết hợp bộ dự đoán ExtraTrees, không phải mô hình neural.

- Màn 1–3 dùng 3–5 chi tiết để giới thiệu xoay vòng và tháo chốt. Ngoại lệ tutorial được ghi trong báo cáo.
- Thanh trượt xuất hiện từ màn 6; chìa khóa/ổ khóa từ màn 26. Có 232 màn dùng thanh trượt và 98 màn dùng chìa khóa.
- Số chi tiết từ 3 đến 76, giữ các nhịp tăng và nghỉ của thứ tự tham chiếu thay vì tăng đều. Màn 150 là một màn đông chi tiết cần ưu tiên test trên màn hình nhỏ.
- Điểm độ khó máy tính lệch mục tiêu trung bình 6,40%, tối đa 34,53%. 199 mục tiêu có lời giải trong mô hình rút gọn; 101 mục tiêu dùng ước lượng cấu trúc vì solver rút gọn chưa giải được màn gốc đó.
- Cả 300 màn có lời giải được kiểm tra bằng hai chính sách solver và bộ thông dịch độc lập. Kiểm tra novelty đối chiếu cả 507 màn gốc đã trích xuất, kể cả 7 màn không dùng để học. Không phát hiện đồ thị tương đương hoặc cặp dưới ngưỡng near-copy .10. Khoảng cách này là heuristic, không phải tỷ lệ phần trăm nội dung mới được bảo đảm.

Mở `Level Design Preview.png` để xem 20 màn đại diện; `Level Design Index.csv` có chỉ số cho từng màn. Lời giải lưu không được chứng minh là tối ưu, nên PAR chỉ là số thao tác của lời giải đó.

## Cần xác nhận bằng playtest

Độ khó và độ hay với người chơi chưa được đo. Điểm máy tính phản ánh mô hình chốt/khe rút gọn; không chứng minh ngang game gốc. Native Core của game gốc chưa được thực thi, nên va chạm liên tục tùy ý, butterfly clip, thanh cong phức tạp và nổ theo tiếp xúc chưa được tái dựng đầy đủ.

Khi test, nên ghi màn, thời gian hoàn thành, số lần thử lại, số hint, chỗ khó nhìn/chạm và đánh giá vui 1–5. Ưu tiên các mốc 1–10, 26, 50, 100, 150, 200, 250, 300; sau đó rà toàn bộ để điều chỉnh nhịp độ. Các báo cáo trong `audits/` lưu riêng bằng chứng máy tính và những giới hạn này.

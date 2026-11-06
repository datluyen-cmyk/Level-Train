# Bộ mẫu huấn luyện trong Unity

Menu **Rotate Rings > Training Preview**. Bấm **Reload manifest**, chọn màn và **Start selected**. Sau khi thắng dùng NEXT LEVEL; tất cả màn thử đều có thể chọn trực tiếp.

Mỗi revision được lưu riêng; `current_preview.json` trỏ đến bộ hiện tại. Màn thử chưa được tính vào bộ 300 màn. Mức độ khó chênh 15% so với nguồn là mục tiêu người dùng, chưa được xác nhận.

Feedback: chọn Too easy / Too hard / Visual issue / Playable, viết mô tả và Save feedback note. Cửa sổ ghi đúng ID màn đang chơi vào `feedback.jsonl`, kèm revision. Có thể gửi ID màn và mô tả ngay trong chat.

Preview v3 gồm 6 màn: 9600003, 9700005, 9600002, 9700002, 9700003, 9700004. Tất cả đã giải và replay độc lập trên luật native hiện hành. Nhánh vòng và cụm vòng–thanh kéo được xếp từ ít tương tác đến nhiều hơn; nhãn dễ/khó chưa được hiệu chuẩn. Đây là số bước hành động của solver, không phải số thao tác tối ưu hay độ khó người thật.

Các bộ E/F trước đó chỉ là chẩn đoán, không có lời giải xác minh, không dùng làm bản chơi chính. Không export Resources hay tạo build.

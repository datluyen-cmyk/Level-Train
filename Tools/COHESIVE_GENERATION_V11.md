# Bộ sinh với kiểm tra bố cục V11

Vẫn mở `Tools/level_training_bot_gui.py`. GUI gọi `relational_level_generator_v10.py`; tên file được giữ để các round cũ tiếp tục đọc được. Chính sách thiết kế mới được lưu là `cohesivePolicyVersion: 11`.

Luồng hiện tại:

1. Học motif từ liên kết gốc, guide và quan hệ hình học slider/ring. Quan hệ hình học chỉ là bằng chứng tĩnh, không phải khóa hoặc thứ tự giải đã xác nhận.
2. Dùng mô hình bố cục toàn màn để hướng dẫn đặt motif, thay vì chỉ xếp từng cụm. Mô hình local được fit trên 460 màn ordinary và đúng danh mục template của bộ test.
3. Giữ các nhánh ghép thay thế; quay lại nhánh trước nếu ghép tiếp bị kẹt hoặc cả màn trượt kiểm tra cuối.
4. Kiểm tra nguồn đóng góp, hình học native, khoảng đứt giữa các cụm, khoảng cách biên hình dạng và mức phân mảnh của đồ thị liên hệ. Giới hạn nguồn được tính theo số chi tiết thực của màn gần hoàn thành, không dùng số mục tiêu để nới tỷ lệ.
5. Chỉ đưa proposal vượt kiểm tra tĩnh sang bot Unity. Chưa có chứng cứ native thì chưa được coi là đã giải được. Bot không thắng hoặc solver hết ngân sách là chưa kết luận được.
6. Khi stage motif native, giữ nguyên hướng guide đã kiểm tra; không ghép lại hướng guide từ một màn tham chiếu khác.

`Fit ML models` đã gồm model motif có quan hệ slider/ring, model tiếp nối motif và model bố cục toàn màn. Nếu chỉ dùng corpus local hiện tại thì các model này đã được fit. Nếu thay source/template, generator kiểm tra hash và yêu cầu fit lại khi chúng không khớp. Mở lại GUI đang chạy để nhận code mới.

Hai cache mới chỉ tránh tính lại cùng dữ liệu. Đã kiểm tra 20 lựa chọn motif cùng seed không đổi và trường bố cục cold/warm giống từng phần tử. Tốc độ đo của riêng thao tác lấy mẫu và riêng tính trường không phải tốc độ cả round.

Nếu không có proposal vượt kiểm tra, tool giữ báo cáo và không thay bộ test Unity. Không tự hạ ngưỡng để tạo đủ số màn. Số màn thực tế vì vậy có thể thấp hơn số yêu cầu.

Kiểm tra lại các snapshot thực tế bằng `audit_cohesive_layout_v11.py --workspace ... --packs ... --output ...`. Các file có `diagnosticOnly: true` chỉ phục vụ phân tích, không được xem là level đã duyệt.

Giới hạn: các ràng buộc này phát hiện được lỗi đã thấy, nhưng chưa chứng minh bộ sinh học được chủ đích thiết kế hoặc độ hay ngang game gốc. Lượt sửa này chưa thay bộ preview hiện tại bằng một bộ mới.

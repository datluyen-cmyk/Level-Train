# Bàn giao Rotate Rings

Mở `RotateRings Test` bằng Unity 6000.0.78f1; Unity sẽ tạo lại Library. Thư mục gốc chứa APK gốc, Research và Tools. Cần cài Git LFS trước khi clone (`git lfs install`, rồi `git clone ...`; nếu đã clone thiếu dữ liệu thì `git lfs pull`).

Đọc `CHAT_EXPORT/chat.md` để xem hội thoại được xuất đến thời điểm bàn giao. Đây là bản lưu văn bản và ảnh trong Git, không phải bản nhập lại nguyên trạng vào lịch sử Codex. Muốn tiếp tục, mở repository trong Codex ở máy mới và yêu cầu đọc file này cùng tài liệu bàn giao.

Bộ nghiên cứu chính: `RotateRings Test/LevelTraining_SourceLayoutV3/`, đặc biệt `level_training.json` và `runs/ordinary_v4/`. Một số cấu hình và log chứa đường dẫn tuyệt đối D:/RotateRings: kiểm tra và cập nhật đường dẫn theo máy mới trước khi chạy, hoặc clone vào cùng đường dẫn. Cần cài Python và các phụ thuộc trong bundle, cấu hình Unity MCP tại máy mới; không giả định MCP hoặc skill cài sẵn sẽ tự chuyển theo Git. Skills đã dùng: `reverse-engineer-anything` và `level-training` (cài riêng trên máy mới nếu cần).

Yêu cầu: nghiên cứu ít nhất 500 màn nguồn, loại màn tạo hình khỏi tập học; xây bộ sinh mới có thể mở rộng lên hàng nghìn màn, học cách xếp và các cơ chế/độ khó, không sao chép nguyên bàn nguồn. Đích ít nhất 300 màn mới, độ khó sai lệch khoảng 15% là chấp nhận được nhưng chưa đo được độ khó người. Không khóa cứng chỉ tiêu hình học/số lượng. Cần mẫu playable ở nhiều độ khó, có 10 màn liên tục để người dùng phản hồi và 1.000 hồ sơ người chơi mô phỏng; không build.

Tình trạng: 507 bố cục nguồn được phục hồi, 460 màn thường và 47 màn tạo hình loại khỏi train nhưng giữ để đối chiếu tính mới. Chưa có màn mới nào được nghiệm thu. Sáu màn preview đã được người dùng đánh giá SUPER EASY, không dùng làm chuẩn độ khó. Lần 1.000 bot đối chiếu mẫu mới chưa hoàn tất. Không khẳng định đã đạt ±15% hoặc đã tạo xong 300 level.

Nghiên cứu các màn 39, 49, 129, 175 và 285 dùng dữ liệu gốc trong runtime Unity clone, không phải thao tác trực tiếp trong APK. Xem `runs/ordinary_v4/playtests/source_design_v7/SOURCE_MECHANISMS_V7.md`. Nhãn HUD cũ hiển thị 37/46 là chỉ số trong corpus đã lọc; đã sửa về position 39/49 cùng source ID.

V8: sáu proposal bị giữ lại do giao cắt hình học không có chứng nhận; kiểm tra giới hạn tiếp theo chỉ sinh được một phần cấu trúc nhỏ, chưa được chứng minh chơi được. Xem `runs/ordinary_v4/batches/joint_hyperedge_v8_native_geometry_checkpoint_20261108.json`. Mô hình phân loại độ khó tĩnh chưa vượt baseline; không dùng như chứng cứ độ khó. Ưu tiên tiếp theo là kiến trúc sinh bố cục hợp lệ toàn cục và kiểm tra đường thao tác/va chạm bằng Unity, sau đó mới đánh giá độ khó, tính mới và nghiệm thu.

Danh mục element và booster: `Research/element_catalog_v7/README.md`. Clone hiện có khác biệt về nhắm mục tiêu pháo hoa so với APK. Giữ các báo cáo thất bại, dữ liệu và weights để tiếp tục nghiên cứu; không biến kết quả chưa hoàn tất thành sản phẩm đã nghiệm thu.

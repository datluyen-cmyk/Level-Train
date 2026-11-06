# Physics v2 — ghi chú rà soát thiết kế

- Batch: `RotateRings Test\LevelTraining_PhysicsV2\batches\final-300\unity_levels.json`
- SHA256 batch: `88574cc744ed69f00cf5ae7e9ad8f0c333099e814abb78ff9885e5959e9103ae`
- Catalog mẫu nguồn: `RotateRings Test\LevelTraining_PhysicsV2\templates.json`
- SHA256 catalog: `12bef8bac58a879b05a4815cd73186d5e408dd7380f4641d952d820e3f9f2095`
- Trạng thái catalog: `native geometry exported; dynamic policy pending physical replay` (62/500 nguồn đủ điều kiện theo metadata catalog)
- Số màn trong index: 300
- Preview hiển thị 30 màn đầu và các vị trí đại diện; ID được chọn: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 50, 100, 151, 200, 249, 299
- Hình preview vẽ các đường PolygonCollider2D nguyên bản theo `templateId` của từng quân cờ, đặt tại vị trí/góc của batch. Tọa độ pin lấy nguyên từ template; script không suy ra pin từ link và không co giãn riêng pin.
- Bố cục trong batch vẫn là bố cục sinh mới. Việc dùng từng mảnh nguồn nguyên bản không chứng minh toàn bộ bố cục từng xuất hiện trong game gốc.
- Chỉ số nhánh/độ sâu được tính từ đồ thị link của batch. Điểm novelty/proxy chỉ được sao chép nếu batch có trường đó; trường thiếu để trống. Chúng không phải đánh giá của người chơi.
- Tài liệu này không khẳng định 300 màn đã được xác nhận vật lý, có độ khó tương đương, vui, công bằng hay có human parity. Hãy ghi trải nghiệm thực tế trong `Playtest Log.csv`.

## Cách đọc

- `Level Design Preview.png`: contact sheet để kiểm tra nhanh các đường viền vật lý theo vị trí.
- `Level Design Index.csv`: một dòng cho mỗi màn; `reference_position/count` là tham chiếu gốc nếu ánh xạ được, `actual_part_count` là số quân thật trong batch.
- `Playtest Log.csv`: 300 dòng trống để ghi nhận buổi chơi thủ công.

# Bộ huấn luyện Rotate Rings — vật lý native V2

Mở `Open Generator.vbs` để tạo thêm level từ mô hình đã lưu. Không cần chạy lại nghiên cứu APK hoặc huấn luyện. Gói cuối được đặt ở `batches/final-300`; chỉ coi là hoàn chỉnh khi `validation_report.json` có `complete: true` và đủ 300 level. Các bản chưa đủ số lượng là checkpoint, không phải bộ giao cuối.

## Dữ liệu và mô hình

- 500 level gốc khác nhau, giữ nguyên thứ tự tiến trình, hash và dữ liệu nguồn trong `corpus.json`/`references.json`.
- Học thống kê điều kiện từ toàn bộ 500 level: số mảnh theo tiến trình, mật độ thành phần liên thông, tỉ lệ phân nhánh và tần suất mẫu mảnh native. Lấy 16 hàng lân cận tiến trình để điều kiện hóa đề xuất. Đây là bộ sinh lai thống kê thực nghiệm, dựng bố cục theo ràng buộc và loại bằng vật lý; không phải mạng neural sinh hình học.
- 62 level có danh mục cơ chế khả dụng; 9 bị loại tiếp vì kiểu đính kèm chưa hỗ trợ. 6 level gốc có đường giải được phát lại thành công. 47 level hết ngân sách tìm kiếm 20 giây/120 thử hành động; kết quả này không chứng minh chúng vô nghiệm.
- Đã so sánh ExtraTrees và baseline trung bình điểm/mảnh trên họ giữ lại. Chọn baseline làm bộ dự đoán/gate chính vì sai số giữ lại thấp hơn; baseline được fit lại trên cả 6 nhãn, trung bình khoảng 1.77646 điểm/mảnh, rồi nhân số mảnh. ExtraTrees chỉ giữ làm artifact chẩn đoán trong models/difficulty_extra_trees.joblib. Chỉ 6 nhãn vật lý khả dụng: 5 hàng huấn luyện và 1 hàng giữ lại theo họ cấu trúc. Sau khi gộp họ đối xứng, MAE tổng điểm giữ lại của ExtraTrees khoảng 0.846, baseline được chọn khoảng 0.194; mô hình này kém baseline trên hàng kiểm định duy nhất và không chứng minh khả năng khái quát tốt hơn. Không đủ dữ liệu để khẳng định độ khó người chơi tương đương; R² không được báo với một hàng kiểm định.
- Tần suất góc xoay lấy từ các lời giải nguồn đã phát lại. Mọi mẫu pin/ring được lấy nguyên khối từ danh mục; không kéo dài stem, đổi kích thước, thêm pin dựa trên cạnh đồ thị hoặc đổi vị trí pin trong một mảnh.

## Phạm vi và giới hạn

Bộ mới dùng tập con ring có khe vật lý 90 độ, bán kính 1.75, bề rộng vật lý 0.5, 0–3 pin TBar ngoài, stem nguồn không quá 1.6. Không chứng nhận slider, bomb, chain, butterfly, catch phức tạp hoặc các ngoại lệ luồn pin đã được tác giả gốc thiết lập. Kích thước render và collider có thể khác nhau; cả hai đều giữ theo đúng mẫu nguồn.

Giữ quy mô mục tiêu từ 300 vị trí nguồn đầu, dao động tối đa khoảng 10%, giới hạn đến 84 mảnh. Thành phần mới có tối thiểu 2 mảnh khóa thực, tối đa 18 mảnh/nhóm để dựng và kiểm chứng; mật độ nhóm được chiếu vào miền này. Các fixture rời chưa hỗ trợ của bản gốc không được biến thành ring trống để đủ số lượng. Bố cục mới không sao chép tọa độ hoặc đồ thị nguồn; mỗi mảnh được đặt và kiểm chứng đường xoay nhả theo thứ tự ngược, sau đó phát lại độc lập toàn cảnh.

Điểm độ khó là chi phí máy không tối ưu: số thao tác + tổng độ xoay/180 + 0.65 × độ sâu + 0.35 × số nút phân nhánh. Gate chấp nhận sai khác tương đối tối đa 35% với nhãn nguồn khi có, hoặc dự đoán nguồn khi không có. Đây là độ khó máy trong tập con đã công bố, không phải chứng nhận độ hay, thời gian chơi hay độ khó của game gốc trên người thật.

## Kiểm chứng và tái lập

- Hình polygon collider native, khoảng skin Box2D 0.01/mỗi phía, ngưỡng khoảng cách hình học 0.0195.
- Không chồng solid lúc bắt đầu. Chỉ head/body đúng bộ `(owner, attachmentId, target)` mới được chạm; cấm head cắt pin, wall hoặc head khác.
- Sweep tối đa 0.5 độ, giữ `delta` có dấu trong lời giải. Góc cuối không đủ để xác định đường xoay.
- Fingerprint bỏ màu, ID và biến đổi cứng; mẫu góc pin của từng mảnh được chuẩn hóa với ảnh gương để loại cả đối xứng toàn bàn, đồng thời có thể loại dư một số bố cục khác biệt do phản chiếu độc lập từng mảnh; ngưỡng khoảng cách cấu trúc tối thiểu 0.1 với toàn bộ 500 nguồn và các level mới đã nhận. Đây là heuristic phát hiện gần sao chép, không phải phần trăm tính mới được bảo đảm.
- `models/manifest.json` ràng buộc adapter, corpus, danh mục, cấu hình, công cụ và mô hình bằng SHA-256. `validation_report.json` ràng buộc pack đầu ra và lưu nguyên các lý do loại.
- Giao diện sinh tiếp tự loại bộ `final-300` khi đã có, đồng thời hỗ trợ người dùng thêm các pack cũ khác. Chỉ tạo dữ liệu, không xuất build Unity.

Các lần sinh bị gián đoạn được giữ riêng trong `batches/`. Bản `interrupted-31-sharing-violation` ghi lại lỗi khóa file Windows trước khi bổ sung retry atomic write và bỏ bản sao sourceRefs dư thừa ở từng instance; danh mục mẫu vẫn giữ đầy đủ provenance.

## Kết quả gói cuối

Bộ `batches/final-300/generated_levels.json` đã đủ 300 level, `complete: true`, gồm 6.211 instance mảnh nguyên bản và tối đa 84 mảnh/level. Tất cả 369 mẫu mảnh đủ điều kiện đã được dùng. Seed 20261105. 304 ứng viên toàn level: 300 nhận, 3 ngoài gate điểm máy, 1 bị phát lại độc lập loại. Các lần thử/backtrack vị trí từng mảnh bên trong constructor không được cộng đầy đủ vào bộ đếm ứng viên này.

Audit số học độc lập tại `audits/training_score_audit.json` PASS 300/300: 4.720 thao tác có delta ký hiệu, quy mô trung bình 20.703 mảnh so với 20.563 của 300 vị trí nguồn đầu. Tỉ lệ điểm máy mới/mục tiêu trung bình 0.786, nhỏ nhất 0.6502 và lớn nhất 1.1260; chỉ nói lên gate proxy đã công bố. Audit vật lý/Unity, nhập asset và quan sát người chơi là bằng chứng riêng.

SHA-256 của pack cuối: `4a7d2a98b16e8a82d02c290247dd27012b24aef9f5b07545771775641274979a`.

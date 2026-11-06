# Rotate Rings — trạng thái chỉnh lại theo bản gốc

**Bộ 300 màn PhysicsV2 đã bị bác bỏ và rút khỏi runtime.** Nó làm mất các dạng thanh thẳng/L/U, catch, butterfly, bomb và chain; các kiểm tra vật lý V2 chỉ áp dụng cho tập luật giản lược đó. Không dùng kết quả 300/300 của V2 làm chứng nhận cho độ khó, bố cục hay độ giống bản gốc.

Đang sửa bộ đọc, hiển thị và luật tương tác trước khi huấn luyện/sinh lại. Chưa có bộ 300 màn V3 được chấp nhận. Không tạo build.

Cập nhật theo yêu cầu bỏ màn tạo hình: nhánh đang dùng là
`LevelTraining_SourceLayoutV3/runs/ordinary_v4`. Đã lọc và huấn luyện lại từ
460 màn thường, loại 47 màn tạo hình. APK chỉ có 507 bố cục khác nhau nên không
đủ 500 màn thường sau lọc. Không dùng các mô hình hỗn hợp cũ để sinh tiếp.
Mục tiêu vẫn là 300 màn mới, theo 300 màn thường đầu tiên của tiến trình gốc.
Số vật thể/cụm, mật độ chốt và góc sửa là đặc trưng mềm, không phải quota cố định.

## Dữ liệu nguồn

Đã đọc đủ 500 màn theo thứ tự gốc, giữ 7.295 vòng, 3.982 thanh kéo (1.363 thẳng, 1.929 L, 690 U), 3.339 catch cố định, 62 butterfly, 179 bomb và 99 mảnh khóa xích. Các số liệu, cấu trúc liên kết và bố cục giữ tại `../Research/physics_v3/`; đây là bằng chứng dữ liệu, chưa phải nhãn giải thành công.

Training phải học quan hệ hình học và cách tổ chức bố cục: cụm lồng/móc, vị trí và hướng tương đối, catch, khoảng trống, đối xứng, nhánh/phụ thuộc và nhịp tiến trình. Không chấp nhận lấy số lượng rồi đảo ngẫu nhiên, hoặc đổi màu/lật gương nguyên màn để tính là màn mới. Thứ tự tháo và độ khó chỉ được gắn nhãn sau khi kiểm chứng luật thực.

## Xem nguyên mẫu để kiểm tra phần port

Trong Unity, menu **Rotate Rings > Original source parity viewer** mở 8 bố cục gốc đại diện. Chúng được đánh dấu **ORIGINAL REFERENCE / QA**, chỉ dùng đối chiếu hình học, vật liệu và cơ chế. Không được xuất các màn này thành bộ màn mới.

Scene chính: `Assets/Scenes/RotateRings.unity`. Color space Gamma theo APK. Unity MCP tại `http://127.0.0.1:8080/mcp`. Nếu bấm Play khi chưa có bộ schema 3 được chấp nhận, runtime hiện thông báo đang rà soát.

## Điều kiện giao lại

Phải kiểm chứng các dạng hình và luật gốc trong Unity; học/đánh giá mô hình bố cục trên họ màn giữ riêng; sinh ít nhất 300 màn mới; kiểm tra lời giải, trùng bố cục và phân bố độ khó/đa dạng theo tiến trình. Sau đó mới cập nhật Resources và bằng chứng theo hash. Các artefact V2 được giữ để truy vết, không phải bản giao hiện tại.

## Bộ mẫu để feedback trong quá trình học

**Rotate Rings > Training Preview** hiện có 6 màn ở `LevelTraining_SourceLayoutV3/runs/ordinary_v4/preview/current_preview.json`. Chọn mẫu và **Start selected**; không cần build. Màn thử không ghi tiến trình PlayerPrefs. **Save feedback note** ghi đúng ID màn đang chơi kèm revision vào `preview/feedback.jsonl`. Nếu đang chơi bộ cũ, chọn Reload manifest rồi Start selected khi muốn đổi.

Cả 6 màn G/H được ghép từ bàn trống qua chốt/receiver tương thích, giữ nguyên asset native và đã có lời giải replay độc lập. Độ khó ±15% so với nguồn là mục tiêu chưa xác nhận; chưa tính màn nào vào bộ 300. Một mẫu nhánh vòng được pilot32 bot đối chiếu: nguồn10005 thắng29/32, mẫu9600002 thắng27/32. Kết quả là mô hình bot chưa hiệu chuẩn người thật.

1.000 hồ sơ bot đã chơi đủ2.000 phiên trên hai màn nguồn. Các lượt1.000 bot đối chiếu màn mới bị ngắt do Unity vàoPlayMode, chưa có kết quả hoàn tất. Giữ quyền chơi thử của người dùng, không tự tắtPlayMode để chạy mô phỏng.

# Yêu cầu thiết kế level đang áp dụng

Nguồn là Rotate Rings 1.2.0 do người dùng cung cấp. Yêu cầu ban đầu học ít nhất
500 level được giữ để báo mức đáp ứng, nhưng yêu cầu mới loại toàn bộ màn tạo
hình được ưu tiên: APK có 507 bố cục khác nhau, sau rà soát giữ 460 màn thường
và loại 47 màn tạo hình. Không nhân bản hoặc giữ màn tạo hình để đủ số lượng.
Mục tiêu ít nhất 300 level mới, theo 300 màn thường đầu tiên trong thứ tự nguồn.
Không nén tiến trình toàn corpus vào 300 màn. Không tạo build.

Yêu cầu sinh hiện hành: dựng level mới từ bảng trống bằng kiến thức đã học
về cụm, quan hệ giữa cụm, bố cục và độ khó. Không lấy một level nguồn làm
khung rồi đổi một vài vòng/thanh/chốt để giao. Toàn bộ 109 ứng viên sửa cục
bộ V4 chỉ còn là thử nghiệm kỹ thuật; không tính vào 300 màn mới. Bộ sinh
phải có quy trình lưu mô hình, seed và chống trùng giữa các batch để có thể
tiếp tục sinh 1.000 hoặc 10.000 màn; khả năng sinh đủ số màn đạt chất lượng
phải được đo thực tế, không suy ra từ số lượng seed.

Đánh giá bổ sung bằng 1.000 hồ sơ người chơi mô phỏng với kỹ năng, trí nhớ,
khám phá, sai sót và độ kiên trì khác nhau. Chạy pilot trước; dùng cùng hồ
sơ/ngân sách/seed cho màn nguồn và màn mới, giữ riêng họ policy và seed cho
lượt đánh giá cuối. Không đưa lời giải có sẵn vào quan sát của bot. Dùng
production physics và cache chuyển trạng thái theo hash để tiết kiệm tính
toán; không gọi LLM cho từng nước đi. Báo riêng số hồ sơ, số phiên thực chạy,
thắng/thua/hết ngân sách, hành động bị chặn và chi phí tính toán. Chỉ số này
là kết quả bot với giả định chưa hiệu chỉnh từ người thật.

Phải học cách xếp: hình tổng thể, cụm đan/móc/lồng nhau, khoảng trống, nhánh cản
và trình tự tháo. Số lượng và tỷ lệ vòng/thanh/chốt chỉ là một phần của mô tả.
Giữ nguyên độ đa dạng asset và cơ chế có thật trong nguồn. Không tự chế chốt
dài, chéo hoặc hình va chạm để làm bộ sinh dễ thành công.

Phải xem cấu trúc từng cụm: thành viên, kích thước, vai trò biên/nội thất và
quan hệ giữa các cụm. Đếm tổng cạnh hoặc tổng cụm không phát hiện được việc
tách một vòng ra rồi nối bù ở chỗ khác. Không ép mọi màn thành một cụm: nguồn
có các nhóm rời hợp lệ, đặc biệt nhóm thanh kéo cùng guide. Graph pin/guide
chỉ là một phần chứng cứ vì chưa mô tả đủ cản hình học và trình tự tháo.

Cập nhật theo người dùng: tách thêm một ít cụm vẫn hợp lệ nếu độ khó được
cân bằng với nguồn và bố cục hợp lý. Không khóa cứng số lượng vật thể, số cụm,
mật độ chốt, góc thay đổi, tỷ lệ root phải sửa hoặc mức đối xứng. Dùng chúng
làm đặc trưng học và xếp hạng tương đối với nguồn, không dùng các ngưỡng thiết
kế cố định để nhận/loại level. Các giới hạn số của pilot cũ là lịch sử, không
phải yêu cầu hiện hành. Luật game, tính nguyên vẹn asset và replay hợp lệ vẫn
là điều kiện đúng/sai; độ khó và độ hay cần chứng cứ riêng.

Cập nhật mới nhất: bỏ các màn tạo hình (kim cương, mặt, người, phương tiện,
hoa/ngôi sao có chủ đích, biểu tượng và cảnh vật) khỏi tập học, reference sinh
và đầu ra. Dùng danh sách ID đã review trong run `runs/ordinary_v4`; không tự
loại grid, mê cung, radial hoặc network chỉ vì chúng đối xứng. Các mô hình cũ
đã học tập hỗn hợp và pilot hình đặc biệt chỉ là lịch sử, không được dùng tiếp
cho nhánh thường. Phải refit mô hình từ corpus thường đã lọc, không chỉ lọc
đầu ra sau khi mô hình đã học hình đặc biệt. Toàn bộ 507 nguồn vẫn là tập tránh
trùng; mục đích này không đưa màn tạo hình trở lại training.

Chỉ đổi màu, phản chiếu/quay cả màn, lật 180 độ một thanh đối xứng, đổi ID hoặc
xoay các chi tiết trang trí ngoài viền không đủ tạo một level mới. Với màn tạo
hình, phải kiểm tra thay đổi trong quan hệ cản hoặc lựa chọn/thứ tự tháo ở phần
chơi có ý nghĩa. Một nhãn static sweep khác không tự chứng minh điều này.

Mọi pin native luôn tồn tại cùng vật thể chủ và là vật cản theo đúng luật.
Tháo binding không được làm mất mesh pin. Không miễn va chạm của stem chỉ vì
hai vật thể từng được liên kết. Guide, butterfly, bomb và xích không được bỏ
để solver dễ giải.

Chỉ nhận level sau kiểm tra asset/collider/input nguyên vẹn, hình học, tính mới
so với đủ 507 nguồn tương thích và các đầu ra đã nhận, replay đường giải bằng
production physics, so sánh độ khó với nguồn và review hình tổng thể. Pose
teleport để chẩn đoán không phải đường kéo hợp lệ. Search hết ngân sách là
`unknown`, không phải bằng chứng level vô nghiệm.

Giữ các chỉ số mô hình kém baseline trong báo cáo. Không gọi một bộ xáo pose
hoặc một pilot bị loại là bộ level đã training thành công. Độ khó máy đo được
khác với độ khó và độ hay mà người chơi cảm nhận; phần sau cần playtest.

Trạng thái hiện tại: acceptedCount = 0, generationEnabled = false. Các pack
`incomplete_*` và `staged_*` chỉ là mẫu nghiên cứu, chưa phải bộ 300 giao để chơi.

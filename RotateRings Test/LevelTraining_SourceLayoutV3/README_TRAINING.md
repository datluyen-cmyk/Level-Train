# Rotate Rings — học bố cục thông thường

**Phạm vi đang áp dụng:** yêu cầu mới bỏ toàn bộ màn tạo hình. Dùng run riêng
`runs/ordinary_v4`, đã lọc 460 màn thường và refit ba mô hình sau đánh giá tách
họ nguồn. 47 màn tạo hình chỉ giữ để tránh trùng; không tham gia học bố cục.
APK có 507 bố cục khác nhau, nên yêu cầu 500 nguồn thường hiện không đáp ứng.
Mục tiêu vẫn 300 màn mới. Các đoạn mô tả mô hình học đủ 500 dưới đây là lịch sử;
không dùng chúng để chứng nhận hoặc sinh đầu ra hiện hành. Accepted vẫn bằng 0.

Đây là workspace V3 đang phát triển. Chưa có bộ 300 level mới được chấp nhận. V2 giữ lại làm bản thử nghiệm đã bị loại vì thiếu độ đa dạng và độ sát game gốc.

Đã trích nguyên vẹn 500 level đầu, giữ thứ tự 1–500, toàn bộ 7.295 vòng, 3.982 thanh kéo thẳng/L/U, 3.339 chốt cố định, 62 butterfly, 179 bomb và 99 phần có khóa xích. Không bỏ level để khớp giới hạn của solver. Mỗi collider giữ ID gốc, lớp collider, layer, trạng thái trigger và đường hình học. CircleCollider2D của bomb giữ hình tròn chính xác.

Mô hình bố cục học 11.381 quan hệ không gian cụ thể: tọa độ tương đối trong hệ trục của từng chi tiết, góc tương đối, hình học nhận pin, vị trí đầu pin, loại/hình dạng nguồn và đích, nhánh phụ thuộc, vị trí guide và tiến trình level. `data/source500_spatial_grammar.json` còn giữ khoảng cách giữa các hình, vị trí theo trục chính, sai số đối xứng, khoảng trống và cụm phụ thuộc. Bộ này không dùng cách khớp số lượng rồi xáo vị trí tùy ý.

`models/composition_pose.joblib` là hỗn hợp pose thực nghiệm có điều kiện theo hình học và topology. Nó có thể đề xuất/rank một quan hệ hình học. Việc ghép thành bàn chơi hoàn chỉnh và chứng minh chơi được còn phải qua kiểm tra native Unity; mô hình này chưa phải bộ sinh hoàn chỉnh.

500 tên nhóm nguồn ban đầu đều là singleton nên đã được kiểm tra lại. Đánh giá dùng 446 nhóm sau khi ghép họ cấu trúc/hình học và họ không gian bất biến qua quay/phản chiếu. Đặc biệt 54 bàn 200xx dùng chung lưới 24 chi tiết được đặt vào cùng một họ dù graph pin khác nhau. Các họ được giữ nguyên trong từng fold, không đưa một phần của họ vào train và phần còn lại vào test.

Đánh giá 5 fold, mỗi fold giữ lại toàn bộ họ level:

| Chỉ số bố cục | Mô hình hình học/topology | Baseline chỉ số lượng/cơ chế |
|---|---:|---:|
| NLL quan hệ, thấp hơn tốt hơn | 4,894 | 10,104 |
| Sai số XY tốt nhất trong 32 pose, đơn vị bề rộng | 0,326 | 0,701 |
| Bố cục gốc được xếp cao hơn một bản xáo pose tổng thể | 99,8% | 99,6% |

Chỉ số thứ hai cho phép chọn pose tốt nhất trong 32 thành phần; không phải độ chính xác của một dự đoán duy nhất. Đơn vị chuẩn hóa quan hệ local là max(bề rộng nguồn, bề rộng đích, 1 world unit); không phải mọi sai số đều chia cho nét vòng 0,5. Bản xáo pose là dữ liệu âm tổng hợp và phép thử khá dễ; không xem chênh lệch 99,8%/99,6% là bằng chứng về độ hay. Các chỉ số trên không đo độ khó hoặc người chơi thật. Cấu hình K=32 và bandwidth được chốt trước đánh giá. Mô hình cuối được refit từ đủ 500 level sau đánh giá.

`models/global_composition.joblib` học 29 thống kê bố cục tổng thể từ đủ 500 bàn: trục chính, đối xứng, khoảng trống, phân bố hướng, độ gần giữa viền hình, vị trí cụm và các quan hệ guide. MAE chuẩn hóa giữ lại toàn họ là 0,530; baseline tuyến tính dùng cùng số lượng/cơ chế/topology là 0,683. Ranker phân biệt nguồn với nhiễu tổng hợp có AUC 0,680 (rung nhẹ), 0,674 (quay một cụm), 0,805 (giãn khoảng cụm) và 0,936 (xáo pose mạnh). Nguồn và mọi bản nhiễu dùng cùng đường tính feature; các trường hợp khó được báo đúng mức, không xem đây là độ đẹp hoặc độ hay.

`models/macro_assembly.joblib` giữ 4.232 mảnh cấu trúc nguồn và 8.452 quan hệ pose thực giữa chúng. Mảnh được phân tích từ cộng đồng graph pin/guide và lân cận không gian của bàn gốc; có hình, quan hệ, pattern, ranh giới kết nối và provenance cụ thể. Guide closure chạy cả hai chiều: khi giữ một Pull thì giữ cả guide tương ứng; butterfly giữ đủ các slot. Socket so sánh toàn bộ hình học vật lý/hình ảnh, thứ tự collider, input band, Rigidbody, alpha, material và khóa/bomb; chỉ cho khác RGB và định danh nguồn. Chi tiết dùng chung vẫn giữ một asset nguyên vẹn, không chỉnh kích thước hay chế pin. Macro pair NLL 48,724 so với baseline 48,770 và sai số XY oracle 3,035 so với 3,067 bề rộng chỉ hơn rất ít; không xem đây là bằng chứng về độ hay.

`models/macro_pattern.joblib` học 37.380 bộ ba macro nguồn thực: hướng đến cụm hiện tại, cụm tiếp theo, tỷ lệ khoảng cách và hướng native. Chuẩn hóa tọa độ theo khoảng cách đầu vào giúp giữ pattern hàng/cột/vòng qua nhiều kích thước. NLL giữ lại toàn họ là 18,485 so với baseline số lượng/cơ chế/tiến trình 23,597; XY tốt nhất trong 32 thành phần là 0,427 so với 0,471 đơn vị khoảng cách macro đầu vào. Cả năm fold cải thiện XY; NLL một fold kém hơn baseline và một fold có đuôi sai số lớn, giữ nguyên trong báo cáo.

`models/whole_board_field.joblib` học bố cục toàn màn bằng 10 kênh 37×25: tâm từng loại chi tiết, footprint solid native từng loại, centerline quan hệ pin/guide; kèm 24 bin hướng cho mỗi loại. Footprint giữ lỗ vòng, stem, thanh L/U dài và guide wall. Khung tọa độ lấy trục stroke bậc bốn, toàn bộ bounds hình học solid; không quay tùy ý theo góc trung bình cụm. PCA 32 thành phần được fit trong từng fold train, rồi mô hình cuối refit đủ 500. Mẫu đề xuất giải mã latent pha giữa hai họ nguồn khác nhau. Sai số L1 giữ lại toàn họ: tâm 1,123 so với tuyến tính 1,132; footprint 1,051 so với 1,054; quan hệ 0,4293 so với 0,4300. Chênh lệch nhỏ được báo đúng mức. Phương pháp này học trường bố cục thực, nhưng chưa đảm bảo mẫu pha đẹp hoặc chơi được.

Các bộ đề xuất đang được kiểm tra theo hai nhánh. Nhánh frontier ghép macro theo socket hoặc pattern/trường bố cục đã học, giữ trong envelope native của mức nguồn. Nhánh `tools/macro_rewrite_v3.py` giữ khung toàn bàn gốc và thay vùng 20–35% root bằng macro khác, kiểm tra mọi socket biên cùng lúc. Nhánh rewrite giữ kích thước vùng, độ phủ nét, mật độ quan hệ, family cơ chế và sai lệch field toàn bàn. Cả hai còn có tỷ lệ thành công thấp; nhiều pilot thiếu số lượng, rời rạc hoặc không tìm được donor phù hợp. Các snapshot thất bại nguyên bản được đánh dấu `rewriteSucceeded=false`; không được xem là level mới. Bộ thay 5 chi tiết bằng 2 làm thủng bố cục đã bị loại.

Đã bổ sung graph bố cục vật lý từ đủ 500 nguồn: 2.641 quan hệ trục thanh đi qua miền vòng, 780 quan hệ vòng nằm trong bao lồi của thanh L/U và 3.720 quan hệ gần nhau yếu. Có 2.957 cặp mạnh khác nhau. Chúng mô tả hình học tại pose nguồn, được gắn `STATICcompositionproxy`; không phải nhãn va chạm đang hoạt động, khóa, thứ tự tháo hay lời giải. Graph pin/guide trước đó bỏ sót các quan hệ này, khiến việc cắt cụm có thể phá cấu trúc bao quanh dù vẫn giữ đủ liên kết pin.

`models/macro_physical_assembly.joblib` dùng graph trên để giữ toàn bộ biên pin/guide và biên bố cục mạnh khi cắt cụm; quan hệ gần nhau yếu chỉ hỗ trợ phân nhóm. Mô hình học 3.561 macro và 7.110 quan hệ macro từ 500 nguồn. NLL giữ lại toàn họ là 53,557 so với baseline 52,734, tức kém hơn; XY tốt nhất trong 32 pose là 3,243 so với 3,303. Pilot 235 mẫu qua kiểm tra nguyên vẹn asset, chỉ 9 qua kiểm tra đường giao hình thăm dò và chưa mẫu nào đạt đủ chất lượng bố cục. Các mẫu đó vẫn bị loại.

`tools/assembly_integrity_audit_v3.py` độc lập kiểm chứng mọi import là phép quay/tịnh tiến thật, hình native nguyên vẹn, guide closure đầy đủ, edge không bị chế và bomb có provenance. Chứng chỉ `compositionBody` phải có đủ mọi shape thân native, đúng pose nguồn và cặp mạnh được tính lại từ hình nguồn. Thiếu graph chứng cứ, sửa số liệu, dùng quan hệ gần nhau yếu hoặc ghép thêm pin ngoài chứng chỉ đều bị loại. Quan hệ bố cục không được xuất thành edge gameplay.

`tools/learned_placement_audit_v3.py` kiểm tra nhánh đặt lại từng asset nguyên bản: guide vẫn là nhóm cứng, mỗi quan hệ có pattern nguồn và pose từ mô hình đã lưu; các chỉ số fit được tính lại. Chứng chỉ thân biên và thân khung thay thế phải giữ đủ hình native; pin bên ngoài vẫn là vật cản. Kiểm tra này không tự cho phép giao hình mới và không chứng minh lời giải.

`tools/assembly_contact_audit_v3.py` phát hiện giao solid/trigger mới giữa các macro, chỉ cho phép penetration đã có ở đúng cặp và đúng vùng hình nguồn nhập vào. Circle bomb chỉ được raster hóa gần đúng cho phép kiểm tra thăm dò; collider xuất vẫn giữ CircleCollider2D chính xác. Các kiểm tra này không thay thế solver/native replay.

Quy tắc nội dung: được tái sử dụng từng asset/template gốc với đúng hình dạng, material và collider. Bàn chơi mới phải có bố cục và quan hệ mới. Bản sao toàn bàn, bản phản chiếu/quay/đổi màu, hoặc thay đổi rất nhỏ vẫn bị loại. `tools/layout_novelty_v3.py` kiểm tra bố cục bằng alignment hình học độc lập ID nguồn/template, gồm quay/phản chiếu và co giãn khoảng cách nhẹ; đây là heuristic near-copy, không phải bằng chứng tuyệt đối về độ mới. Ngoài 500 bàn train, 7 bàn native tương thích khác (10210–10216) chỉ dùng làm exclusion, tổng 507 nguồn; không đưa chúng vào train hoặc nhãn độ khó. Một bố cục thủng/rơi cơ chế có thể lọt heuristic độ mới do khác inventory, nên phải qua kiểm chứng cơ chế/bố cục riêng. Cổng xuất vẫn khóa đến khi replay native và chuẩn độ khó đạt.

Mục tiêu cuối là ít nhất 300 level mới tương ứng tiến trình nguồn 1–300, học từ toàn bộ 500 level. Các pilot cũ trải 1–500 chỉ để QA độ đa dạng; không nén độ khó 500 level vào 300 level đầu. Tutorial có quá ít trạng thái mới hợp lệ phải báo giới hạn, không thêm nhiều chi tiết để ép mới.

Vật lý và chiến thắng: mọi gameplay root đăng ký với solve service đều phải được giải, bao gồm vòng/thanh/guide/butterfly có `CanCollect=0`. `CanCollect` chỉ phục vụ phần thưởng thu thập; không quyết định có phải giải vật thể hay không. Bomb đứng riêng không phải một root mục tiêu độc lập. Cần replay nguyên bản đại diện cho thanh thẳng/L/U, guide, butterfly, bomb, xích và từng feature binding trước khi tạo nhãn độ khó.

`tools/rotate_rings_physics_v3.py` tách state machine Pull từ oracle va chạm. Nó giữ signed axis gốc, frame/substep, initial overlap escape, đúng collider được miễn bởi passive binding, rollback chỉ lúc thả tay và xử lý release trước khi xét giải. Backend native yêu cầu phản hồi Unity chính xác và dừng rõ ràng khi thiếu query. Backend Shapely chỉ dùng kiểm tra overlap thăm dò; không được tạo nhãn lời giải hoặc độ khó. Chín kiểm thử contract không thay thế kiểm chứng runtime.

Đã trích thêm 15.652 hợp đồng vùng dò từ đủ 500 nguồn: 12.235 query shape của RingObject/RingRelative và 3.417 của PullObject (3.982 instance thanh kéo), không có conflict hoặc tham chiếu chưa giải. `data/source_overlap_queries.json` giữ collider shapeIndex và toàn bộ ContactFilter2D native. Khóa active của chủ pin và khóa passive của vật nhận có state riêng, được lưu/khôi phục khi undo hoặc replay. Input dùng graph cấu hình và query RingRelative riêng. Pull có quy tắc xóa binding khi collider đã đăng ký rời ra, khác vòng. Các phép đo diagnostic pose chỉ kiểm tra các luật này; chúng không phải đường kéo hợp lệ hoặc lời giải. Các chứng nhận cũ trước thay đổi luật phải chạy lại; batch full500 cũ đã dừng ở 268/500 và không được dùng để nhận level. Native parity toàn game và bộ 300 vẫn chưa hoàn tất.

Catalog mới giữ cả Rigidbody2D gốc, PhysicsMaterial2D, sourceRole và localZ từng collider. Tách chính xác 11.277 DragArea PolygonCollider2D (layer 6) để picking; không đưa chúng thành vật cản gameplay. 53.933 collider gameplay/bomb còn lại giữ hình và layer riêng. Một batch 242 primitive query đã đo được với Unity; snapshot đầu vào cũ được lưu riêng và không dùng để chứng nhận catalog mới thay đổi. Primitive cast/distance/overlap không thay thế replay một level.

Chạy lại đánh giá từ thư mục workspace:

```powershell
python tools\layout_learning_v3.py --workspace .
python tools\global_composition_v3.py --workspace .
python tools\macro_assembly_v3.py --workspace . --train
python tools\macro_pattern_v3.py --workspace .
python tools\composition_field_v3.py --workspace .
python tools\test_physics_v3_contract.py
python tools\test_native_socket_v3.py
python tools\test_assembly_contact_v3.py
python tools\test_assembly_integrity_v3.py
```

`Open Generator.vbs` mở giao diện cục bộ với cổng sinh level đang khóa và thông báo lý do. Sau khi native parity, replay nguồn, bộ ghép bố cục, kiểm tra độ mới và chuẩn độ khó đều đạt, mới mở sinh và xuất đủ ít nhất 300 level. Không tạo build trong workflow này.

Đã rà hình tổng thể của đủ 500 nguồn, tách nhãn nhìn thấy và nhóm suy luận trong `reports/source500_motif_inventory_first500.json`. Các bố cục kim cương, tên lửa, hộp quà, thuyền, bông tuyết, gương mặt và dạng đối xứng có chủ đích phải giữ hình nhận diện cùng cấu trúc nổi bật bên trong. Mô hình khoảng cách field hoặc số lượng chi tiết không đủ phân loại các hình này. Pilot kim cương 7953000 chỉ thay ba thanh ngoài viền và một thanh biên, không thay các facet bên trong, nên bị loại khỏi nhánh có thể chấp nhận và chỉ giữ làm nghiên cứu. Chưa có bộ sinh hình đặc biệt đạt yêu cầu.

Kiểm tra luật hiện tại có 17 cặp chốt đại diện, 3.672 pose diagnostic và 19.034 assertion hướng active/passive đều đạt; đây là các pose kiểm tra tách rời, không phải đường kéo hợp lệ. Tám nguồn đại diện qua kiểm tra input; tìm kiếm 15 giây/màn tìm và replay được sáu màn, hai màn 10010 và 10190 còn `unknown`. Không dùng kết quả này để khẳng định toàn bộ 500 màn hoặc toàn game đã đạt parity. Audit tĩnh đủ 7.295 vòng xác nhận tập probe penetration khớp các danh sách collider native, không có thiếu/thừa; tập probe đúng không tự chứng minh đường giải.

`tools/straight_pull_obstruction_diagnostic_v3.py` bổ sung graph hình học thăm dò cho bàn chỉ có thanh thẳng lồi và guide: giới hạn sweep đến lúc thanh rời guide về mặt hình học. Nó tránh tính những vật cản ở quá xa sau khoảng thoát guide. Năm regression hình học tổng hợp đã đạt, nhưng công cụ không mô phỏng cast skin, rollback, thời gian hay release native; không được dùng làm nhãn lời giải hoặc độ khó. Với kim cương nguồn và pilot 7953000, mỗi bàn có 30 cạnh thăm dò; chỉ ba cạnh thêm và ba cạnh bỏ, các tập đích đi ra từ facet2–6 giữ nguyên. Chứng cứ này không thay thế replay Unity hoặc đánh giá độ hay.

Cập nhật theo yêu cầu mới: không khóa cứng số lượng vật thể/cụm, mật độ chốt,
góc sửa hay tỷ lệ root phải đổi. Tách thêm vài cụm vẫn có thể đạt nếu bố cục
hợp lý và độ khó tương ứng nguồn. Các số liệu này là đặc trưng học/xếp hạng,
không phải điều kiện thiết kế cố định. Lượt staging cũ đã dừng ở 74 mẫu chưa
được nhận; các ngưỡng cũ chỉ còn giá trị lịch sử. Chính sách đang dùng nằm tại
`batches/user_revised_soft_policy_v4/policy.json`.

Review đủ 10 sheet của snapshot 57 mẫu được lưu trong
`reports/staged57_visual_composition_review.md`. Audit cấu trúc phát hiện 35
mẫu có nhiều cụm pin/guide hơn và 26 mẫu có root di động mới bị tách riêng;
đây là dấu hiệu cần xem tác động chơi, không phải lỗi tự động. Hai cặp thử ở
vị trí 2 và 7 giải/replay được và cùng số segment của solver, nhưng số root
cần tác động khác nhau. Chưa có kết luận độ khó ngang nhau. Cặp vị trí 26 có
nguồn giải được, mẫu mới còn unknown. Báo cáo so sánh lưu rõ hash luật lịch sử,
thông số ngân sách và giá trị thiếu; không biến unknown thành level vô nghiệm.

Audit guide đầy đủ 500 nguồn xác định 41 quan hệ vòng-guide có 117 collider
pin/wall nằm ngoài query guide của mã gốc. Không có collider dư nào chạm guide
ở pose ban đầu. Runtime đã chuyển xét guide sang đúng các overlap query gốc,
giữ ContactFilter2D và khớp chính xác cả owner lẫn collider của slot butterfly.
Các kiểm thử và nhãn replay phải gắn hash mới sau thay đổi này; báo cáo cũ
không tự chứng nhận runtime hiện tại.

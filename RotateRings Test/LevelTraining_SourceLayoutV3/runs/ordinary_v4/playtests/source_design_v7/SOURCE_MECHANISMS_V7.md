# Học cơ chế từ các màn gốc

Đây là các phiên điều khiển từng thao tác qua runtime Unity, có kiểm tra va chạm, gỡ chốt, bom, xích và điều kiện thắng. Chưa xác nhận thao tác chuột/điện thoại hay parity trực tiếp với APK.

| Màn gốc | Source | Nhãn gốc | Lần dò đầu | Lần kiểm tra lại |
|---|---|---|---|---|
|39|70033|VeryHard|25 bước|17 bước|
|49|60011|VeryHard|32 đoạn theo witness cũ|Đã bác bỏ việc 3 đoạn đầu cần thiết cho lần tháo đầu|
|129|10017|VeryHard|24 bước|12 bước|
|175|71029|Hard|51 bước|31 bước, còn bước thử dư|
|285|10187|Hard|Nhánh bom dương dừng ở26 bước, chưa giải|Nhánh bom âm thắng46 bước, còn bước thử dư|

Các số trên là đường đi tìm được, không phải số bước tối ưu hay độ khó con người. Số màn trên HUD cũ dùng chỉ số corpus sau khi bỏ màn tạo hình:39 thành37,49 thành46. HUD đã sửa dùng progression position và kèm source ID.

## Bài học có dấu vết thao tác

- Source39: slider12 initially hits10; moving10 hits11. Parking11 opens travel for10, then12 can leave before11. No authored pin needs to disappear during parking.
  Áp dụng cho bộ sinh: Learn transverse slider parking and shared travel corridors with actual native collision outcomes; a pin DAG misses this.
- Source39: bottom sliders21/20 block rotations7/8 despite no authored pin or guide binding between those components. Removing the bottom sliders permits the tested continued rotations.
  Áp dụng cho bộ sinh: Model collision/swept-space dependencies as well as authored bindings. Do not classify pin-disconnected groups as automatically irrelevant or independent.
- Source129: rotating bomb host1 from90 to101 degrees removes host1 and neighboring closed ring2. Requested45 degrees is truncated by native mid-gesture explosion.
  Áp dụng cho bộ sinh: Preserve bomb geometry and reachable contact, not just bomb count; closed rings cannot be treated as generic gapped ring variants.
- Source175 begins with chain counts22 and15. Ring11 rotation is rejected while locked. Bomb host28 removes5/16/28 and both chain counts fall by3; later nonfixed removals unlock11 before10.
  Áp dụng cho bộ sinh: Learn order constraints coupling distant branches and nonfixed solve counts; validate reachability of unlock thresholds rather than inventing a fixed quota.
- Source175: ring11 releases12 at210deg then8/11 at30deg. Later ring10 releases13 near120deg then itself near300deg. One gap serves different occupied directions in sequence.
  Áp dụng cho bộ sinh: Learn multireceiver fanout and release order, not only single-edge pin chains.
- Source285: ring13 at240deg opens one butterfly14 slot without removing it; ring12 at60deg opens the other and removes butterfly14. Rings13/12 then need different angles300/0 to release closed center10.
  Áp dụng cho bộ sinh: Keep complete two-slot catch geometry; learn staged goal alignment and subsequent reorientation, not a generic fixed catch substitution.
- Source285: ring7 attempting to continue toward180deg is blocked by ring6 head wall. Returning7 near86deg permits6 to rotate from211 to241deg; then7 can rotate through126 to181deg and remove7/9. Ring6 then rotates back near68deg to remove5/6.
  Áp dụng cho bộ sinh: Learn intermediate clearance poses and alternating actions between rings. Scalar pin depth and count miss these state-dependent paths.
- Source285: initial slider1 positive direction removes1/3; after restart negative direction removes1/2. Negative branch reaches goal. Positive branch was abandoned after26 moves; not proved impossible.
  Áp dụng cho bộ sinh: Preserve reachable bomb contacts and branch-specific consequences; do not model bombs as unconditional identical clearing actions.

Log gốc, hash và chỉ số bước nằm trong `source_mechanism_lessons_v7.json`. Không tính bất cứ mẫu mới nào là level được nghiệm thu từ báo cáo này.

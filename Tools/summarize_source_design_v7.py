"""Checkpoint observed source-design lessons; never infer optimal/human difficulty."""
from pathlib import Path
import hashlib
import json

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4'
OUT = WORK / 'playtests/source_design_v7'


def main():
    names = [
        'source60011_veryhard49_counterfactual.json',
        'source70033_veryhard39_after_restart_v7.json',
        'source70033_veryhard39_counterfactual_v7.json',
        'source70033_veryhard39_clean17_v7.json',
        'source10017_veryhard129_bomb_exploration_v7.json',
        'source10017_veryhard129_counterfactual_clean10_v7.json',
        'source71029_hard175_bomb_chain_exploration_v7.json',
        'source71029_hard175_counterfactual_v7.json',
        'source10187_hard285_butterfly_exploration_v7.json',
    ]
    reports = {}
    entries = []
    for name in names:
        path = OUT / name
        report = json.loads(path.read_text(encoding='utf-8-sig'))
        reports[name] = report
        last = report['steps'][-1]
        assert last['won'] and last['after']['remainingParts'] == 0, name
        entries.append(dict(file=name, sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                            sourceId=report['levelId'], originalPosition=report['position'],
                            gestureTrials=sum(s['action'] in ('rotate','slide') for s in report['steps']),
                            finalAttemptMoves=last['after']['moves'], reachedGoal=True,
                            runtimeSha256=report['runtimeSha256'], rulesSha256=report['rulesSha256']))

    def proof(name, index, **expected):
        step = next(s for s in reports[name]['steps'] if s['index'] == index)
        for key, value in expected.items():
            assert step[key] == value, (name, index, key, step[key], value)
        return dict(file=name, step=index, partId=step['partId'], action=step['action'],
                    requested=step['requestedAmount'], reachedBefore=step['reachedBefore'],
                    reachedAfter=step['reachedAfter'], blockerOwner=step['blockerOwner'],
                    blockerRole=step['blockerRole'], removed=step['removedDelta'])

    a = 'source70033_veryhard39_counterfactual_v7.json'
    b = 'source10017_veryhard129_bomb_exploration_v7.json'
    c = 'source71029_hard175_bomb_chain_exploration_v7.json'
    d = 'source71029_hard175_counterfactual_v7.json'
    e = 'source10187_hard285_butterfly_exploration_v7.json'
    lessons = [
        dict(id='shared_slider_corridor', status='observed_for_tested_exits',
             finding='Source39: slider12 initially hits10; moving10 hits11. Parking11 opens travel for10, then12 can leave before11. No authored pin needs to disappear during parking.',
             evidence=[proof(a,1,partId=12,blockerOwner=10), proof(a,3,partId=10,blockerOwner=11),
                       proof(a,5,partId=11,removedDelta=[]), proof(a,6,partId=10,removedDelta=[]),
                       proof(a,7,removedDelta=[12,17]), proof(a,8,removedDelta=[11,16])],
             generationImplication='Learn transverse slider parking and shared travel corridors with actual native collision outcomes; a pin DAG misses this.'),
        dict(id='physical_bridge_across_binding_components', status='observed_for_tested_directions',
             finding='Source39: bottom sliders21/20 block rotations7/8 despite no authored pin or guide binding between those components. Removing the bottom sliders permits the tested continued rotations.',
             evidence=[proof(a,11,partId=7,blockerOwner=21), proof(a,12,partId=8,blockerOwner=20),
                       proof(a,20,removedDelta=[21,22]), proof(a,21,removedDelta=[19,20]),
                       proof(a,22,removedDelta=[3,7]), proof(a,23,removedDelta=[8])],
             generationImplication='Model collision/swept-space dependencies as well as authored bindings. Do not classify pin-disconnected groups as automatically irrelevant or independent.'),
        dict(id='bomb_contact_clears_closed_body', status='observed_native_clone_and_original_code_backed',
             finding='Source129: rotating bomb host1 from90 to101 degrees removes host1 and neighboring closed ring2. Requested45 degrees is truncated by native mid-gesture explosion.',
             evidence=[proof(b,1,partId=1,removedDelta=[1,2])],
             generationImplication='Preserve bomb geometry and reachable contact, not just bomb count; closed rings cannot be treated as generic gapped ring variants.'),
        dict(id='chain_progression_gate', status='observed_native_clone_and_original_code_backed',
             finding='Source175 begins with chain counts22 and15. Ring11 rotation is rejected while locked. Bomb host28 removes5/16/28 and both chain counts fall by3; later nonfixed removals unlock11 before10.',
             evidence=[proof(c,1,partId=11,rejectionReason='owner-locked'),
                       proof(c,2,partId=28,removedDelta=[5,16,28])],
             generationImplication='Learn order constraints coupling distant branches and nonfixed solve counts; validate reachability of unlock thresholds rather than inventing a fixed quota.'),
        dict(id='sequential_opposed_pin_release', status='observed_successful_sequence_not_unique_or_optimal',
             finding='Source175: ring11 releases12 at210deg then8/11 at30deg. Later ring10 releases13 near120deg then itself near300deg. One gap serves different occupied directions in sequence.',
             evidence=[proof(d,28,partId=11,removedDelta=[12]), proof(d,29,partId=11,removedDelta=[8,11]),
                       proof(d,30,partId=10,removedDelta=[13]), proof(d,31,partId=10,removedDelta=[10])],
             generationImplication='Learn multireceiver fanout and release order, not only single-edge pin chains.'),
        dict(id='butterfly_two_slot_staging', status='observed_native_clone_and_original_code_backed',
             finding='Source285: ring13 at240deg opens one butterfly14 slot without removing it; ring12 at60deg opens the other and removes butterfly14. Rings13/12 then need different angles300/0 to release closed center10.',
             evidence=[proof(e,5,partId=13,removedDelta=[]), proof(e,6,partId=12,removedDelta=[14]),
                       proof(e,12,removedDelta=[13]), proof(e,13,removedDelta=[12])],
             generationImplication='Keep complete two-slot catch geometry; learn staged goal alignment and subsequent reorientation, not a generic fixed catch substitution.'),
        dict(id='alternating_two_ring_clearance', status='observed_successful_continuation_not_necessity_proof',
             finding='Source285: ring7 attempting to continue toward180deg is blocked by ring6 head wall. Returning7 near86deg permits6 to rotate from211 to241deg; then7 can rotate through126 to181deg and remove7/9. Ring6 then rotates back near68deg to remove5/6.',
             evidence=[proof(e,59,partId=7,blockerOwner=6), proof(e,63,partId=7,removedDelta=[]),
                       proof(e,64,partId=6,removedDelta=[]), proof(e,65,partId=7,removedDelta=[]),
                       proof(e,67,removedDelta=[7,9]), proof(e,68,removedDelta=[5,6])],
             generationImplication='Learn intermediate clearance poses and alternating actions between rings. Scalar pin depth and count miss these state-dependent paths.'),
        dict(id='bomb_direction_selects_different_target', status='observed_two_branches_only_one_completed',
             finding='Source285: initial slider1 positive direction removes1/3; after restart negative direction removes1/2. Negative branch reaches goal. Positive branch was abandoned after26 moves; not proved impossible.',
             evidence=[proof(e,2,partId=1,removedDelta=[1,3]), proof(e,30,partId=1,removedDelta=[1,2])],
             generationImplication='Preserve reachable bomb contacts and branch-specific consequences; do not model bombs as unconditional identical clearing actions.'),
    ]
    payload = dict(schemaVersion=7, status='SOURCE_MECHANISM_RESEARCH_NOT_DIFFICULTY_ACCEPTANCE',
                   sourceReplayMethod='Programmatic gestures through live Unity native rules; no OS pointer/raycast input. Original APK code supports mechanics but this is not direct APK runtime parity certification.',
                   reports=entries, lessons=lessons,
                   cautions=[
                       'Original39 solved17 moves versus25 on exploration; neither proves optimality.',
                       'Original129 solved12 versus24 exploration moves. Filename clean10 was a proposed target, not the actual result.',
                       'Original175 solved31 versus51 exploration moves; final31 still contains avoidable failed directions.',
                       'Original285 negative bomb branch solved46 moves after restart; positive branch abandoned unresolved after26. Neither branch result proves optimality or unsolvability.',
                       'Original49 historical32-segment witness contains unnecessary early preparation for the first observed removal.',
                       'No-removal actions and search path length do not establish intrinsic difficulty or human skill.',
                       'Actual shipped default tags:39/49/129 VeryHard,175/285 Hard. Tags are not human outcome measurements.',
                       'No new final accepted levels; 15% human difficulty matching remains unmeasured.',
                   ])
    (OUT/'source_mechanism_lessons_v7.json').write_text(json.dumps(payload,indent=2,ensure_ascii=False),encoding='utf-8')
    lines = ['# Học cơ chế từ các màn gốc', '',
             'Đây là các phiên điều khiển từng thao tác qua runtime Unity, có kiểm tra va chạm, gỡ chốt, bom, xích và điều kiện thắng. Chưa xác nhận thao tác chuột/điện thoại hay parity trực tiếp với APK.', '',
             '| Màn gốc | Source | Nhãn gốc | Lần dò đầu | Lần kiểm tra lại |',
             '|---|---|---|---|---|',
             '|39|70033|VeryHard|25 bước|17 bước|',
             '|49|60011|VeryHard|32 đoạn theo witness cũ|Đã bác bỏ việc 3 đoạn đầu cần thiết cho lần tháo đầu|',
             '|129|10017|VeryHard|24 bước|12 bước|',
             '|175|71029|Hard|51 bước|31 bước, còn bước thử dư|',
             '|285|10187|Hard|Nhánh bom dương dừng ở26 bước, chưa giải|Nhánh bom âm thắng46 bước, còn bước thử dư|', '',
             'Các số trên là đường đi tìm được, không phải số bước tối ưu hay độ khó con người. Số màn trên HUD cũ dùng chỉ số corpus sau khi bỏ màn tạo hình:39 thành37,49 thành46. HUD đã sửa dùng progression position và kèm source ID.', '',
             '## Bài học có dấu vết thao tác', '']
    for lesson in lessons:
        lines += ['- '+lesson['finding'], '  Áp dụng cho bộ sinh: '+lesson['generationImplication']]
    lines += ['', 'Log gốc, hash và chỉ số bước nằm trong `source_mechanism_lessons_v7.json`. Không tính bất cứ mẫu mới nào là level được nghiệm thu từ báo cáo này.']
    (OUT/'SOURCE_MECHANISMS_V7.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    print(json.dumps({'reports':len(entries),'lessons':len(lessons),'acceptedNewLevels':0}))


if __name__ == '__main__':
    main()

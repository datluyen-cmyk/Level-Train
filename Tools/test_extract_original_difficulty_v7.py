import unittest
import struct
import brotli
from extract_original_difficulty_v7 import decode_difficulty_table,resolve_default


class ShippedDifficultyRules(unittest.TestCase):
    def test_memorypack_full_consumption_and_enum(self):
        row=struct.pack("<iBiiiiiiii",7,7,7,0,19,35,10,1,9,2)
        payload=struct.pack("<i",1)+row
        compressed=brotli.compress(payload)
        meta=struct.pack("<i",1)+struct.pack("<Biqii",5,1,0,0,len(compressed))
        rules,evidence=decode_difficulty_table(meta,compressed,index=0)
        self.assertEqual(rules[0]["difficultyName"],"VeryHard")
        self.assertEqual(evidence["consumedBytes"],len(payload))
        self.assertEqual(resolve_default(rules,29),(2,7))
        self.assertEqual(resolve_default(rules,39),(0,None))
        malformed=brotli.compress(payload+b"x")
        bad_meta=struct.pack("<i",1)+struct.pack("<Biqii",5,1,0,0,len(malformed))
        with self.assertRaisesRegex(ValueError,"Trailing"):
            decode_difficulty_table(bad_meta,malformed,index=0)

    def test_serialized_rule_order_and_mode_are_preserved(self):
        broad={"GameMode":0,"Begin":1,"End":100,"Divisor":10,"Remainder":[9],"DifficultMode":1,"Id":2}
        later={**broad,"DifficultMode":2,"Id":3}
        self.assertEqual(resolve_default([broad,later],39),(1,2))
        self.assertEqual(resolve_default([broad,later],39,mode=1),(0,None))


if __name__=="__main__":unittest.main()

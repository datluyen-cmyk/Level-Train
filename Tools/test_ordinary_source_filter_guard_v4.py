"""Categorical user scope, source provenance and refit guard regressions."""
import copy
import unittest

from ordinary_source_filter_guard_v4 import validate_source_filter, validate_refit_manifest


def curated():
    return {"curationStatus": "CURATED_ORDINARY_TRAINING_CORPUS", "sourcePackSha256": "pack",
            "ordinarySourceIds": [str(i) for i in range(500)], "excludedSourceIds": ["10157"],
            "notAutomaticallyExcluded": ["ordinary_radial", "ordinary_grid"]}


class OrdinarySourceFilterGuard(unittest.TestCase):
    def test_curated500_ordinary_sources_pass(self):
        self.assertEqual(len(validate_source_filter(curated(), "pack", range(500))), 500)

    def test_pending_curation_blocks_before_training(self):
        manifest = curated()
        manifest["curationStatus"] = "PENDING_ORDINARY_CURATED_CORPUS"
        with self.assertRaisesRegex(ValueError, "pending"):
            validate_source_filter(manifest, "pack")

    def test_explicit_pictorial_source_cannot_remain(self):
        manifest = curated()
        manifest["ordinarySourceIds"][-1] = "10157"
        with self.assertRaisesRegex(ValueError, "pictorial"):
            validate_source_filter(manifest, "pack")

    def test_radial_grid_labels_are_not_inferred_as_pictures(self):
        manifest = curated()
        manifest["ordinaryRoleAnnotations"] = {"1": "ordinary_radial", "2": "ordinary_grid"}
        self.assertIn("1", validate_source_filter(manifest, "pack"))
        self.assertIn("2", validate_source_filter(manifest, "pack"))

    def test_duplicates_rejected_but_exhausted_available_corpus_allowed(self):
        manifest = curated()
        manifest["ordinarySourceIds"][-1] = "1"
        with self.assertRaisesRegex(ValueError, "Duplicate"):
            validate_source_filter(manifest, "pack")
        manifest = curated()
        manifest["ordinarySourceIds"].pop()
        manifest.update(datasetExhausted=True, requestedTrainingSourceCount=500, actualUniqueOrdinaryCount=499)
        self.assertEqual(len(validate_source_filter(manifest, "pack")), 499)
        manifest["actualUniqueOrdinaryCount"] = 500
        with self.assertRaisesRegex(ValueError, "count provenance"):
            validate_source_filter(manifest, "pack")

    def test_stale_or_actual_mismatched_source_pack_rejected(self):
        with self.assertRaisesRegex(ValueError, "stale"):
            validate_source_filter(curated(), "different-pack")
        with self.assertRaisesRegex(ValueError, "Actual source"):
            validate_source_filter(curated(), "pack", range(1, 501))

    def test_refit_certificate_binds_filter_source_and_exact_model(self):
        manifest = {"status": "REFIT_ON_CURATED_ORDINARY_CORPUS", "sourcePackSha256": "pack", "sourceFilterSha256": "filter",
                    "models": {"model.joblib": {"sha256": "new-model"}}}
        validate_refit_manifest(manifest, "pack", "filter", {"model.joblib": "new-model"})
        for key in ("sourcePackSha256", "sourceFilterSha256"):
            changed = copy.deepcopy(manifest)
            changed[key] = "stale"
            with self.assertRaisesRegex(ValueError, "stale"):
                validate_refit_manifest(changed, "pack", "filter", {"model.joblib": "new-model"})
        with self.assertRaisesRegex(ValueError, "model binding"):
            validate_refit_manifest(manifest, "pack", "filter", {"model.joblib": "historical-model"})


if __name__ == "__main__":
    unittest.main()

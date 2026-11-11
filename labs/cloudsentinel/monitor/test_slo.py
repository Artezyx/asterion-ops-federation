import unittest
from slo import evaluate


class SloTests(unittest.TestCase):
    def test_all_success_and_nearest_rank_p95(self):
        samples = [{"latency_ms": str(n * 10), "status_code": "200"} for n in range(1, 101)]
        result = evaluate(samples, latency_budget_ms=950)
        self.assertEqual(result["p95_latency_ms"], 950)
        self.assertTrue(result["availability_met"])
        self.assertFalse(result["alert"])
        self.assertEqual(result["remaining_error_budget_requests"], 1)

    def test_outage_triggers_alert(self):
        samples = [{"latency_ms": "300", "status_code": "503"}] + [
            {"latency_ms": "100", "status_code": "200"} for _ in range(19)
        ]
        result = evaluate(samples, availability_target=0.99)
        self.assertTrue(result["alert"])
        self.assertFalse(result["availability_met"])
        self.assertEqual(result["remaining_error_budget_requests"], 0)

    def test_missing_or_invalid_metric_fails_closed(self):
        with self.assertRaises(ValueError):
            evaluate([{"latency_ms": "nan", "status_code": "200"}])
        with self.assertRaises(ValueError):
            evaluate([])
        with self.assertRaises(ValueError):
            evaluate([{"latency_ms": "4", "status_code": "700"}])


if __name__ == "__main__":
    unittest.main()

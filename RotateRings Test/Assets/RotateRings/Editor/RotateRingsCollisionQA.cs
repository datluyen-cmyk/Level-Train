#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Source-template collision regression. Run from Tools/Rotate Rings/QA after script compilation.</summary>
public static class RotateRingsCollisionQA
{
    const int ObstacleId = 1;
    const int MovingId = 2;
    const string FixtureLevelId = "10001";

    [Serializable] sealed class Corpus { public SourceLevel[] levels; }
    [Serializable] sealed class SourceLevel { public string id; public SourceState state; }
    [Serializable] sealed class SourceState { public RotateRingsPart[] parts; public RotateRingsLink[] links; }
    [Serializable] sealed class StemPose { public string obstacleTemplateId; public string movingTemplateId; public float obstacleAngle; public Vector2 center; public float angle; public float delta; public float firstContactAt; }
    [Serializable] sealed class Check { public string name; public bool passed; public string detail; }
    [Serializable] sealed class Report
    {
        public string status;
        public bool passed;
        public string unityVersion;
        public string fixtureLevelId;
        public string obstacleTemplateId;
        public string movingTemplateId;
        public string qaHarnessSha256;
        public string collisionWorldSha256;
        public string sourceCatalogSha256;
        public string sourceFixtureSha256;
        public List<string> blockingRoles = new List<string>();
        public Vector2 movingCenter;
        public float initialAngle;
        public float attemptedDelta;
        public float clampedAngle;
        public List<Check> checks = new List<Check>();
        public string error;
    }
    sealed class Scenario
    {
        public float obstacleAngle;
        public Vector2 center;
        public float angle;
        public float delta;
        public float clamped;
        public string role;
    }

    [MenuItem("Tools/Rotate Rings/QA/Original Collision Backend")]
    public static void RunMenu() { Debug.Log("Collision QA report: " + Run()); }

    public static string Run()
    {
        var report = new Report { status = "running", unityVersion = Application.unityVersion, fixtureLevelId = FixtureLevelId };
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
        string research = Path.Combine(root, "Research", "physics_v2");
        string reportPath = Path.Combine(research, "unity_collision_qa.json");
        report.qaHarnessSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings", "Editor", "RotateRingsCollisionQA.cs"));
        report.collisionWorldSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings", "Scripts", "RotateRingsCollisionWorld.cs"));
        report.sourceCatalogSha256 = Hash(Path.Combine(research, "templates.json"));
        report.sourceFixtureSha256 = Hash(Path.Combine(research, "corpus500.json"));
        try
        {
            string corpusPath = Path.Combine(research, "corpus500.json");
            string catalogPath = Path.Combine(research, "templates.json");
            string stemPosePath = Path.Combine(research, "qa_stem_pose.json");
            Require(File.Exists(corpusPath) && File.Exists(catalogPath) && File.Exists(stemPosePath), "Source corpus, templates and stem pose fixture are required.");
            report.sourceFixtureSha256 = Hash(corpusPath) + ":" + Hash(stemPosePath);
            var corpus = JsonUtility.FromJson<Corpus>(File.ReadAllText(corpusPath));
            var catalog = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(catalogPath));
            var source = Array.Find(corpus.levels, l => l.id == FixtureLevelId);
            Require(source != null && source.state != null && source.state.parts != null && source.state.parts.Length >= 2,
                "Source fixture level 10001 is missing its parts.");
            var obstacle = Array.Find(source.state.parts, p => p.id == ObstacleId);
            var moving = Array.Find(source.state.parts, p => p.id == MovingId);
            Require(obstacle != null && moving != null, "Fixture level 10001 requires source parts 1 and 2.");
            report.obstacleTemplateId = obstacle.templateId;
            report.movingTemplateId = moving.templateId;
            var obstacleTemplate = Array.Find(catalog.templates, t => t.id == obstacle.templateId);
            var movingTemplate = Array.Find(catalog.templates, t => t.id == moving.templateId);
            Require(obstacleTemplate != null && movingTemplate != null, "Fixture templates are absent from templates.json.");
            Require(HasShape(obstacleTemplate, "stem", false) && HasShape(obstacleTemplate, "head_wall", false) && HasShape(obstacleTemplate, "head_trigger", true),
                "T-bar source template must include solid stems/head walls and a trigger head.");
            Require(HasShape(movingTemplate, "body", false), "Moving source template must include its solid C-ring body.");
            Add(report, "Level 10001 original templates include solid stems, solid head walls, a trigger head and a solid C-ring body", true,
                obstacle.templateId + " / " + moving.templateId);
            var headWallScenario = new Scenario { obstacleAngle = 0f, center = new Vector2(-3.67051814f, .98351237f), angle = 315f, delta = 44.5f, clamped = 356.5f, role = "head_wall" };
            var headCatalog = FixtureCatalog(catalog, obstacleTemplate, movingTemplate);
            VerifyBlockedScenario(report, obstacle, moving, source.state.links, headCatalog, headWallScenario);
            report.blockingRoles.Add("head_wall");
            report.movingCenter = headWallScenario.center;
            report.initialAngle = headWallScenario.angle;
            report.attemptedDelta = headWallScenario.delta;
            report.clampedAngle = headWallScenario.clamped;

            var stemPose = JsonUtility.FromJson<StemPose>(File.ReadAllText(stemPosePath));
            var stemSource = Array.Find(corpus.levels, l => l.id == "70011");
            Require(stemPose != null && stemSource != null && stemSource.state != null, "Native stem pose fixture or source scene 70011 is missing.");
            var stemObstacle = Array.Find(stemSource.state.parts, p => p.templateId == stemPose.obstacleTemplateId);
            var stemTemplate = Array.Find(catalog.templates, t => t.id == stemPose.obstacleTemplateId);
            var stemMovingTemplate = Array.Find(catalog.templates, t => t.id == stemPose.movingTemplateId);
            Require(stemObstacle != null && stemTemplate != null && stemMovingTemplate != null && HasShape(stemTemplate, "stem", false),
                "The original level 70011 stem template is missing.");
            var stemMoving = new RotateRingsPart { id = MovingId, templateId = stemPose.movingTemplateId, kind = "ring", radius = 1.75f, gap = 90f };
            var stemObstacleCopy = Copy(stemObstacle); stemObstacleCopy.id = ObstacleId;
            var stemCatalog = FixtureCatalog(catalog, stemTemplate, stemMovingTemplate);
            var stemScenario = new Scenario { obstacleAngle = stemPose.obstacleAngle, center = stemPose.center, angle = stemPose.angle,
                delta = stemPose.delta, clamped = stemPose.firstContactAt, role = "stem" };
            report.obstacleTemplateId += ";" + stemObstacleCopy.templateId;
            report.movingTemplateId += ";" + stemMovingTemplate.id;
            VerifyBlockedScenario(report, stemObstacleCopy, stemMoving, FixtureLinks(), stemCatalog, stemScenario);
            report.blockingRoles.Add("stem");

            using (var triggerWorld = new WorldHarness(obstacle, moving, source.state.links, headCatalog,
                0f, new Vector2(-4.55590343f, 0f), 180f))
            {
                bool binding = triggerWorld.Collision.IsBinding(ObstacleId, MovingId);
                bool noHardOverlap = !triggerWorld.Collision.HasHardOverlap(MovingId);
                Add(report, "Authorized head trigger binds the C-ring without creating a hard overlap", binding && noHardOverlap,
                    "binding=" + binding + ", noHardOverlap=" + noHardOverlap + ", normalized source fixture center=(-4.55590343,0), angle=180");
            }
            report.passed = report.checks.TrueForAll(c => c.passed);
            report.status = report.passed ? "passed" : "failed";
        }
        catch (Exception ex)
        {
            report.error = ex.ToString();
            report.status = "failed";
            report.passed = false;
            Add(report, "Collision QA completed without exception", false, ex.GetType().Name + ": " + ex.Message);
        }
        finally
        {
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        }
        Debug.Log("Original collision backend QA " + report.status + ": " + reportPath);
        return reportPath;
    }

    static void VerifyBlockedScenario(Report report, RotateRingsPart obstacle, RotateRingsPart moving, RotateRingsLink[] links, RotateRingsTemplateCatalog catalog, Scenario scenario)
    {
        using (var world = new WorldHarness(obstacle, moving, links, catalog, scenario.obstacleAngle, scenario.center, scenario.angle))
        {
            bool initialValid = world.Collision.InitialPlacementValid();
            Add(report, scenario.role + " fixture begins without a hard overlap", initialValid,
                "center=" + scenario.center + ", obstacleAngle=" + scenario.obstacleAngle + ", movingAngle=" + scenario.angle);
            bool blocked;
            float reached = world.Collision.ClampRotation(MovingId, scenario.angle, scenario.delta, out blocked);
            string blockerRole = world.Collision.LastBlockerRole;
            // The fixture records the first invalid half-degree sample. The engine
            // bisects back to a legal pose inside that sample interval.
            float retreat = (scenario.clamped - reached) * Mathf.Sign(scenario.delta);
            bool forwardBlocked = blocked && blockerRole == scenario.role && retreat >= -.02f && retreat <= RotateRingsCollisionWorld.MaxAngularStep + .02f && !world.Collision.HasHardOverlap(MovingId);
            Add(report, scenario.role + " blocks the C-ring rotation at its expected stop", forwardBlocked,
                "blocked=" + blocked + ", role=" + blockerRole + ", reached=" + reached + ", firstInvalidSample=" + scenario.clamped + ", retreat=" + retreat);
            float reverseDelta = scenario.angle - reached;
            float reversed = world.Collision.ClampRotation(MovingId, reached, reverseDelta, out blocked);
            Add(report, "Reverse motion clears the " + scenario.role + " and returns to the start", !blocked && Mathf.Abs(Mathf.DeltaAngle(reversed, scenario.angle)) < .06f,
                "blocked=" + blocked + ", returned=" + reversed + ", start=" + scenario.angle);
        }
        using (var world = new WorldHarness(obstacle, moving, links, catalog, scenario.obstacleAngle, scenario.center, scenario.angle))
        {
            bool blocked;
            float large = scenario.delta > 0f ? 360f : -360f;
            float largeResult = world.Collision.ClampRotation(MovingId, scenario.angle, large, out blocked);
            bool largeStopped = blocked && Mathf.Abs(Mathf.DeltaAngle(largeResult, scenario.angle)) > .05f && Mathf.Abs(largeResult - (scenario.angle + large)) > 1f;
            Add(report, "360-degree frame request cannot tunnel through the " + scenario.role, largeStopped,
                "blocked=" + blocked + ", accepted=" + largeResult + ", requested=" + (scenario.angle + large));
        }
    }

    static RotateRingsTemplateCatalog FixtureCatalog(RotateRingsTemplateCatalog full, RotateRingsOriginalTemplate obstacle, RotateRingsOriginalTemplate moving) =>
        new RotateRingsTemplateCatalog { schemaVersion = full.schemaVersion, templates = new[] { obstacle, moving } };

    static RotateRingsLink[] FixtureLinks() => new[] { new RotateRingsLink { source = ObstacleId, attachmentId = 0, target = MovingId } };

    sealed class WorldHarness : IDisposable
    {
        public readonly RotateRingsCollisionWorld Collision;
        readonly GameObject host;
        public WorldHarness(RotateRingsPart originalObstacle, RotateRingsPart originalMoving, RotateRingsLink[] sourceLinks, RotateRingsTemplateCatalog catalog, float obstacleAngle, Vector2 center, float angle)
        {
            host = new GameObject("Collision QA isolated source geometry");
            Collision = host.AddComponent<RotateRingsCollisionWorld>();
            var obstacle = Copy(originalObstacle); obstacle.id = ObstacleId; obstacle.x = 0; obstacle.y = 0; obstacle.angle = obstacleAngle;
            var moving = Copy(originalMoving); moving.id = MovingId; moving.x = center.x; moving.y = center.y; moving.angle = angle;
            var links = new List<RotateRingsLink>();
            if (sourceLinks != null) foreach (var sourceLink in sourceLinks)
                if (sourceLink.source == ObstacleId && sourceLink.target == MovingId)
                    links.Add(new RotateRingsLink { source = sourceLink.source, target = sourceLink.target,
                        attachmentId = sourceLink.attachmentId, bearing = sourceLink.bearing, pinWidth = sourceLink.pinWidth });
            var level = new RotateRingsLevel { id = 10001, parts = new List<RotateRingsPart> { obstacle, moving }, links = links };
            Collision.Build(level, catalog);
            Collision.ApplyState(new Dictionary<int, float> { { ObstacleId, obstacleAngle }, { MovingId, angle } }, new HashSet<int>());
        }
        public void Dispose() { if (host != null) UnityEngine.Object.DestroyImmediate(host); }
    }

    static RotateRingsPart Copy(RotateRingsPart p) => new RotateRingsPart {
        id = p.id, templateId = p.templateId, kind = p.kind, x = p.x, y = p.y, radius = p.radius, width = p.width,
        gap = p.gap, angle = p.angle, color = p.color, collectible = p.collectible, key = p.key, lockKey = p.lockKey,
        bomb = p.bomb, axis = p.axis, length = p.length
    };
    static bool HasShape(RotateRingsOriginalTemplate t, string role, bool trigger) {
        foreach (var s in t.physicalShapes) if (s.role == role && s.trigger == trigger) return true;
        return false;
    }
    static string Hash(string path) {
        if (!File.Exists(path)) return string.Empty;
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
    }
    static void Add(Report r, string name, bool passed, string detail) => r.checks.Add(new Check { name = name, passed = passed, detail = detail });
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
#endif

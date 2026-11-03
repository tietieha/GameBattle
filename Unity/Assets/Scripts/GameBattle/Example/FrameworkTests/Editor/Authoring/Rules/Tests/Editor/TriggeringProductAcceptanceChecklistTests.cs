using System.IO;
using NUnit.Framework;

namespace AbilityKit.Triggering.Tests
{
    public sealed class TriggeringProductAcceptanceChecklistTests
    {
        [Test]
        public void Documentation_ReferencesFormalSampleAndDiagnostics()
        {
            var packageRoot = FindPackageRoot();
            var productGuide = File.ReadAllText(Path.Combine(packageRoot, "Document", "ProductAcceptanceGuide.md"));
            var stageClosureGuide = File.ReadAllText(Path.Combine(packageRoot, "Document", "StageClosureGuide.md"));
            var packageReadme = File.ReadAllText(Path.Combine(packageRoot, "Documentation~", "README.md"));

            StringAssert.Contains("Samples/FormalTriggeringMainlineExample.cs", productGuide);
            StringAssert.Contains("TriggeringDiagnosticCollector", productGuide);
            StringAssert.Contains("dotnet build Unity\\AbilityKit.Triggering.csproj --no-restore -v:minimal", productGuide);
            StringAssert.Contains("TriggeringRuntimePoolingHotspotScanTests", productGuide);
            StringAssert.Contains("TriggeringRuntimePools.CreateDefault", stageClosureGuide);
            StringAssert.Contains("dotnet build Unity\\AbilityKit.Triggering.Tests.csproj --no-restore -v:minimal", stageClosureGuide);
            StringAssert.Contains("Document/StageClosureGuide.md", packageReadme);
            StringAssert.Contains("AbilityKit/Triggering/Diagnostics/Write Runtime Report", packageReadme);
            StringAssert.Contains("AbilityKit/Triggering/Diagnostics/Write Product Acceptance Report", packageReadme);
        }

        [Test]
        public void FormalSample_UsesOnlyMainlineEntries()
        {
            var packageRoot = FindPackageRoot();
            var samplePath = Path.Combine(packageRoot, "Samples", "FormalTriggeringMainlineExample.cs");
            var text = File.ReadAllText(samplePath);

            StringAssert.Contains("TriggerRunner<ExampleContext>", text);
            StringAssert.Contains("TriggerPlanFactory.When<DamageEvent>", text);
            StringAssert.Contains("CompositeTriggerValidator<DamageEvent>", text);
            StringAssert.Contains("RuleSchedulerRegistry", text);
            Assert.That(text, Does.Not.Contain("Runtime.Scheduler"));
            Assert.That(text, Does.Not.Contain("TriggerDispatcherHub"));
            Assert.That(text, Does.Not.Contain("Runtime.Executable"));
        }

        [Test]
        public void CommercialChecklist_MarksProductizationItemsAsLanded()
        {
            var packageRoot = FindPackageRoot();
            var checklist = File.ReadAllText(Path.Combine(packageRoot, "Document", "Triggering-Commercial-Remediation-Checklist.md"));

            StringAssert.Contains("收敛正式主线 API（已落地）", checklist);
            StringAssert.Contains("统一调度体系职责（已落地第一轮）", checklist);
            StringAssert.Contains("建立正式 API 边界规范（已落地）", checklist);
            StringAssert.Contains("建立商业化验收基线（已落地产品验收指南与回归入口）", checklist);
            StringAssert.Contains("完善编辑器工具链（已完成诊断与验收报告入口）", checklist);
            StringAssert.Contains("增强测试矩阵（已落地产品化清单回归）", checklist);
            StringAssert.Contains("收敛命名与目录结构（已完成文档边界与样板收口）", checklist);
            StringAssert.Contains("规划下线 legacy 入口（已完成调用方防回流与下线跟踪）", checklist);
        }

        [Test]
        public void ProductAcceptanceReport_SummarizesChecklistOneToFive()
        {
            var packageRoot = FindPackageRoot();
            var menuPath = Path.Combine(packageRoot, "Editor", "Diagnostics", "TriggeringProductAcceptanceMenu.cs");
            var reportSource = File.ReadAllText(menuPath);

            StringAssert.Contains("## Checklist 1-5", reportSource);
            StringAssert.Contains("1. Mainline API convergence: completed.", reportSource);
            StringAssert.Contains("2. Scheduling responsibility split: completed.", reportSource);
            StringAssert.Contains("3. Formal API boundary policy: completed.", reportSource);
            StringAssert.Contains("4. Commercial acceptance baseline: completed.", reportSource);
            StringAssert.Contains("5. Editor diagnostics and acceptance reports: completed.", reportSource);
        }

        private static string FindPackageRoot()
        {
            var candidates = new[]
            {
                Path.Combine("Packages", "com.abilitykit.triggering"),
                Path.Combine("Unity", "Packages", "com.abilitykit.triggering"),
                Path.Combine("..", "Packages", "com.abilitykit.triggering"),
                Path.Combine("..", "Unity", "Packages", "com.abilitykit.triggering")
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                var fullPath = Path.GetFullPath(candidates[i]);
                if (Directory.Exists(fullPath))
                    return fullPath;
            }

            Assert.Fail("Unable to locate com.abilitykit.triggering package root.");
            return null;
        }
    }
}

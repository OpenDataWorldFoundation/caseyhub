using System.Text.Json;
using CaseyHub.API.Evaluators;
using CaseyHub.API.Repositories;
using CaseyHub.Core.Entities;
using CaseyHub.Core.Enums;
using CaseyHub.Models.DTOs.Internal.PermitChecker;
using Microsoft.Extensions.Logging;

namespace CaseyHub.API.Services;


/// <summary>
/// Orchestrates the full permit evaluation pipeline:
///
/// Stage 1 — Zone/Overlay Override Check
///   Load ZoneOverrideRules for the building type.
///   For each rule, check if the zone or any overlay code matches.
///   First match → immediate conclusive verdict. Pipeline ends.
///
/// Stage 2 — PermitRule Evaluation (priority order)
///   Load PermitRules + their Questions for the building type.
///   For each rule (ascending priority):
///     a. Collect required field keys from the rule's Questions.
///     b. If any field key is missing from ctx.Answers → the user hasn't answered this
///        question yet. Accumulate the question in the "needs more info" batch.
///        Continue to next rule (don't skip it — we may need its questions too).
///     c. If all fields are present → evaluate TriggerContextJson.
///        If condition matches and Outcome is conclusive → record triggered rule.
///        If ShortCircuitOnMatch → stop evaluating further rules.
///
/// Stage 3 — Response construction
///   If there are unanswered questions → return NeedsMoreInfo with deduplicated questions.
///   If there are triggered conclusive rules → return Conclusive verdict.
///   If no rules matched → return NoPermitRequired (the catch-all fires instead via DB).
/// </summary>
public class PermitEvaluatorService(
    IPermitCheckerRepository repo,
    IConditionEvaluator conditionEvaluator,
    ILogger<PermitEvaluatorService> logger) : IPermitEvaluatorService
{
    public async Task<EvaluationResponseDto> EvaluateAsync(EvaluationContext ctx, Guid? userId = null)
    {
        logger.LogInformation(
            "Starting evaluation: BuildingType={Type}, Zone={Zone}, Overlays={Overlays}, Answers={Count}",
            ctx.BuildingTypeSlug, ctx.ZoneCode, string.Join(",", ctx.OverlayCodes), ctx.Answers.Count);

        var buildingType = await repo.GetBuildingTypeBySlugAsync(ctx.BuildingTypeSlug);
        if (buildingType is null)
        {
            logger.LogError("Unknown building type slug: {Slug}", ctx.BuildingTypeSlug);
            throw new ArgumentException($"Unknown building type: {ctx.BuildingTypeSlug}");
        }

        // Collect the clauses in scope for the sidebar (updated after every call)
        var clausesInScope = await repo.GetClausesForZoneAsync(ctx.ZoneCode);

        // ──────────────────────────────────────────────────────────────────────
        // STAGE 1: Zone/Overlay Override Check
        // ──────────────────────────────────────────────────────────────────────
        var zoneOverrides = await repo.GetZoneOverrideRulesAsync(buildingType.Id);

        foreach (var overrideRule in zoneOverrides.Where(r => r.IsActive))
        {
            bool matches = overrideRule.PrefixMatch
                ? ctx.ZoneCode.StartsWith(overrideRule.ZoneOrOverlayCode, StringComparison.OrdinalIgnoreCase)
                  || ctx.OverlayCodes.Any(oc => oc.StartsWith(overrideRule.ZoneOrOverlayCode, StringComparison.OrdinalIgnoreCase))
                : string.Equals(ctx.ZoneCode, overrideRule.ZoneOrOverlayCode, StringComparison.OrdinalIgnoreCase)
                  || ctx.OverlayCodes.Contains(overrideRule.ZoneOrOverlayCode, StringComparer.OrdinalIgnoreCase);

            if (matches)
            {
                logger.LogInformation(
                    "Zone override MATCHED: Code={Code}, Outcome={Outcome}",
                    overrideRule.ZoneOrOverlayCode, overrideRule.Outcome);

                // Add the triggered clause to the in-scope list if not already there
                if (!clausesInScope.Any(c => c.Id == overrideRule.PlanningClauseId))
                    clausesInScope.Add(overrideRule.PlanningClause);

                var triggeredOverride = new TriggeredRuleDto(
                    RuleId: overrideRule.Id,
                    OutcomeReason: overrideRule.OutcomeReason,
                    Clause: MapClauseToDto(overrideRule.PlanningClause)
                );

                return BuildConclusiveResponse(
                    outcome: overrideRule.Outcome,
                    outcomeSummary: overrideRule.OutcomeReason,
                    triggeredRules: new List<TriggeredRuleDto> { triggeredOverride },
                    clausesInScope: clausesInScope,
                    ctx: ctx,
                    userId: userId
                );
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // STAGE 2: PermitRule evaluation
        // ──────────────────────────────────────────────────────────────────────
        var rules = await repo.GetPermitRulesWithQuestionsAsync(buildingType.Id);

        // Collect all unanswered questions across all rules before we evaluate
        // We do a TWO-PASS approach:
        //   Pass 1 — collect all missing questions from all rules
        //   Pass 2 — evaluate rules that have all their fields answered
        // This ensures the frontend always receives ALL outstanding questions in one
        // batch (one screen), not one at a time.

        var missingQuestions = new Dictionary<string, RuleQuestion>(); // keyed by FieldKey (deduplication)
        var triggeredRules = new List<TriggeredRuleDto>();
        bool shortCircuited = false;

        foreach (var rule in rules.Where(r => r.IsActive))
        {
            if (shortCircuited) break;

            // Add this rule's clause to the sidebar
            if (!clausesInScope.Any(c => c.Id == rule.PlanningClauseId))
                clausesInScope.Add(rule.PlanningClause);

            // Collect required field keys for this rule
            var requiredFieldKeys = rule.Questions
                .Select(q => q.FieldKey)
                .Distinct()
                .ToList();

            // Find which fields are missing from the answers
            var missingForThisRule = requiredFieldKeys
                .Where(fk => !ctx.Answers.ContainsKey(fk))
                .ToList();

            if (missingForThisRule.Count > 0)
            {
                // Accumulate missing questions (deduplicated by FieldKey)
                foreach (var fk in missingForThisRule)
                {
                    if (!missingQuestions.ContainsKey(fk))
                    {
                        var question = rule.Questions.First(q => q.FieldKey == fk);
                        missingQuestions[fk] = question;
                    }
                }
                // We cannot evaluate this rule yet — continue to collect other missing fields
                continue;
            }

            // All required fields are present — evaluate the condition
            bool conditionMet = conditionEvaluator.Evaluate(rule.TriggerContextJson, ctx);

            if (conditionMet)
            {
                logger.LogInformation(
                    "Rule MATCHED: Id={Id}, Priority={Priority}, Outcome={Outcome}",
                    rule.Id, rule.Priority, rule.Outcome);

                triggeredRules.Add(new TriggeredRuleDto(
                    RuleId: rule.Id,
                    OutcomeReason: rule.OutcomeReason,
                    Clause: MapClauseToDto(rule.PlanningClause)
                ));

                if (rule.ShortCircuitOnMatch)
                {
                    shortCircuited = true;
                    break;
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // STAGE 3: Build response
        // ──────────────────────────────────────────────────────────────────────

        // If there are unanswered questions → return NeedsMoreInfo
        if (missingQuestions.Count > 0)
        {
            logger.LogInformation("Evaluation needs more info — {Count} question(s) pending",
                missingQuestions.Count);

            return new EvaluationResponseDto(
                Status: "NeedsMoreInfo",
                Outcome: null,
                OutcomeSummary: null,
                NextQuestions: missingQuestions.Values
                    .OrderBy(q => q.DisplayOrder)
                    .Select(MapQuestionToDto)
                    .ToList(),
                TriggeredRules: null,
                ClausesInScope: clausesInScope.Select(MapClauseToDto).ToList(),
                AssessmentId: null
            );
        }

        // All rules evaluated — build conclusive response from the triggered rules
        // If multiple rules triggered (possible when ShortCircuitOnMatch = false),
        // the most severe outcome wins: PermitRequired > ReferToCouncil > Exempt > NoPermitRequired
        if (triggeredRules.Count > 0)
        {
            // The last-triggered rule's outcome is the verdict
            // (rules are evaluated in priority order; the last one to fire wins for non-short-circuit chains)
            var primaryTriggered = rules
                .Where(r => triggeredRules.Any(t => t.RuleId == r.Id))
                .OrderBy(r => r.Priority)
                .Last();

            return BuildConclusiveResponse(
                outcome: primaryTriggered.Outcome,
                outcomeSummary: BuildOutcomeSummary(primaryTriggered.Outcome, triggeredRules),
                triggeredRules: triggeredRules,
                clausesInScope: clausesInScope,
                ctx: ctx,
                userId: userId
            );
        }

        // Fallback — should not reach here if seed data is correct (catch-all rule handles it)
        logger.LogWarning("No rules matched for BuildingType={Type}, Zone={Zone} — returning NoPermitRequired fallback",
            ctx.BuildingTypeSlug, ctx.ZoneCode);

        return BuildConclusiveResponse(
            outcome: RuleOutcome.NoPermitRequired,
            outcomeSummary: "Based on the information provided, a planning permit does not appear to be required. Please verify with Casey Council before commencing works.",
            triggeredRules: new List<TriggeredRuleDto>(),
            clausesInScope: clausesInScope,
            ctx: ctx,
            userId: userId
        );
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private EvaluationResponseDto BuildConclusiveResponse(
        RuleOutcome outcome,
        string outcomeSummary,
        List<TriggeredRuleDto> triggeredRules,
        List<PlanningClause> clausesInScope,
        EvaluationContext ctx,
        Guid? userId)
    {
        var assessmentId = Guid.NewGuid();

        // Fire-and-forget audit save (do not await — we don't want audit latency blocking the response)
        _ = SaveAuditAsync(assessmentId, outcome, outcomeSummary, triggeredRules, clausesInScope, ctx, userId);

        return new EvaluationResponseDto(
            Status: "Conclusive",
            Outcome: outcome.ToString(),
            OutcomeSummary: outcomeSummary,
            NextQuestions: null,
            TriggeredRules: triggeredRules,
            ClausesInScope: clausesInScope.Select(MapClauseToDto).ToList(),
            AssessmentId: assessmentId
        );
    }

    private async Task SaveAuditAsync(
        Guid assessmentId,
        RuleOutcome outcome,
        string outcomeSummary,
        List<TriggeredRuleDto> triggeredRules,
        List<PlanningClause> clausesInScope,
        EvaluationContext ctx,
        Guid? userId)
    {
        try
        {
            var assessment = new PermitAssessment(
                normalisedAddress: ctx.NormalisedAddress,
                latitude: ctx.Latitude,
                longitude: ctx.Longitude,
                zoneCode: ctx.ZoneCode,
                overlayCodes: string.Join(",", ctx.OverlayCodes),
                buildingTypeSlug: ctx.BuildingTypeSlug,
                answersJson: JsonSerializer.Serialize(ctx.Answers),
                outcome: outcome,
                outcomeReason: outcomeSummary,
                triggeredClauseNumbers: string.Join(",", triggeredRules.Select(t => t.Clause.ClauseNumber)),
                userId: userId
            );

            await repo.SaveAssessmentAsync(assessment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save permit assessment audit record");
        }
    }

    private static string BuildOutcomeSummary(RuleOutcome outcome, List<TriggeredRuleDto> triggeredRules)
    {
        // Use the reason from the first triggered rule as the primary summary
        var primary = triggeredRules.FirstOrDefault();
        return primary?.OutcomeReason ?? outcome.ToString();
    }

    private static ClauseDto MapClauseToDto(PlanningClause clause) =>
        new(clause.ClauseNumber, clause.Title, clause.Summary, clause.OfficialUrl);

    private static QuestionDto MapQuestionToDto(RuleQuestion question)
    {
        List<QuestionOptionDto>? options = null;
        QuestionValidationDto? validation = null;

        if (!string.IsNullOrEmpty(question.OptionsJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(question.OptionsJson);
                if (doc.RootElement.TryGetProperty("choices", out var choices))
                {
                    options = choices.EnumerateArray()
                        .Select(c => new QuestionOptionDto(
                            c.GetProperty("value").GetString() ?? "",
                            c.GetProperty("label").GetString() ?? ""))
                        .ToList();
                }
            }
            catch { /* malformed JSON — options remain null */ }
        }

        if (!string.IsNullOrEmpty(question.ValidationJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(question.ValidationJson);
                var root = doc.RootElement;
                validation = new QuestionValidationDto(
                    Min: root.TryGetProperty("min", out var minEl) ? minEl.GetDouble() : null,
                    Max: root.TryGetProperty("max", out var maxEl) ? maxEl.GetDouble() : null,
                    Unit: root.TryGetProperty("unit", out var unitEl) ? unitEl.GetString() : null,
                    DecimalPlaces: root.TryGetProperty("decimalPlaces", out var dpEl) ? dpEl.GetInt32() : null
                );
            }
            catch { /* malformed JSON — validation remains null */ }
        }

        return new QuestionDto(
            FieldKey: question.FieldKey,
            QuestionText: question.QuestionText,
            HelpText: question.HelpText,
            InputType: question.InputType.ToString(),
            Options: options,
            Validation: validation,
            DisplayOrder: question.DisplayOrder
        );
    }
}
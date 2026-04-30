using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CaseyHub.API.Evaluators;

/// <summary>
/// Interprets the TriggerContextJson from a PermitRule against the current EvaluationContext.
///
/// Supported JSONB schemas (all combinable):
///
/// Leaf conditions:
///   { "field": "height_m", "operator": "gt", "value": 2.0 }
///   { "field": "location", "operator": "eq", "value": "side_rear" }
///   { "field": "existing_dwelling_count", "operator": "gte", "value": 2 }
///
/// Zone/overlay conditions (tested against VicPlan data, not user answers):
///   { "zone_any": ["GRZ", "NRZ", "RGZ"] }     — zone code starts with any of these
///   { "zone_not_any": ["TRZ2", "UGZ"] }        — zone code does NOT start with any
///   { "overlay_any": ["HO", "BMO"] }            — any overlay code starts with any of these
///
/// Compound:
///   { "all": [ ...conditions ] }   — all must be true (AND)
///   { "any": [ ...conditions ] }   — at least one must be true (OR)
///
/// Catchall (used for no-permit catch-all rules):
///   { "catchall": true }           — always true
///
/// Operators: "gt", "gte", "lt", "lte", "eq", "neq"
/// </summary>
public interface IConditionEvaluator
{
    /// <summary>
    /// Evaluates the condition JSON against the context.
    /// Returns true if the condition is satisfied.
    /// Throws never — returns false on any parse or evaluation error and logs the fault.
    /// </summary>
    bool Evaluate(string triggerContextJson, EvaluationContext ctx);
}

public class ConditionEvaluator(ILogger<ConditionEvaluator> logger) : IConditionEvaluator
{
    public bool Evaluate(string triggerContextJson, EvaluationContext ctx)
    {
        try
        {
            using var doc = JsonDocument.Parse(triggerContextJson);
            return EvaluateNode(doc.RootElement, ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to evaluate condition JSON: {Json}", triggerContextJson);
            return false;
        }
    }

    private bool EvaluateNode(JsonElement node, EvaluationContext ctx)
    {
        // ── Catchall ───────────────────────────────────────────────────────────
        if (node.TryGetProperty("catchall", out _))
            return true;

        // ── Compound: all (AND) ────────────────────────────────────────────────
        if (node.TryGetProperty("all", out var allNode))
        {
            foreach (var child in allNode.EnumerateArray())
                if (!EvaluateNode(child, ctx)) return false;
            return true;
        }

        // ── Compound: any (OR) ────────────────────────────────────────────────
        if (node.TryGetProperty("any", out var anyNode))
        {
            foreach (var child in anyNode.EnumerateArray())
                if (EvaluateNode(child, ctx)) return true;
            return false;
        }

        // ── Zone: zone_any ────────────────────────────────────────────────────
        if (node.TryGetProperty("zone_any", out var zoneAny))
        {
            var codes = zoneAny.EnumerateArray().Select(e => e.GetString() ?? "").ToList();
            return codes.Any(code =>
                ctx.ZoneCode.StartsWith(code, StringComparison.OrdinalIgnoreCase));
        }

        // ── Zone: zone_not_any ────────────────────────────────────────────────
        if (node.TryGetProperty("zone_not_any", out var zoneNotAny))
        {
            var codes = zoneNotAny.EnumerateArray().Select(e => e.GetString() ?? "").ToList();
            return !codes.Any(code =>
                ctx.ZoneCode.StartsWith(code, StringComparison.OrdinalIgnoreCase));
        }

        // ── Overlay: overlay_any ──────────────────────────────────────────────
        if (node.TryGetProperty("overlay_any", out var overlayAny))
        {
            var codes = overlayAny.EnumerateArray().Select(e => e.GetString() ?? "").ToList();
            return ctx.OverlayCodes.Any(oc =>
                codes.Any(code => oc.StartsWith(code, StringComparison.OrdinalIgnoreCase)));
        }

        // ── Overlay: overlay_not_any ──────────────────────────────────────────
        if (node.TryGetProperty("overlay_not_any", out var overlayNotAny))
        {
            var codes = overlayNotAny.EnumerateArray().Select(e => e.GetString() ?? "").ToList();
            return !ctx.OverlayCodes.Any(oc =>
                codes.Any(code => oc.StartsWith(code, StringComparison.OrdinalIgnoreCase)));
        }

        // ── Field comparison ──────────────────────────────────────────────────
        if (node.TryGetProperty("field", out var fieldProp) &&
            node.TryGetProperty("operator", out var opProp) &&
            node.TryGetProperty("value", out var valueProp))
        {
            string fieldKey = fieldProp.GetString() ?? "";
            string op = opProp.GetString() ?? "";

            if (!ctx.Answers.TryGetValue(fieldKey, out object? rawAnswer))
            {
                // Field not yet answered — condition cannot be evaluated
                // The evaluator pipeline handles this upstream (checks for missing fields)
                // At this point, if we reach here it means the caller passed a context
                // with all required fields, so this should not happen.
                logger.LogWarning("Field '{FieldKey}' not found in answers during condition evaluation", fieldKey);
                return false;
            }

            return EvaluateFieldCondition(fieldKey, op, valueProp, rawAnswer);
        }

        logger.LogWarning("Unrecognised condition node: {Json}", node.ToString());
        return false;
    }

    private bool EvaluateFieldCondition(
        string fieldKey,
        string op,
        JsonElement expectedValue,
        object? rawAnswer)
    {
        // Boolean comparison
        if (expectedValue.ValueKind == JsonValueKind.True ||
            expectedValue.ValueKind == JsonValueKind.False)
        {
            bool expected = expectedValue.GetBoolean();
            bool actual = rawAnswer switch
            {
                bool b     => b,
                string s   => bool.TryParse(s, out bool b2) && b2,
                JsonElement je when je.ValueKind is JsonValueKind.True or JsonValueKind.False
                           => je.GetBoolean(),
                _          => false
            };
            return op == "eq" ? actual == expected : actual != expected;
        }

        // String equality
        if (expectedValue.ValueKind == JsonValueKind.String)
        {
            string expected = expectedValue.GetString() ?? "";
            string actual = rawAnswer switch
            {
                string s   => s,
                JsonElement je => je.GetString() ?? "",
                _          => rawAnswer?.ToString() ?? ""
            };
            return op switch
            {
                "eq"  => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
                "neq" => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
                _     => false
            };
        }

        // Numeric comparison
        if (expectedValue.ValueKind == JsonValueKind.Number)
        {
            double expected = expectedValue.GetDouble();
            double actual;

            try
            {
                actual = rawAnswer switch
                {
                    double d   => d,
                    int i      => (double)i,
                    long l     => (double)l,
                    float f    => (double)f,
                    decimal m  => (double)m,
                    string s   => double.Parse(s),
                    // System.Text.Json deserialises numbers as JsonElement when type is object
                    JsonElement je => je.GetDouble(),
                    _          => Convert.ToDouble(rawAnswer)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Cannot convert answer for field '{FieldKey}' to double. Raw: {Raw}",
                    fieldKey, rawAnswer);
                return false;
            }

            return op switch
            {
                "gt"  => actual > expected,
                "gte" => actual >= expected,
                "lt"  => actual < expected,
                "lte" => actual <= expected,
                "eq"  => Math.Abs(actual - expected) < 0.0001,
                "neq" => Math.Abs(actual - expected) >= 0.0001,
                _     => false
            };
        }

        logger.LogWarning("Unsupported expected value kind '{Kind}' for field '{Field}'",
            expectedValue.ValueKind, fieldKey);
        return false;
    }
}
using CaseyHub.Core.Entities;
using CaseyHub.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace CaseyHub.API.Data;

/// <summary>
/// Seeds all static planning data:
///   - PlanningClauses
///   - BuildingTypes
///   - ZoneOverrideRules (overlay short-circuits evaluated before questions)
///   - PermitRules + RuleQuestions (fence and second dwelling)
///
/// Called from the migration or from Program.cs after MigrateAsync().
/// Safe to call multiple times — checks existence before inserting.
/// </summary>
public static class PermitCheckerSeeder
{
    public static async Task SeedAsync(CaseyHubDbContext db)
    {
        // Skip if already seeded (idempotency guard)
        if (await db.BuildingTypes.AnyAsync()) return;

        // ══════════════════════════════════════════════════════════════════════
        // STEP 1 — Planning Clauses
        // Every clause referenced anywhere in the engine must live here first.
        // ══════════════════════════════════════════════════════════════════════
        var clauses = new List<PlanningClause>
        {
            // General provisions
            new("62.02-2",
                "Buildings and works not requiring a permit",
                "Lists buildings and works that sometimes require a permit depending on the specific planning scheme provisions. Fences fall under this clause — the scheme must be checked to confirm whether a permit is triggered.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/62"),

            // Front fence — ResCode Standard A20
            new("54.06-2",
                "Front fences — Standard A20",
                "A front fence within 3 metres of a street must not exceed the maximum height in the zone schedule. If no schedule height is specified, the default maximums from Table A2 apply (1.5m for most streets, 2.0m abutting Transport Zone 2).",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/54"),

            // Second dwelling ResCode front fence
            new("55.02-8",
                "Front fences (two or more dwellings) — Standard B32",
                "For developments with two or more dwellings, a front fence within 3 metres of a street must not exceed the maximum height specified in the zone schedule or Table B3.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/55"),

            // Bushfire Management Overlay
            new("44.06-2",
                "Bushfire Management Overlay — buildings and works",
                "The BMO requires a permit for buildings and works unless a specific exemption applies. It does not provide a blanket exemption for fences exceeding standard height limits.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/44"),

            // Heritage Overlay
            new("43.01-1",
                "Heritage Overlay — buildings and works",
                "A planning permit is required to construct a building or carry out works, including fencing, on land covered by a Heritage Overlay.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/43"),

            // Urban Growth Zone (Casey-specific)
            new("37.07",
                "Urban Growth Zone",
                "The Urban Growth Zone applies to greenfield land in Casey designated for future urban development. Permit requirements for buildings and works are set out in the UGZ schedule applicable to the land.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/37"),

            // General Residential Zone
            new("32.08",
                "General Residential Zone",
                "Applies to established residential areas in Casey. A permit is required to construct or extend a front fence within 3 metres of a street where the fence exceeds the Standard A20 height limits.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/32"),

            // Neighbourhood Residential Zone
            new("32.09",
                "Neighbourhood Residential Zone",
                "Applies to residential areas where neighbourhood character is to be maintained. Front fence height limits and permit triggers are the same as the General Residential Zone.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/32"),

            // Residential Growth Zone
            new("32.07",
                "Residential Growth Zone",
                "Applies to residential areas identified for increased housing density. Front fence height limits and permit triggers follow the standard ResCode provisions.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/32"),

            // Second dwelling — Clause 52.18 Small Second Dwelling
            new("52.18",
                "Small Second Dwelling",
                "Allows a small second dwelling on a residential lot as a Section 1 use (no permit required for use) in the General Residential Zone, Neighbourhood Residential Zone, Residential Growth Zone, Housing Choice and Transport Zone, Mixed Use Zone, and Township Zone — subject to conditions including lot size, setbacks, and infrastructure requirements.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/52"),

            // Clause 54 single dwelling code (second dwelling context)
            new("54",
                "One dwelling and small second dwelling — ResCode",
                "Sets out the residential development standards (ResCode) that apply to one dwelling and small second dwellings on lots less than 300 square metres. Standards include setbacks, site coverage, private open space, and overlooking.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/54"),

            // Clause 55 multi-dwelling
            new("55",
                "Two or more dwellings on a lot — ResCode",
                "Sets out the residential development standards for developments with two or more dwellings on a lot. A planning permit is required.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/55"),

            // Design and Development Overlay
            new("43.02",
                "Design and Development Overlay",
                "A DDO may impose specific height, setback, or design requirements for buildings and works in addition to zone provisions. Where a DDO applies, its schedule requirements take precedence.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/43"),

            // Significant Landscape Overlay
            new("42.03",
                "Significant Landscape Overlay",
                "A permit is required to construct a building or carry out works on land covered by an SLO. This includes fencing that may affect significant landscape values.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/42"),

            // Land Subject to Inundation Overlay
            new("44.04",
                "Land Subject to Inundation Overlay",
                "A permit is required to construct a building or carry out works, including solid fencing, on land subject to inundation. Solid fences can redirect or impede floodwaters.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/44"),

            // Activity Centre Zone (Casey)
            new("37.08",
                "Activity Centre Zone",
                "Applies to Casey's major activity centres. In the ACZ, a permit may be required to construct a front fence depending on the schedule. Second dwellings are permitted subject to Clause 55 standards.",
                "https://planning-schemes.app.planning.vic.gov.au/Casey/ordinance/37"),
        };

        await db.PlanningClauses.AddRangeAsync(clauses);
        await db.SaveChangesAsync();

        // Build a lookup so we can reference clause IDs by number below
        var clauseByNumber = await db.PlanningClauses
            .ToDictionaryAsync(c => c.ClauseNumber, c => c.Id);

        // ══════════════════════════════════════════════════════════════════════
        // STEP 2 — Building Types
        // ══════════════════════════════════════════════════════════════════════
        var fence = new BuildingType("fence", "Fence", "A boundary fence, front fence, or side/rear fence.", 1);
        var secondDwelling = new BuildingType("second_dwelling", "Second Dwelling",
            "A second home on the same lot as an existing dwelling (granny flat, secondary unit, small second dwelling).", 2);

        await db.BuildingTypes.AddRangeAsync(fence, secondDwelling);
        await db.SaveChangesAsync();

        int fenceId = fence.Id;
        int sdId = secondDwelling.Id;

        // ══════════════════════════════════════════════════════════════════════
        // STEP 3 — Zone Override Rules
        // Checked BEFORE any user questions. A match here ends the wizard immediately.
        // These cover overlays that unconditionally require a permit regardless of
        // building dimensions or location.
        // ══════════════════════════════════════════════════════════════════════
        var zoneOverrides = new List<ZoneOverrideRule>
        {
            // ── FENCE overrides ────────────────────────────────────────────────

            // Heritage Overlay — any HO code → permit required for ALL fencing
            new(fenceId, "HO", prefixMatch: true, RuleOutcome.PermitRequired,
                "Your property is within a Heritage Overlay (HO). Under Clause 43.01-1, a planning permit is required for all fencing works on heritage-overlaid land, regardless of fence height or location.",
                clauseByNumber["43.01-1"]),

            // Significant Landscape Overlay — permit required for fencing
            new(fenceId, "SLO", prefixMatch: true, RuleOutcome.PermitRequired,
                "Your property is within a Significant Landscape Overlay (SLO). Under Clause 42.03, a planning permit is required for buildings and works including fencing that may affect the significant landscape values.",
                clauseByNumber["42.03"]),

            // Land Subject to Inundation Overlay — solid fencing requires permit
            new(fenceId, "LSIO", prefixMatch: false, RuleOutcome.ReferToCouncil,
                "Your property is within a Land Subject to Inundation Overlay (LSIO). Under Clause 44.04, solid fencing that could impede floodwaters requires a planning permit. Please refer to Casey Council to confirm requirements for your fence type.",
                clauseByNumber["44.04"]),

            // Design and Development Overlay — refer to council (schedule-specific)
            new(fenceId, "DDO", prefixMatch: true, RuleOutcome.ReferToCouncil,
                "Your property is within a Design and Development Overlay (DDO). The DDO schedule may impose specific height or design requirements for fencing that override the standard ResCode limits. Please refer to the specific DDO schedule or contact Casey Council.",
                clauseByNumber["43.02"]),

            // ── SECOND DWELLING overrides ─────────────────────────────────────

            // Heritage Overlay — permit required, and it is more complex
            new(sdId, "HO", prefixMatch: true, RuleOutcome.PermitRequired,
                "Your property is within a Heritage Overlay (HO). Under Clause 43.01-1, a planning permit is required for all buildings and works on heritage-overlaid land. Second dwellings in an HO require a heritage impact assessment as part of the permit application.",
                clauseByNumber["43.01-1"]),

            // Land Subject to Inundation Overlay
            new(sdId, "LSIO", prefixMatch: false, RuleOutcome.PermitRequired,
                "Your property is within a Land Subject to Inundation Overlay (LSIO). Under Clause 44.04, a planning permit is required for buildings and works including second dwellings. Additional flood-level and drainage requirements will apply.",
                clauseByNumber["44.04"]),

            // Significant Landscape Overlay
            new(sdId, "SLO", prefixMatch: true, RuleOutcome.PermitRequired,
                "Your property is within a Significant Landscape Overlay (SLO). A planning permit is required for second dwellings on SLO-affected land. The application must address the relevant landscape character objectives.",
                clauseByNumber["42.03"]),
        };

        await db.ZoneOverrideRules.AddRangeAsync(zoneOverrides);
        await db.SaveChangesAsync();

        // ══════════════════════════════════════════════════════════════════════
        // STEP 4 — Permit Rules + Questions
        //
        // FENCE RULES
        // Decision tree (priority order):
        //
        // Priority 10: Is it a side/rear fence? → Clause 62.02-2 height check (2.0m)
        // Priority 20: Is it a front fence (within 3m of street)?
        //              Sub-check: Is street a Transport Zone 2 (major road)?
        //   Priority 21:  Zone IS TRZ2 → limit is 2.0m → Clause 54.06-2
        //   Priority 22:  Zone is NOT TRZ2 → limit is 1.5m → Clause 54.06-2
        // Priority 30: BMO zone — no additional fence-height exemption → informational
        //
        // SECOND DWELLING RULES
        // Priority 10: What is the zone?
        //   GRZ, NRZ, RGZ, MUZ, TZ → Clause 52.18 eligibility check
        //   UGZ → always permit required (Clause 37.07)
        //   ACZ → always permit required (Clause 37.08)
        //   Other → refer to council
        // Priority 20: Lot size check — < 300 sqm → Clause 54 applies
        // Priority 30: Lot size check — ≥ 300 sqm → building permit only (no planning permit)
        // Priority 40: Second dwelling already exists on lot? → refer
        // Priority 50: Is natural gas connected? (52.18 condition) → permit required
        // ══════════════════════════════════════════════════════════════════════

        // ── FENCE RULE 1: Side/rear fence height > 2.0m ──────────────────────
        // Applies under Clause 62.02-2 across all residential zones.
        var fenceRule_SideRearHeight = new PermitRule(
            buildingTypeId: fenceId,
            ruleType: RuleType.ConditionalCheck,
            priority: 10,
            triggerContextJson: """
            {
              "all": [
                { "field": "location", "operator": "eq", "value": "side_rear" },
                { "field": "height_m", "operator": "gt", "value": 2.0 }
              ]
            }
            """,
            outcome: RuleOutcome.PermitRequired,
            outcomeReason: "The proposed fence height exceeds 2.0 metres on a side or rear boundary. Under Clause 62.02-2, a fence that exceeds 2.0m in height on a side or rear boundary requires a planning permit.",
            planningClauseId: clauseByNumber["62.02-2"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(fenceRule_SideRearHeight);
        await db.SaveChangesAsync();

        // Questions for Rule 1 — need location AND height
        await db.RuleQuestions.AddRangeAsync(
            new RuleQuestion(
                permitRuleId: fenceRule_SideRearHeight.Id,
                fieldKey: "location",
                questionText: "Where will the fence be located on your property?",
                helpText: "A 'front fence' is any fence within 3 metres of the street boundary.",
                inputType: QuestionInputType.SingleSelect,
                optionsJson: """
                {
                  "choices": [
                    { "value": "front", "label": "Front (within 3m of the street)" },
                    { "value": "side_rear", "label": "Side or rear boundary" }
                  ]
                }
                """,
                validationJson: null,
                displayOrder: 1
            ),
            new RuleQuestion(
                permitRuleId: fenceRule_SideRearHeight.Id,
                fieldKey: "height_m",
                questionText: "What is the proposed fence height?",
                helpText: "Measure from natural ground level to the top of the fence. Enter in metres (e.g. 1.8 for 1.8m).",
                inputType: QuestionInputType.Number,
                optionsJson: null,
                validationJson: """{ "min": 0.1, "max": 10.0, "unit": "m", "decimalPlaces": 2 }""",
                displayOrder: 2
            )
        );

        // ── FENCE RULE 2: Front fence height > 2.0m abutting Transport Zone 2 ─
        // When the street is a declared road (TRZ2), the higher limit of 2.0m applies.
        var fenceRule_FrontTRZ2Height = new PermitRule(
            buildingTypeId: fenceId,
            ruleType: RuleType.ConditionalCheck,
            priority: 20,
            triggerContextJson: """
            {
              "all": [
                { "field": "location", "operator": "eq", "value": "front" },
                { "zone_any": ["TRZ2"] },
                { "field": "height_m", "operator": "gt", "value": 2.0 }
              ]
            }
            """,
            outcome: RuleOutcome.PermitRequired,
            outcomeReason: "The proposed front fence height exceeds 2.0 metres. On land abutting a Transport Zone 2 (major declared road), the maximum front fence height under Standard A20 (Clause 54.06-2) is 2.0 metres. A planning permit is required.",
            planningClauseId: clauseByNumber["54.06-2"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(fenceRule_FrontTRZ2Height);
        await db.SaveChangesAsync();

        // Questions for Rule 2 — same location + height fields (will be deduplicated)
        await db.RuleQuestions.AddRangeAsync(
            new RuleQuestion(fenceRule_FrontTRZ2Height.Id, "location",
                "Where will the fence be located on your property?",
                "A 'front fence' is any fence within 3 metres of the street boundary.",
                QuestionInputType.SingleSelect,
                """{"choices":[{"value":"front","label":"Front (within 3m of the street)"},{"value":"side_rear","label":"Side or rear boundary"}]}""",
                null, 1),
            new RuleQuestion(fenceRule_FrontTRZ2Height.Id, "height_m",
                "What is the proposed fence height?",
                "Measure from natural ground level to the top of the fence. Enter in metres.",
                QuestionInputType.Number, null,
                """{ "min": 0.1, "max": 10.0, "unit": "m", "decimalPlaces": 2 }""",
                2)
        );

        // ── FENCE RULE 3: Front fence height > 1.5m (non-TRZ2 streets) ────────
        // Standard A20 default — most streets in Casey.
        var fenceRule_FrontStandardHeight = new PermitRule(
            buildingTypeId: fenceId,
            ruleType: RuleType.ConditionalCheck,
            priority: 21,
            triggerContextJson: """
            {
              "all": [
                { "field": "location", "operator": "eq", "value": "front" },
                { "zone_not_any": ["TRZ2"] },
                { "field": "height_m", "operator": "gt", "value": 1.5 }
              ]
            }
            """,
            outcome: RuleOutcome.PermitRequired,
            outcomeReason: "The proposed front fence height exceeds 1.5 metres. On most streets in Casey (not abutting a Transport Zone 2), the maximum front fence height under Standard A20 (Clause 54.06-2) is 1.5 metres. A planning permit is required.",
            planningClauseId: clauseByNumber["54.06-2"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(fenceRule_FrontStandardHeight);
        await db.SaveChangesAsync();

        await db.RuleQuestions.AddRangeAsync(
            new RuleQuestion(fenceRule_FrontStandardHeight.Id, "location",
                "Where will the fence be located on your property?",
                "A 'front fence' is any fence within 3 metres of the street boundary.",
                QuestionInputType.SingleSelect,
                """{"choices":[{"value":"front","label":"Front (within 3m of the street)"},{"value":"side_rear","label":"Side or rear boundary"}]}""",
                null, 1),
            new RuleQuestion(fenceRule_FrontStandardHeight.Id, "height_m",
                "What is the proposed fence height?",
                "Measure from natural ground level to the top of the fence. Enter in metres.",
                QuestionInputType.Number, null,
                """{ "min": 0.1, "max": 10.0, "unit": "m", "decimalPlaces": 2 }""",
                2)
        );

        // ── FENCE RULE 4: No permit required — within all limits ───────────────
        // This rule fires as the final catch-all when NO prior rule matched.
        // Priority 99 — evaluated last.
        var fenceRule_NoneRequired = new PermitRule(
            buildingTypeId: fenceId,
            ruleType: RuleType.ConditionalCheck,
            priority: 99,
            triggerContextJson: """{ "catchall": true }""",
            outcome: RuleOutcome.NoPermitRequired,
            outcomeReason: "Based on the information provided, a planning permit is not required for this fence. The proposed fence is within the height limits under Clause 62.02-2 and Standard A20 (Clause 54.06-2) for your zone and street type. Note: A building permit may still be required — check with a registered building surveyor.",
            planningClauseId: clauseByNumber["62.02-2"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(fenceRule_NoneRequired);
        await db.SaveChangesAsync();

        // Rule 4 requires the same questions to be answered before it can be evaluated.
        await db.RuleQuestions.AddRangeAsync(
            new RuleQuestion(fenceRule_NoneRequired.Id, "location",
                "Where will the fence be located on your property?",
                "A 'front fence' is any fence within 3 metres of the street boundary.",
                QuestionInputType.SingleSelect,
                """{"choices":[{"value":"front","label":"Front (within 3m of the street)"},{"value":"side_rear","label":"Side or rear boundary"}]}""",
                null, 1),
            new RuleQuestion(fenceRule_NoneRequired.Id, "height_m",
                "What is the proposed fence height?",
                "Measure from natural ground level to the top of the fence. Enter in metres.",
                QuestionInputType.Number, null,
                """{ "min": 0.1, "max": 10.0, "unit": "m", "decimalPlaces": 2 }""",
                2)
        );

        // ══════════════════════════════════════════════════════════════════════
        // SECOND DWELLING RULES
        // ══════════════════════════════════════════════════════════════════════

        // ── SD RULE 1: Urban Growth Zone → always permit required ─────────────
        // UGZ land in Casey requires a permit for all second dwellings.
        var sdRule_UGZ = new PermitRule(
            buildingTypeId: sdId,
            ruleType: RuleType.ZoneOverlayCheck,
            priority: 5,
            triggerContextJson: """{ "zone_any": ["UGZ"] }""",
            outcome: RuleOutcome.PermitRequired,
            outcomeReason: "Your property is in the Urban Growth Zone (UGZ). All second dwellings in the UGZ require a planning permit under Clause 37.07. The application must comply with the relevant UGZ schedule and any applicable Precinct Structure Plan.",
            planningClauseId: clauseByNumber["37.07"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(sdRule_UGZ);
        await db.SaveChangesAsync();
        // No questions needed — zone is known before any questions are asked

        // ── SD RULE 2: Activity Centre Zone → permit required ──────────────────
        var sdRule_ACZ = new PermitRule(
            buildingTypeId: sdId,
            ruleType: RuleType.ZoneOverlayCheck,
            priority: 6,
            triggerContextJson: """{ "zone_any": ["ACZ"] }""",
            outcome: RuleOutcome.PermitRequired,
            outcomeReason: "Your property is in the Activity Centre Zone (ACZ). A planning permit is required for second dwellings in the ACZ. The application must comply with Clause 37.08 and the applicable ACZ schedule.",
            planningClauseId: clauseByNumber["37.08"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(sdRule_ACZ);
        await db.SaveChangesAsync();

        // ── SD RULE 3: Residential zone + lot < 300 sqm → permit required ─────
        // On lots under 300m² in residential zones, Clause 54 applies and a permit is required.
        var sdRule_SmallLot = new PermitRule(
            buildingTypeId: sdId,
            ruleType: RuleType.ConditionalCheck,
            priority: 10,
            triggerContextJson: """
            {
              "all": [
                { "zone_any": ["GRZ", "NRZ", "RGZ", "MUZ", "TZ", "HCTZ"] },
                { "field": "lot_size_sqm", "operator": "lt", "value": 300 }
              ]
            }
            """,
            outcome: RuleOutcome.PermitRequired,
            outcomeReason: "Your lot is under 300 square metres in a residential zone. Under Clause 54, a planning permit is required to construct a small second dwelling on a lot less than 300 square metres. The application must meet the ResCode standards in Clause 54.",
            planningClauseId: clauseByNumber["54"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(sdRule_SmallLot);
        await db.SaveChangesAsync();

        await db.RuleQuestions.AddAsync(
            new RuleQuestion(sdRule_SmallLot.Id, "lot_size_sqm",
                "What is the total area of your lot?",
                "You can find this on your property's title documents or a recent rates notice. Enter in square metres (e.g. 450 for 450m²).",
                QuestionInputType.Number, null,
                """{ "min": 1, "max": 100000, "unit": "m²", "decimalPlaces": 0 }""",
                1)
        );

        // ── SD RULE 4: Residential zone + lot ≥ 300 sqm + Clause 52.18 applies ─
        // On lots 300m²+ in residential zones, a Small Second Dwelling can be built
        // as a Section 1 use (no planning permit for USE) but buildings/works may
        // still need a permit depending on conditions.
        var sdRule_52_18_Check = new PermitRule(
            buildingTypeId: sdId,
            ruleType: RuleType.ConditionalCheck,
            priority: 20,
            triggerContextJson: """
            {
              "all": [
                { "zone_any": ["GRZ", "NRZ", "RGZ", "MUZ", "TZ", "HCTZ"] },
                { "field": "lot_size_sqm", "operator": "gte", "value": 300 },
                { "field": "existing_dwelling_count", "operator": "eq", "value": 1 }
              ]
            }
            """,
            outcome: RuleOutcome.NoPermitRequired,
            outcomeReason: "Your property may be eligible for a Small Second Dwelling under Clause 52.18 without a planning permit for the use. However, a building permit is required for the construction works. The dwelling must meet all Clause 52.18 conditions: it must be the only second dwelling on the lot, reticulated natural gas must not be connected to it, and it must comply with any applicable zone schedule requirements. Verify with Casey Council before commencing.",
            planningClauseId: clauseByNumber["52.18"],
            shortCircuitOnMatch: false // Don't short-circuit — Rule 5 (gas) must also be checked
        );

        await db.PermitRules.AddAsync(sdRule_52_18_Check);
        await db.SaveChangesAsync();

        await db.RuleQuestions.AddRangeAsync(
            new RuleQuestion(sdRule_52_18_Check.Id, "lot_size_sqm",
                "What is the total area of your lot?",
                "You can find this on your property title documents or rates notice.",
                QuestionInputType.Number, null,
                """{ "min": 1, "max": 100000, "unit": "m²", "decimalPlaces": 0 }""",
                1),
            new RuleQuestion(sdRule_52_18_Check.Id, "existing_dwelling_count",
                "How many dwellings currently exist on this lot?",
                "Count the number of separate dwellings already on your property.",
                QuestionInputType.SingleSelect,
                """{"choices":[{"value":"0","label":"None"},{"value":"1","label":"One"},{"value":"2","label":"Two or more"}]}""",
                null, 2)
        );

        // ── SD RULE 5: Natural gas connected → permit required (52.18 condition) ─
        // Clause 52.18 specifically prohibits natural gas connection to a small second dwelling.
        // If gas IS connected, the no-permit path is no longer available.
        var sdRule_GasConnected = new PermitRule(
            buildingTypeId: sdId,
            ruleType: RuleType.ConditionalCheck,
            priority: 25,
            triggerContextJson: """
            {
              "all": [
                { "zone_any": ["GRZ", "NRZ", "RGZ", "MUZ", "TZ", "HCTZ"] },
                { "field": "lot_size_sqm", "operator": "gte", "value": 300 },
                { "field": "natural_gas_connected", "operator": "eq", "value": true }
              ]
            }
            """,
            outcome: RuleOutcome.PermitRequired,
            outcomeReason: "Reticulated natural gas is connected to the property. Under Clause 52.18, a Small Second Dwelling cannot have reticulated natural gas connected to it. If the second dwelling is to be connected to gas, a planning permit is required. Alternatively, the second dwelling may proceed without a planning permit if it has no gas connection.",
            planningClauseId: clauseByNumber["52.18"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(sdRule_GasConnected);
        await db.SaveChangesAsync();

        await db.RuleQuestions.AddRangeAsync(
            new RuleQuestion(sdRule_GasConnected.Id, "lot_size_sqm",
                "What is the total area of your lot?",
                "You can find this on your property title documents or rates notice.",
                QuestionInputType.Number, null,
                """{ "min": 1, "max": 100000, "unit": "m²", "decimalPlaces": 0 }""",
                1),
            new RuleQuestion(sdRule_GasConnected.Id, "natural_gas_connected",
                "Is reticulated natural gas connected to the property?",
                "Check your utility bills or contact your gas retailer if unsure.",
                QuestionInputType.Boolean, null, null, 3)
        );

        // ── SD RULE 6: Two or more existing dwellings → permit required ─────────
        // Clause 55 (not 52.18) applies when adding a dwelling where 2+ already exist.
        var sdRule_MultipleDwellings = new PermitRule(
            buildingTypeId: sdId,
            ruleType: RuleType.ConditionalCheck,
            priority: 30,
            triggerContextJson: """
            {
              "field": "existing_dwelling_count",
              "operator": "gte",
              "value": 2
            }
            """,
            outcome: RuleOutcome.PermitRequired,
            outcomeReason: "There are already two or more dwellings on this lot. A planning permit is required under Clause 55 (two or more dwellings on a lot). The application must meet the full ResCode standards in Clause 55.",
            planningClauseId: clauseByNumber["55"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(sdRule_MultipleDwellings);
        await db.SaveChangesAsync();

        await db.RuleQuestions.AddAsync(
            new RuleQuestion(sdRule_MultipleDwellings.Id, "existing_dwelling_count",
                "How many dwellings currently exist on this lot?",
                "Count the number of separate dwellings already on your property.",
                QuestionInputType.SingleSelect,
                """{"choices":[{"value":"0","label":"None"},{"value":"1","label":"One"},{"value":"2","label":"Two or more"}]}""",
                null, 1)
        );

        // ── SD RULE 7: Unknown/industrial/commercial zone → refer to council ─────
        var sdRule_UnknownZone = new PermitRule(
            buildingTypeId: sdId,
            ruleType: RuleType.ZoneOverlayCheck,
            priority: 98,
            triggerContextJson: """{ "zone_not_any": ["GRZ", "NRZ", "RGZ", "MUZ", "TZ", "HCTZ", "UGZ", "ACZ"] }""",
            outcome: RuleOutcome.ReferToCouncil,
            outcomeReason: "The zone of your property is not one of the standard residential zones where second dwelling rules are straightforward. Please contact Casey Council's Planning Department to confirm whether a second dwelling is permissible on your property and what approvals are required.",
            planningClauseId: clauseByNumber["37.07"],
            shortCircuitOnMatch: true
        );

        await db.PermitRules.AddAsync(sdRule_UnknownZone);
        await db.SaveChangesAsync();
        // No questions for this rule — zone is known

        await db.SaveChangesAsync();
    }
}
import { create } from "zustand";
import { ClauseDto, EvaluationOutcome, QuestionDto, TriggeredRuleDto } from "@/src/types/permitchecker/responses";


interface PermitCheckerState {
  sessionId: string | null;
  normalisedAddress: string | null;
  latitude: number | null;
  longitude: number | null;
  zoneCode: string | null;
  zoneDescription: string | null;
  overlayCodes: string[];
  buildingTypeSlug: string | null;
  buildingTypeDisplayName: string | null;
  answers: Record<string, unknown>;
  clausesInScope: ClauseDto[];
  outcome: EvaluationOutcome | null;
  outcomeSummary: string | null;
  triggeredRules: TriggeredRuleDto[] | null;
  assessmentId: string | null;
}


interface PermitCheckerActions {
  setAddressLookupResult: (result: {
    sessionId: string;
    normalisedAddress: string;
    latitude: number;
    longitude: number;
    zoneCode: string;
    zoneDescription: string;
    overlayCodes: string[];
    clausesInScope: ClauseDto[];
  }) => void;

  setBuildingType: (slug: string, displayName: string) => void;

  mergeAnswers: (newAnswers: Record<string, unknown>) => void;

  setClausesInScope: (clauses: ClauseDto[]) => void;

  setVerdict: (result: {
    outcome: EvaluationOutcome;
    outcomeSummary: string;
    triggeredRules: TriggeredRuleDto[];
    clausesInScope: ClauseDto[];
    assessmentId: string | null;
  }) => void;

  reset: () => void;
}

// Initial state

const initialState: PermitCheckerState = {
  sessionId: null,
  normalisedAddress: null,
  latitude: null,
  longitude: null,
  zoneCode: null,
  zoneDescription: null,
  overlayCodes: [],
  buildingTypeSlug: null,
  buildingTypeDisplayName: null,
  answers: {},
  clausesInScope: [],
  outcome: null,
  outcomeSummary: null,
  triggeredRules: null,
  assessmentId: null,
};


export const usePermitCheckerStore = create<PermitCheckerState & PermitCheckerActions>()(
  (set) => ({
    ...initialState,

    setAddressLookupResult: (result) =>
      set({
        sessionId: result.sessionId,
        normalisedAddress: result.normalisedAddress,
        latitude: result.latitude,
        longitude: result.longitude,
        zoneCode: result.zoneCode,
        zoneDescription: result.zoneDescription,
        overlayCodes: result.overlayCodes,
        clausesInScope: result.clausesInScope,
      }),

    setBuildingType: (slug, displayName) =>
      set({ buildingTypeSlug: slug, buildingTypeDisplayName: displayName }),

    mergeAnswers: (newAnswers) =>
      set((state) => ({ answers: { ...state.answers, ...newAnswers } })),

    setClausesInScope: (clauses) => set({ clausesInScope: clauses }),

    setVerdict: (result) =>
      set({
        outcome: result.outcome,
        outcomeSummary: result.outcomeSummary,
        triggeredRules: result.triggeredRules,
        clausesInScope: result.clausesInScope,
        assessmentId: result.assessmentId,
      }),

    reset: () => set(initialState),
  })
);
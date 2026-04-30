
export interface ClauseDto {
  clauseNumber: string;
  title: string;
  summary: string | null;
  officialUrl: string | null;
}
 
export interface BuildingTypeDto {
  id: number;
  slug: string;
  displayName: string;
  description: string | null;
  displayOrder: number;
}
 
export interface QuestionOptionDto {
  value: string;
  label: string;
}
 
export interface QuestionValidationDto {
  min: number | null;
  max: number | null;
  unit: string | null;
  decimalPlaces: number | null;
}
 
export interface QuestionDto {
  fieldKey: string;
  questionText: string;
  helpText: string | null;
  inputType: "Number" | "SingleSelect" | "MultiSelect" | "Boolean";
  options: QuestionOptionDto[] | null;
  validation: QuestionValidationDto | null;
  displayOrder: number;
}
 
export interface TriggeredRuleDto {
  ruleId: number;
  outcomeReason: string;
  clause: ClauseDto;
}
 
export interface AddressLookupResponseDto {
  sessionId: string;
  normalisedAddress: string;
  latitude: number;
  longitude: number;
  zoneCode: string;
  zoneDescription: string;
  overlayCodes: string[];
  relevantClauses: ClauseDto[];
}
 
export type EvaluationStatus = "NeedsMoreInfo" | "Conclusive";
 
export type EvaluationOutcome =
  | "PermitRequired"
  | "NoPermitRequired"
  | "Exempt"
  | "ReferToCouncil";
 
export interface EvaluationResponseDto {
  status: EvaluationStatus;
  outcome: EvaluationOutcome | null;
  outcomeSummary: string | null;
  nextQuestions: QuestionDto[] | null;
  triggeredRules: TriggeredRuleDto[] | null;
  clausesInScope: ClauseDto[];
  assessmentId: string | null;
}
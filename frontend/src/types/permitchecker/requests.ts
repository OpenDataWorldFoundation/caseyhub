export interface AddressLookupRequestDto {
  address: string;
}
 
export interface EvaluationRequestDto {
  sessionId: string;
  normalisedAddress: string;
  latitude: number;
  longitude: number;
  zoneCode: string;
  overlayCodes: string[];
  buildingTypeSlug: string;
  answers: Record<string, unknown>;
}
 
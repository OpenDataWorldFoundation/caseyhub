import { BACKEND_BASE_URL } from "@/src/constants/api";
import { AddressLookupResponseDto, BuildingTypeDto, EvaluationResponseDto } from "../types/permitchecker/responses";
import { AddressLookupRequestDto, EvaluationRequestDto } from "../types/permitchecker/requests";
import { authFetch } from "../utils/authFetch";


const BASE = `${BACKEND_BASE_URL}/permit-checker`;

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `HTTP ${response.status}`);
  }
  return response.json() as Promise<T>;
}

export async function fetchBuildingTypes(token: string | null): Promise<BuildingTypeDto[]> {
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const response = await fetch(`${BASE}/building-types`, { headers });
  return handleResponse<BuildingTypeDto[]>(response);
}

export async function lookupAddress(payload: AddressLookupRequestDto,token: string | null): Promise<AddressLookupResponseDto> {
  
  // const headers: Record<string, string> = { "Content-Type": "application/json" };
  // if (token) headers["Authorization"] = `Bearer ${token}`;
  const response = await authFetch(`/permit-checker/address`, token, {method: "POST", body: JSON.stringify(payload)});
  return handleResponse<AddressLookupResponseDto>(response);
}

export async function evaluatePermit(
  payload: EvaluationRequestDto,
  token: string | null
): Promise<EvaluationResponseDto> {
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const response = await fetch(`${BASE}/evaluate`, {
    method: "POST",
    headers,
    body: JSON.stringify(payload),
  });
  return handleResponse<EvaluationResponseDto>(response);
}
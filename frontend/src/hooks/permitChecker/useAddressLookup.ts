import { useAuth } from "@/src/context/authContext";
import { lookupAddress } from "@/src/services/PermitCheckerService";
import { AddressLookupRequestDto } from "@/src/types/permitchecker/requests";
import { AddressLookupResponseDto } from "@/src/types/permitchecker/responses";
import { useQuery } from "@tanstack/react-query";

// Uses enabled:false + refetch() pattern — matches useGetPermitsNearAddress in this codebase.
// The address is submitted imperatively on form submit, not reactively on state change.

export const useAddressLookup = (address: string) => {
  const { userToken } = useAuth();

  const payload: AddressLookupRequestDto = { address };

  return useQuery<AddressLookupResponseDto, Error>({
    queryKey: ["permitChecker", "addressLookup", address],
    queryFn: () => lookupAddress(payload, userToken),
    enabled: false,
    retry: false,
  });
};
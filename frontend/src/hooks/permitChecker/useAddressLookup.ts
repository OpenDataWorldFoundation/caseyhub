import { useAuth } from "@/src/context/authContext";
import { lookupAddress } from "@/src/services/PermitCheckerService";
import { AddressLookupRequestDto } from "@/src/types/permitchecker/requests";
import { AddressLookupResponseDto } from "@/src/types/permitchecker/responses";
import { useQuery } from "@tanstack/react-query";

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
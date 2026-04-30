import { useAuth } from "@/src/context/authContext";
import { evaluatePermit } from "@/src/services/PermitCheckerService";
import { EvaluationRequestDto } from "@/src/types/permitchecker/requests";
import { EvaluationResponseDto } from "@/src/types/permitchecker/responses";
import { useMutation } from "@tanstack/react-query";

export const useEvaluatePermit = () => {
  const { userToken } = useAuth();

  return useMutation<EvaluationResponseDto, Error, EvaluationRequestDto>({
    mutationFn: (payload: EvaluationRequestDto) => evaluatePermit(payload, userToken),
  });
};
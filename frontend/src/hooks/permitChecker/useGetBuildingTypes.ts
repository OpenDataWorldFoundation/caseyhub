import { useAuth } from "@/src/context/authContext";
import { fetchBuildingTypes } from "@/src/services/PermitCheckerService";
import { BuildingTypeDto } from "@/src/types/permitchecker/responses";
import { useQuery } from "@tanstack/react-query";

export const useGetBuildingTypes = () => {
  const { userToken } = useAuth();

  return useQuery<BuildingTypeDto[], Error>({
    queryKey: ["permitChecker", "buildingTypes"],
    queryFn: () => fetchBuildingTypes(userToken),
    staleTime: 1000 * 60 * 10, // Building types rarely change — 10 min cache
  });
};
import { useAuth } from "@/src/context/authContext";
import { Permit } from "@/src/types/types";
import { authFetch } from "@/src/utils/authFetch";
import { useQuery } from "@tanstack/react-query";

const fetchPermit = async (appNumber: string, userToken: string | null): Promise<Permit> => {
    
    const response = await authFetch(`/Permits/getPermitByAppNumber/${encodeURIComponent(appNumber)}`, userToken);

    if(!response.ok){
        throw new Error ('Network response was not OK');
    }
    return response.json();
}

export const useCheckPermitExists = (appNumber: string) => {
    const {userToken} = useAuth();
    return useQuery<Permit, Error>({
        queryKey: ['permits', 'byAppNumber', appNumber], // same key as useGetPermitByAppNumber
        queryFn: () => fetchPermit(appNumber, userToken),
        enabled: false,  // only fires on refetch()
        retry: false,
    });
};
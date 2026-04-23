import { Permit } from "@/src/types";
import { useQuery } from "@tanstack/react-query";

const fetchPermit = async (appNumber: string): Promise<Permit> => {
    const response = await fetch(`http://localhost:8080/api/Permits/getPermitByAppNumber/${encodeURIComponent(appNumber)}`);

    if(!response.ok){
        throw new Error ('Network response was not OK');
    }
    return response.json();
}

export const useCheckPermitExists = (appNumber: string) => {
    return useQuery<Permit, Error>({
        queryKey: ['permits', 'byAppNumber', appNumber], // same key as useGetPermitByAppNumber
        queryFn: () => fetchPermit(appNumber),
        enabled: false,  // only fires on refetch()
        retry: false,
    });
};
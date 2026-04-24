import { Permit } from '@/src/types';
import {useQuery} from '@tanstack/react-query'
import { buildApiUrl } from '@/src/lib/api';

const fetchPermit = async (appNumber: string): Promise<Permit> => {
    const response = await fetch(buildApiUrl(`/api/Permits/getPermitByAppNumber/${encodeURIComponent(appNumber)}`));

    if(!response.ok){
        throw new Error ('Network response was not OK');
    }
    return response.json();
}

export const useGetPermitByAppNumber = (appNumber: string, options = {}) => {
    return useQuery<Permit, Error> ({
        queryKey: ['applicationNumber', appNumber],
        queryFn: () => fetchPermit(appNumber),
        ...options,
    });
};

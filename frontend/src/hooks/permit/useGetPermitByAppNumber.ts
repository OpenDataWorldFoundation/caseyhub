import { BACKEND_BASE_URL } from '@/src/constants/api';
import { useAuth } from '@/src/context/authContext';
import { Permit } from '@/src/types';
import { authFetch } from '@/src/utils/authFetch';
import {useQuery} from '@tanstack/react-query'

const fetchPermit = async (appNumber: string, userToken: string | null): Promise<Permit> => {
    const response = await authFetch(`/permits/getPermitByAppNumber/${encodeURIComponent(appNumber)}`, userToken);

    if(!response.ok){
        throw new Error ('Network response was not OK');
    }
    return response.json();
}

export const useGetPermitByAppNumber = (appNumber: string, options = {}) => {
    const {userToken} = useAuth();
    return useQuery<Permit, Error> ({
        queryKey: ['permits', 'byAppNumber', appNumber],
        queryFn: () => fetchPermit(appNumber, userToken),
        enabled: !!appNumber
    });
};
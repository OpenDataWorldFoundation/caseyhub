import { useAuth } from "@/src/context/authContext";
import { Permit } from "@/src/types/types";
import { authFetch } from "@/src/utils/authFetch"
import { useQuery } from "@tanstack/react-query";

const fetchUserPermits = async (token:string) => {
    const response = await authFetch('/Permits/getUserSavedPermits', token);
    if (!response.ok){
        throw new Error ("Response was NOT OK");
    }
    return response.json();
}

export const useGetUserPermits = () => {
    const {userToken} = useAuth();
    return useQuery<Permit[], Error> ({
        queryKey: ["user", "savedpermits", userToken],
        queryFn: async () =>{
            if(!userToken){
                throw new Error ("No User Token")
            }  
            return fetchUserPermits(userToken)
        }, 
        enabled: !!userToken
    })
}